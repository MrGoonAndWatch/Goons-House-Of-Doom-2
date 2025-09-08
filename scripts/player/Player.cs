using Godot;
using System;
using static GameConstants;

public partial class Player : ICutsceneActor
{
    [Export]
    private PauseScreenUi _pauseScreenUi;
    [Export]
    private RayCast3D _hitscanRay;
    [Export]
    private PlayerInventory _playerInventory;
    [Export]
    private Control _deathFadeUi;
    [Export]
    private ColorRect _deathFadeBg;
    [Export]
    private SaveGame _saveUi;

    private Camera3D _camera;

    const float SPEED = 50.0f;
	const float RUN_MODIFIER = 3.0f;
	const float BACKWARDS_MODIFIER = 0.5f;

	const float QUICK_TURN_DURATION = 0.25f;
	const float ROTATION_SPEED = 2.0f;
    const float DEATH_FADEOUT_TIMEOUT = 5.0f;

    private bool IsQuickTurning;
    private bool IsDying;
    private double _deathFadeoutTimeLeft;

    private bool _holdAnalogueDirection;
    private float _analogueControlCurrentCameraRotation;

    private float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() / 100;

    private PlayerStatus _playerStatus;
    private PlayerAnimationControl _playerAnimationControl;

    private const float NOCLIP_SPEED_BONUS = 5.0f;

    public override void _Ready()
    {
        // TODO: this sucks, get the camera in a more elegant way.
        _camera = GetNode<Camera3D>(GameConstants.NodePaths.FromSceneRoot.Camera);

        _playerStatus = PlayerStatus.GetInstance();
        _playerAnimationControl = PlayerAnimationControl.GetInstance();
        RefreshNoClip();
    }

    public override void _Process(double delta)
    {
        HandlePauseMenu();
        HandleAiming();
        HandleShooting();
        HandleDeath(delta);
    }

    private void HandlePauseMenu()
    {
        if (Input.IsActionJustPressed(Controls.pause.ToString()) && !DebugManager.IsDebugConsoleActive())
        {
            if (_playerStatus.Paused)
            {
                _playerStatus.Paused = _pauseScreenUi.OnPauseMenuClosed();
            }
            else if(_playerStatus.CanPause())
            {
                _playerStatus.Paused = true;
                _pauseScreenUi.OnPauseMenuOpened();
            }
        }
    }

    private void HandleAiming()
    {
        if (_playerStatus.EquipedWeapon == null) return;

        if (_playerStatus.CanAim() && Input.IsActionPressed(Controls.aim.ToString()))
        {
            _playerStatus.Aiming = true;
            _playerStatus.ReadyToShoot = false;
            _playerAnimationControl.StartAiming();
        }
        else if (!_playerStatus.HasAnyUiOpen() && _playerStatus.Aiming && !Input.IsActionPressed(Controls.aim.ToString()))
            EndAiming();
    }

    public void RefreshNoClip()
    {
        var noClipping = DebugManager.IsPlayerNoClipping();
        CollisionMask = noClipping ? (uint)0 : 1;
    }

    public void OnShootingReady()
    {
        _playerStatus.ReadyToShoot = true;
    }

    public void EndAiming()
    {
        _playerStatus.Aiming = false;
        _playerAnimationControl.StopAiming();
    }

    private void HandleShooting()
    {
        if (_playerStatus.CanShoot() && _playerStatus.Aiming && Input.IsActionJustPressed(Controls.confirm.ToString()))
            _playerStatus.EquipedWeapon.ShootWeapon(_playerInventory, _hitscanRay, _playerAnimationControl);
    }

    public void OnShootingEnded()
    {
        _playerAnimationControl.OnShootingEnded();
        _playerStatus.ReadyToShoot = true;
    }

    private void HandleDeath(double delta)
    {
        if (!IsDying) return;

        _deathFadeoutTimeLeft -= delta;
        UpdateDeathFade();

        if (_deathFadeoutTimeLeft <= 0)
            FinishDying();
    }

    private void UpdateDeathFade()
    {
        var newAlpha = (DEATH_FADEOUT_TIMEOUT - _deathFadeoutTimeLeft) / DEATH_FADEOUT_TIMEOUT;
        var newColor = new Color(1, 0, 0, (float)newAlpha);
        _deathFadeBg.Color = newColor;
    }

    private void FinishDying()
    {
        AttemptToWipeSaveData();
        SceneChanger.GetInstance().ChangeSceneDirectly(SceneChanger.GameOverScreen);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Note: override for when we're moving in a cutscene per cutscene instructions.
        if (_currentTargetPosition.HasValue && !_isCutscenePaused) {
            MoveAndSlide();
            return;
        }

        var velocity = Velocity;
		velocity = ProcessGravity(delta, velocity);

        (var input_dir, var analogue) = GameConstants.GetMovementVectorWithDeadzone();
        velocity = ProcessMovement(delta, velocity, input_dir, analogue);
        ProcessRotation(delta, input_dir, analogue);

		Velocity = velocity;

		MoveAndSlide();

        //GD.Print($"Rotation = {Rotation} , RotationDegrees = {RotationDegrees}");
        _playerStatus.UpdatePlayerPosition(Position, RotationDegrees.Y);
    }

	private Vector3 ProcessGravity(double delta, Vector3 velocity)
	{
        if (!DebugManager.IsPlayerNoClipping() && !IsOnFloor())
            velocity.Y -= (float)(Gravity * delta);
        return velocity;
	}

    private void ResetAnalogueMovement()
    {
        _holdAnalogueDirection = false;
        _analogueControlCurrentCameraRotation = 0;
    }

    private Vector3 ProcessMovement(double delta, Vector3 velocity, Vector2 inputMovement, bool analogue)
    {
        if (_playerStatus.IsMovementPrevented())
        {
            _playerAnimationControl.StopMoving();
            ResetAnalogueMovement();
            return new Vector3(0, velocity.Y, 0);
        }

        if (((analogue && inputMovement != Vector2.Zero) || inputMovement.Y != 0) && !IsQuickTurning)
        {
            var running = Input.IsActionPressed(Controls.run.ToString());

            var runMod = 1.0f;

            if (running && (inputMovement.Y < 0 || analogue))
                runMod = RUN_MODIFIER;

            var backwardsMod = 1.0f;

            if (!analogue && inputMovement.Y > 0)
                backwardsMod = BACKWARDS_MODIFIER;

            var noclipMod = DebugManager.IsPlayerNoClipping() ? NOCLIP_SPEED_BONUS : 1.0f;
            var speedMod = DebugManager.GetSpeedMod();

            if (!analogue && inputMovement.Y > 0 && Input.IsActionJustPressed(Controls.run.ToString()))
                StartQuickTurn();

            if (!_holdAnalogueDirection)
            {
                _analogueControlCurrentCameraRotation = _camera.Rotation.Y;
                _holdAnalogueDirection = true;
            }

            var movement = analogue ?
                // Analogue controls (i.e. move the direction you pressed relative to camera)
                new Vector3(inputMovement.X, 0.0f, inputMovement.Y).Rotated(Vector3.Up, _analogueControlCurrentCameraRotation) * ((float)delta) * SPEED * runMod * noclipMod * speedMod :
                // Tank controls (i.e. left/right = rotate, up/down = forwards/backwards)
                -(Transform.Basis.X * inputMovement.Y * (float)delta) * SPEED * runMod * backwardsMod * noclipMod * speedMod;

            velocity.X = movement.X;
            velocity.Z = movement.Z;

            // TODO: Don't spam call this, or maybe put logic in animation control to not redundantly call?
            if (running)
                _playerAnimationControl.StartRunning();
            else
                _playerAnimationControl.StartWalking();
        }
        else
        {
            ResetAnalogueMovement();
            velocity = new Vector3(0, velocity.Y, 0);
            _playerAnimationControl.StopMoving();
        }
        return velocity;
    }

    private void ProcessRotation(double delta, Vector2 inputMovement, bool analogue)
    {
        if (_playerStatus.IsRotationPrevented()) return;

        if (analogue)
        {
            var newDir = new Vector3(inputMovement.X, 0.0f, inputMovement.Y).Rotated(Vector3.Up, _analogueControlCurrentCameraRotation);
            LookAt(GlobalPosition + newDir);
            RotateY(1.570796f);
        }
        else
        {
            var noclipFactor = DebugManager.IsPlayerNoClipping() ? NOCLIP_SPEED_BONUS : 1;
            if (inputMovement.X != 0 && !IsQuickTurning)
                RotateY(inputMovement.X * ROTATION_SPEED * (float)delta * -1 * noclipFactor);
        }
    }

    private void StartQuickTurn() {
        if (IsQuickTurning)
            return;


        var tween = CreateTween();

        tween.TweenProperty(this, "rotation:y", Rotation.Y + Mathf.Pi, QUICK_TURN_DURATION);

        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenCallback(Callable.From(EndQuickTurn));

        IsQuickTurning = true;
    }

    private void EndQuickTurn() {
        IsQuickTurning = false;
    }

    public override void SetAnimationFlag(string flagName, Variant value)
    {
        _playerAnimationControl.SetAnimationVariable(flagName, value);
    }

    public void DebugTweenToAngle(float angle, Action callback)
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "rotation:y", angle, 0.25f);
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenCallback(Callable.From(callback));
    }

    public RayCast3D GetHitscan()
    {
        return _hitscanRay;
    }

    public void StartDeath()
    {
        _playerAnimationControl.GenericDeath();

        // TODO: remove this line when we have real death animations playing!
        OnDeathAnimationFinished("");
    }

    public void OnDeathAnimationFinished(string animationName)
    {
        // TODO: Eventually we should load in different game over screens based on which animation finished (or set a different visual in the game over screen or whatever makes sense implementation-wise).
        //         Whenever that happens, use the method param to set something in the state or wherever to help determine what scene to load/visuals to load.

        _deathFadeoutTimeLeft = DEATH_FADEOUT_TIMEOUT;
        UpdateDeathFade();
        _deathFadeUi.Visible = true;
        IsDying = true;
    }

    private void AttemptToWipeSaveData()
    {
        if (_playerStatus.GameSettings == null)
        {
            GD.PrintErr("Unable to perform RNG chance to delete save files on death. No 'GameSettings' were found under the current PlayerStatus object. Game will assume 'Funny Mode' is turned off and thus not run this check!");
            return;
        }

        if (!_playerStatus.GameSettings.FunnyMode) return;

        GD.Print("Rolling dice to see if save data should be wiped.");
        var randomNumber = GD.Randf();
        if (randomNumber < FunnyModeProbabilities.ChanceToWipeSaveFileOnDeath)
        {
            GD.Print("Actually attempting to wipe save data...");
            _saveUi.DeleteAllSaveFilesInPlaythrough(_playerStatus.GameSettings.PlaythroughId);
            // TODO: Taunt the player that we wiped all their saves (maybe with something on the game over screen?).
        }
    }
}

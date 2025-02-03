using Godot;
using System;
using static GameConstants;

public partial class Player : ICutsceneActor
{
    [Export]
    private AnimationTree _tree;
    [Export]
    private PauseScreenUi _pauseScreenUi;
    [Export]
    private RayCast3D _hitscanRay;

    const float SPEED = 50.0f;
	const float RUN_MODIFIER = 3.0f;
	const float BACKWARDS_MODIFIER = 0.5f;

	const float QUICK_TURN_DURATION = 0.25f;
	const float ROTATION_SPEED = 2.0f;

    private bool IsQuickTurning;

    private float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() / 100;

    private PlayerStatus _playerStatus;

    private bool _noClipping;
    private const float NOCLIP_SPEED_BONUS = 5.0f;

    public override void _Ready()
    {
        _playerStatus = PlayerStatus.GetInstance();
    }

    public override void _Process(double delta)
    {
        HandlePauseMenu();
        HandleAiming();
        HandleShooting();
        HandleDebugInput();
    }

    private void HandlePauseMenu()
    {
        if (Input.IsActionJustPressed(Controls.pause.ToString()))
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
            _tree.Set(GameConstants.Animation.Player.Aiming, true);
        }
        else if (!_playerStatus.HasAnyUiOpen() && _playerStatus.Aiming && !Input.IsActionPressed(Controls.aim.ToString()))
            EndAiming();
    }

    private void HandleDebugInput()
    {
        if (!DataSaver.IsDebugBuild()) return;

        if (Input.IsActionJustPressed(Controls.debug_noclip.ToString()))
            ToggleNoClip();
    }

    private void ToggleNoClip()
    {
        _noClipping = !_noClipping;
        CollisionMask = _noClipping ? (uint)0 : 1;
    }

    public void WeaponEquipped(Weapon weapon)
    {
        if (weapon == null) return;

        _tree.Set(weapon.GetEquipAnimationName(), true);
    }

    public void WeaponUnequipped(Weapon weapon)
    {
        if (weapon == null) return;
        EndAiming();
        _tree.Set(weapon.GetEquipAnimationName(), false);
    }

    public void OnShootingReady()
    {
        _playerStatus.ReadyToShoot = true;
    }

    private void EndAiming()
    {
        _playerStatus.Aiming = false;
        _tree.Set(GameConstants.Animation.Player.Aiming, false);
    }

    private void HandleShooting()
    {
        if (_playerStatus.CanShoot() && _playerStatus.Aiming && Input.IsActionJustPressed(Controls.confirm.ToString()))
        {
            if (_playerStatus.WeaponHasAmmo())
            {
                _playerStatus.ReadyToShoot = false;
                _tree.Set(GameConstants.Animation.Player.Fire, true);
                _playerStatus.EquipedWeapon.PlaySfx();
                // NOTE: Can subtract more than 1 here if we need to, probably would need to add another property to the weapon class/implementation.
                _playerStatus.EquipedWeapon.AddAmmo();
                var inv = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
                inv.SetAllDirty();
                if (_playerStatus.EquipedWeapon.IsHitscan())
                {
                    var collider = _hitscanRay.GetCollider();
                    if(collider is Enemy)
                    {
                        (collider as Enemy).TakeDamage(_playerStatus.EquipedWeapon.GetDamagePerHit());
                    }
                    //else if(collider is Node)
                    //{
                    //    GD.Print($"Didn't hit enemy, hit '{(collider as Node).Name}' instead.");
                    //}
                    //else
                    //{
                    //    GD.Print($"Hit nothing? Collider==null={collider == null}, collider.type={collider?.GetType()}");
                    //}
                }
                else
                {
                    // TODO: Handle knives and other non-hitscan weapons!
                }
            }
            else if (_playerStatus.HasAmmoInInventory())
            {
                // TODO: Play reload animation.
                _playerStatus.AddAmmoToCurrentWeaponFromInventory();
            }
            else
            {
                // TODO: Play no ammo click.
            }
        }
    }

    public void OnShootingEnded()
    {
        _tree.Set(GameConstants.Animation.Player.Fire, false);
        _playerStatus.ReadyToShoot = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Note: override for when we're moving in a cutscene per cutscene instructions.
        if (_currentTargetPosition.HasValue) {
            MoveAndSlide();
            return;
        }

        var velocity = Velocity;
		velocity = ProcessGravity(delta, velocity);

        var input_dir = GameConstants.GetMovementVectorWithDeadzone();
        velocity = ProcessMovement(delta, velocity, input_dir.Y);
        ProcessRotation(delta, input_dir.X);
		Velocity = velocity;

		MoveAndSlide();

        //GD.Print($"Rotation = {Rotation} , RotationDegrees = {RotationDegrees}");
        _playerStatus.UpdatePlayerPosition(Position, RotationDegrees.Y);
    }

	private Vector3 ProcessGravity(double delta, Vector3 velocity)
	{
        if (!_noClipping && !IsOnFloor())
            velocity.Y -= (float)(Gravity * delta);
        return velocity;
	}

    private Vector3 ProcessMovement(double delta, Vector3 velocity, float inputMovement)
    {
        if (_playerStatus.IsMovementPrevented())
        {
            _tree.Set(GameConstants.Animation.Player.Walking, false);
            return new Vector3(0, velocity.Y, 0);
        }

        if (inputMovement != 0 && !IsQuickTurning)
        {
            var running = Input.IsActionPressed(Controls.run.ToString());

            var runMod = 1.0f;

            if (running && inputMovement < 0)
                runMod = RUN_MODIFIER;

            var backwardsMod = 1.0f;

            if (inputMovement > 0)
                backwardsMod = BACKWARDS_MODIFIER;

            var noclipMod = _noClipping ? NOCLIP_SPEED_BONUS : 1.0f;

            if (inputMovement > 0 && Input.IsActionJustPressed(Controls.run.ToString()))
                StartQuickTurn();

            var movement = -(Transform.Basis.X * inputMovement * (float)delta) * SPEED * runMod * backwardsMod * noclipMod;

            velocity.X = movement.X;
            velocity.Z = movement.Z;

            _tree.Set(GameConstants.Animation.Player.Walking, true);
        }
        else
        {
            velocity = new Vector3(0, velocity.Y, 0);
            _tree.Set(GameConstants.Animation.Player.Walking, false);
        }
        return velocity;
    }

    private void ProcessRotation(double delta, float inputRotation)
    {
        if (_playerStatus.IsRotationPrevented()) return;

        var noclipFactor = _noClipping ? NOCLIP_SPEED_BONUS : 1;
        if (inputRotation != 0 && !IsQuickTurning)
            RotateY(inputRotation * ROTATION_SPEED * (float)delta * -1 * noclipFactor);
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
        _tree.Set(flagName, value);
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
}

using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export]
    private AnimationTree _tree;

    const float SPEED = 50.0f;
	const float RUN_MODIFIER = 3.0f;
	const float BACKWARDS_MODIFIER = 0.5f;

	const float QUICK_TURN_DURATION = 0.25f;
	const float ROTATION_SPEED = 2.0f;

    private bool IsQuickTurning;

    private float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() / 100;

    private PlayerStatus _playerStatus;

    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("pause"))
            GetTree().Quit();
    }

    public override void _PhysicsProcess(double delta)
    {
        _playerStatus = PlayerStatus.GetInstance();
        var velocity = Velocity;
		velocity = ProcessGravity(delta, velocity);
		velocity = ProcessMovement(delta, velocity);
		Velocity = velocity;

		MoveAndSlide();
    }

	private Vector3 ProcessGravity(double delta, Vector3 velocity)
	{
        if (!IsOnFloor())
            velocity.Y -= (float)(Gravity * delta);
        return velocity;
	}

    private Vector3 ProcessMovement(double delta, Vector3 velocity)
    {
        if (_playerStatus.IsMovementPrevented()) return new Vector3(0, velocity.Y, 0);

        var input_dir = Input.GetVector(GameConstants.Controls.left.ToString(), GameConstants.Controls.right.ToString(), GameConstants.Controls.up.ToString(), GameConstants.Controls.down.ToString());

        var inputRotation = input_dir.X;

        var inputMovement = input_dir.Y;

        if (inputRotation != 0 && !IsQuickTurning)
            RotateY(inputRotation * ROTATION_SPEED * (float)delta * -1);

        if (inputMovement != 0 && !IsQuickTurning)
        {
            var running = Input.IsActionPressed(GameConstants.Controls.run.ToString());

            var runMod = 1.0f;

            if (running && inputMovement < 0)
                runMod = RUN_MODIFIER;

            var backwardsMod = 1.0f;

            if (inputMovement > 0)
                backwardsMod = BACKWARDS_MODIFIER;


            if (inputMovement > 0 && Input.IsActionJustPressed(GameConstants.Controls.run.ToString()))
                StartQuickTurn();

            var movement = -(Transform.Basis.X * inputMovement * (float)delta) * SPEED * runMod * backwardsMod;

            velocity.X = movement.X;
            velocity.Z = movement.Z;

            _tree.Set(GameConstants.Animation.Player.Idle, false);
            _tree.Set(GameConstants.Animation.Player.Walking, true);
        }
        else
        {
            velocity = new Vector3(0, velocity.Y, 0);
            _tree.Set(GameConstants.Animation.Player.Walking, false);
            _tree.Set(GameConstants.Animation.Player.Idle, true);
        }
        return velocity;
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
}

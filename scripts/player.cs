using Godot;
using System;

public partial class Player : CharacterBody3D
{

	const float SPEED = 50.0f;
	const float RUN_MODIFIER = 3.0f;
	const float BACKWARDS_MODIFIER = 0.5f;

	const float QUICK_TURN_DURATION = 0.25f;
	const float ROTATION_SPEED = 2.0f;

    private bool IsQuickTurning;

    private float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() / 100;

    public override void _PhysicsProcess(double delta)
    {
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
        var input_dir = Input.GetVector("left", "right", "up", "down");


        var inputRotation = input_dir.X;

        var inputMovement = input_dir.Y;

        if (inputRotation != 0 && !IsQuickTurning)
            RotateY(inputRotation * ROTATION_SPEED * (float)delta * -1);

        if (inputMovement != 0 && !IsQuickTurning)
        {
            var running = Input.IsActionPressed("run");

            var runMod = 1.0f;

            if (running && inputMovement < 0)
                runMod = RUN_MODIFIER;

            var backwardsMod = 1.0f;

            if (inputMovement > 0)
                backwardsMod = BACKWARDS_MODIFIER;


            if (inputMovement > 0 && Input.IsActionJustPressed("run"))
                StartQuickTurn();

            var movement = -(Transform.Basis.X * inputMovement * (float)delta) * SPEED * runMod * backwardsMod;

            velocity.X = movement.X;
            velocity.Z = movement.Z;
        }
        else
            velocity = new Vector3(0, velocity.Y, 0);
        return velocity;
    }

    private void StartQuickTurn() {
        if (IsQuickTurning)
            return;


        var tween = CreateTween();

        tween.TweenProperty(this, "rotation:y", Rotation.Y + Math.PI, QUICK_TURN_DURATION);

        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenCallback(Callable.From(EndQuickTurn));

        IsQuickTurning = true;
    }

    private void EndQuickTurn() {
        IsQuickTurning = false;
    }
}

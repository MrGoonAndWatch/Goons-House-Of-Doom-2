using Godot;
using System;

public partial class player : CharacterBody3D
{

	private float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		var velocity = Velocity;
		ProcessGravity(delta, velocity);
		ProcessMovement(delta, velocity);
		Velocity = velocity;

		MoveAndSlide();
	}

	private void ProcessGravity(double delta, Vector3 velocity)
	{
		if(!IsOnFloor()) {
			velocity.Y += (float) (Gravity * delta);
			GD.Print($"Processed gravity! {Gravity * delta}");
		}
		else
			GD.Print("Nothing happened!");
	}

	private void ProcessMovement(double delta, Vector3 velocity)
	{
	}
}

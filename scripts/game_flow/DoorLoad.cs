using Godot;
using System;

public partial class DoorLoad : Node3D
{
	private float OpeningTime = 1.0f;
	private float _timeElapsed;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		_timeElapsed += (float)delta;

        // TODO: eventually we'll trigger an animation in the _Ready function and listen for the finished event instead of just hard coding a rotation here.
		var openedPercent = Math.Min(_timeElapsed, OpeningTime) / OpeningTime;
        RotationDegrees = new Vector3(0, openedPercent * 90, 0);

		// NOTE: Leave half of this condition here when cleaning up the above code!!!
		if (openedPercent >= 1.0 || Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
			ChangeScene();
    }

	private void ChangeScene()
	{
		var sceneChanger = SceneChanger.GetInstance();
        sceneChanger.FinishSceneLoad();
    }
}

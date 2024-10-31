using Godot;
using System;

public partial class DoorLoad : Node3D
{
	[Export]
	private float OpeningTime = 1.0f;
	[Export]
	private Node3D[] DoorsToRotate;
	[Export]
	private bool FlipRotationEveryOther;
	
	private float _timeElapsed;
	private Vector3 _initialRotation;

	public override void _Ready()
	{
		_initialRotation = RotationDegrees;
    }

	public override void _Process(double delta)
	{
		var openedPercent = RotateDoors(delta);

		// NOTE: Leave half of this condition here when cleaning up the above code!!!
		if (openedPercent >= 1.0 || Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
			ChangeScene();
    }

	private float RotateDoors(double delta)
	{
        _timeElapsed += (float)delta;
        // TODO: eventually we'll trigger an animation in the _Ready function and listen for the finished event instead of just hard coding a rotation here.
        var openedPercent = Math.Min(_timeElapsed, OpeningTime) / OpeningTime;
		for (var i = 0; i < DoorsToRotate.Length; i++)
		{
			var reverse = 1;
			if (FlipRotationEveryOther && i % 2 == 1)
				reverse = -1;
			DoorsToRotate[i].RotationDegrees = _initialRotation + new Vector3(0, openedPercent * 90 * reverse, 0);
		}

		return openedPercent;
    }

	private void ChangeScene()
	{
		var sceneChanger = SceneChanger.GetInstance();
        sceneChanger.FinishSceneLoad();
    }
}

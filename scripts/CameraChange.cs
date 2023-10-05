using Godot;

public partial class CameraChange : Area3D
{
	[Export]
	private Camera3D TargetCamera;

	public void OnBodyEntered(Node3D body)
	{
		if (TargetCamera == null)
			return;

		if (body.IsInGroup("player"))
		{
			GD.Print($"changing camera to {TargetCamera.Name}");
			TargetCamera.Current = true;
		}
	}
}

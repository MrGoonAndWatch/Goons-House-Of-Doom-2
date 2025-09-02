using Godot;

public partial class CameraChange : Area3D
{
	[Export]
	private Camera3D TargetCamera;

	[Export]
	private Node3D TargetCameraPos;

	public override void _Ready()
	{
		if (TargetCamera == null)
			TargetCamera = GetNode<Camera3D>(GameConstants.NodePaths.FromSceneRoot.Camera);
	}
	
	public void OnBodyEntered(Node3D body)
	{
		if (TargetCameraPos == null || TargetCamera == null)
			return;

		if (body.IsInGroup("player"))
		{
			//GD.Print($"changing camera to {TargetCameraPos.Name}");
			if (PlayerStatus.CanChangeCameraAngle())
			{
				TargetCamera.GlobalPosition = TargetCameraPos.GlobalPosition;
				TargetCamera.GlobalRotation = TargetCameraPos.GlobalRotation;
			}
			else
				PlayerStatus.StoreCameraPositioning(TargetCameraPos.GlobalPosition, TargetCameraPos.GlobalRotation);
		}
	}
}

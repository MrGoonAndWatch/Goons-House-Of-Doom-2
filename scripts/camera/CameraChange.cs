using Godot;

public partial class CameraChange : Area3D
{
	[Export]
	private Camera3D TargetCamera;

	[Export]
	private CameraPosition TargetCameraPos;

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
				PlayerStatus.ChangeCamera(TargetCameraPos, TargetCamera);
			else
				PlayerStatus.StoreCameraPositioning(TargetCameraPos);
		}
	}
}

using Godot;
using static GameConstants;

public abstract partial class Teleporter : Node3D
{
    [Export]
    public int DoorId;

    [Export]
    private string GoesToRoom;
    [Export]
    private DoorLoadType DoorLoadType;
    [Export]
    private Vector3 StartAtPosition;
    [Export(PropertyHint.Range, "-360,360")]
    private Vector3 StartAtAngle;

    protected void ActivateTeleporter()
    {
        var mapStatus = MapStatus.GetInstance();
        mapStatus.EnterDoor(DoorId);
        var sceneChanger = SceneChanger.GetInstance();
        var sceneChangeInfo = new SceneLoadData
        {
            TargetScene = GoesToRoom,
            LoadPosition = StartAtPosition,
            LoadRotation = StartAtAngle,
        };
        sceneChanger.ChangeScene(sceneChangeInfo, DoorLoadType);
    }
}

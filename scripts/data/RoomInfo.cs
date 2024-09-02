using Godot;

public partial class RoomInfo : Node
{
    [Export]
    public string RoomName;
    [Export]
    public int RoomId;

    [Export]
    public Door[] doors;

    public override void _Ready()
    {
        var mapStatus = MapStatus.GetInstance();
        mapStatus.VisitRoom(RoomId);

        for (int i = 0; i < doors.Length; i++)
            mapStatus.FoundDoor(doors[i].DoorId);
    }
}

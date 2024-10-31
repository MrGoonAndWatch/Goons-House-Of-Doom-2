using Godot;

public partial class RoomInfo : Node
{
    /// <summary>
    /// Name of the room to use in save files.
    /// </summary>
    [Export]
    public string RoomName;
    /// <summary>
    /// A unique number for this room. Using duplicate values across different rooms will cause data errors!
    /// </summary>
    [Export]
    public int RoomId;
    /// <summary>
    /// References to every door in the current room, used for marking their status on the map.
    /// </summary>
    [Export]
    public Teleporter[] doors;

    public override void _Ready()
    {
        var mapStatus = MapStatus.GetInstance();
        mapStatus.VisitRoom(RoomId);

        for (int i = 0; i < doors.Length; i++)
            mapStatus.FoundDoor(doors[i].DoorId);

        MapStatus.CheckForRoomCleared();
    }
}

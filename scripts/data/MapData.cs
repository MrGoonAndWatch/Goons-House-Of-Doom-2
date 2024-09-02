using Godot;

public partial class MapData : Control
{
    [Export]
    public int AreaId;
    [Export]
    public string AreaName;
    [Export]
    public MapRoomData[] RoomData;
    [Export]
    public MapDoorData[] DoorData;

    [Export]
    private Label AreaLabel;

    public override void _Ready()
    {
        AreaLabel.Text = AreaName;
        RefreshMap();
    }

    public void RefreshMap()
    {
        var mapStatus = MapStatus.GetInstance();
        var hasMap = mapStatus.HasMap(AreaId);

        for (var i = 0; i < RoomData.Length; i++)
        {
            RoomData[i].PlayerVisitedRoom = mapStatus.VisitedRoom(RoomData[i].RoomId);
            RoomData[i].PlayerClearedRoom = mapStatus.ClearedRoom(RoomData[i].RoomId);
            RoomData[i].UpdateStatus(hasMap, RoomData[i].RoomId == GameConstants.GetCurrentRoomId(this));
        }

        for (var i = 0; i < DoorData.Length; i++)
        {
            var doorStatus = GameConstants.DoorMapStatus.Unseen;
            if (mapStatus.EnteredDoor(DoorData[i].DoorId))
                doorStatus = GameConstants.DoorMapStatus.Opened;
            else if (mapStatus.LockedDoorFound(DoorData[i].DoorId))
                doorStatus = GameConstants.DoorMapStatus.Locked;
            else if (hasMap || mapStatus.HasSeenDoor(DoorData[i].DoorId))
                doorStatus = GameConstants.DoorMapStatus.Unknown;
            DoorData[i].UpdateStatus(doorStatus);

            GD.Print($"Set Door {DoorData[i].DoorId} to Status {doorStatus}");
        }
    }

    public MapRoomData GetRoom(int roomId)
    {
        for(var i = 0; i < RoomData.Length; i++)
        {
            if (roomId == RoomData[i].RoomId)
                return RoomData[i];
        }

        GD.PrintErr($"Couldn't find roomId '{roomId}' in area '{AreaName} ({AreaId})'");
        return null;
    }
}

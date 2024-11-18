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
    [Export]
    private TextureRect PlayerIcon;

    private MapRoomData _currentRoom;

    public override void _Ready()
    {
        AreaLabel.Text = AreaName;
        RefreshMap();
    }

    public override void _Process(double delta)
    {
        MovePlayerIcon(delta);
    }

    private void MovePlayerIcon(double delta)
    {
        PlayerIcon.Visible = _currentRoom != null;
        if (_currentRoom == null) return;

        var playerStatus = PlayerStatus.GetInstance();
        var playerPos = playerStatus.GetPlayerPosition();
        var scaledPlayerPos = new Vector2((playerPos.X + _currentRoom.RoomOriginOffset.X) * _currentRoom.PlayerMapPositionScale.X, (playerPos.Z + _currentRoom.RoomOriginOffset.Y) * _currentRoom.PlayerMapPositionScale.Y);
        PlayerIcon.Position = _currentRoom.Position + scaledPlayerPos;
        PlayerIcon.RotationDegrees = (-playerStatus.GetPlayerAngle()) + 90;
    }

    public void SetCurrentRoom(int roomId)
    {
        _currentRoom = null;
        for (var i = 0; i < RoomData.Length; i++)
        {
            if (RoomData[i]?.RoomId == roomId)
            {
                _currentRoom = RoomData[i];
            }
        }
    }

    public void RefreshMap()
    {
        var mapStatus = MapStatus.GetInstance();
        var hasMap = mapStatus.HasMap(AreaId);

        for (var i = 0; i < RoomData.Length; i++)
        {
            if (RoomData[i] == null) continue;
            RoomData[i].PlayerVisitedRoom = mapStatus.VisitedRoom(RoomData[i].RoomId);
            RoomData[i].PlayerClearedRoom = mapStatus.ClearedRoom(RoomData[i].RoomId);
            RoomData[i].UpdateStatus(hasMap, RoomData[i].RoomId == GameConstants.GetCurrentRoomId(this));
        }

        for (var i = 0; i < DoorData.Length; i++)
        {
            var doorData = DoorData[i];
            if(doorData == null) continue;

            var doorStatus = GameConstants.DoorMapStatus.Unseen;
            if (mapStatus.EnteredDoor(doorData.DoorId))
                doorStatus = GameConstants.DoorMapStatus.Opened;
            else if (mapStatus.LockedDoorFound(doorData.DoorId))
                doorStatus = GameConstants.DoorMapStatus.Locked;
            else if (hasMap || mapStatus.HasSeenDoor(doorData.DoorId))
                doorStatus = GameConstants.DoorMapStatus.Unknown;
            doorData.UpdateStatus(doorStatus);

            //GD.Print($"Set Door {doorData.DoorId} to Status {doorStatus}");
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

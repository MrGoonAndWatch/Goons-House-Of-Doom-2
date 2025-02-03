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

    private Vector2 _playerIconCenteringOffset;

    public override void _Ready()
    {
        AreaLabel.Text = AreaName;
        _playerIconCenteringOffset = PlayerIcon.Size / 2;
        GD.Print($"_playerIconCenteringOffset={_playerIconCenteringOffset}");
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
        var isSideways = _currentRoom.RoomOrientation == GameConstants.RoomOrientation.Rotated90Degrees || _currentRoom.RoomOrientation == GameConstants.RoomOrientation.Rotated270Degrees;
        var playerX = isSideways ? playerPos.Z : playerPos.X;
        var playerY = isSideways ? playerPos.X : playerPos.Z;
        var flipX = _currentRoom.RoomOrientation == GameConstants.RoomOrientation.Rotated270Degrees;
        var flipY = _currentRoom.RoomOrientation == GameConstants.RoomOrientation.Rotated90Degrees || _currentRoom.RoomOrientation == GameConstants.RoomOrientation.Rotated180Degrees;
        var backwardsRoomMultiplier = new Vector2(flipX ? -1 : 1, flipY ? -1 : 1);
        float roomRotation = (int)_currentRoom.RoomOrientation;

        var scaledPlayerPos = new Vector2((playerX + _currentRoom.RoomOriginOffset.X) * _currentRoom.PlayerMapPositionScale.X, 
            (playerY + _currentRoom.RoomOriginOffset.Y) * _currentRoom.PlayerMapPositionScale.Y) 
            * backwardsRoomMultiplier + _playerIconCenteringOffset;
        PlayerIcon.Position = _currentRoom.Position + scaledPlayerPos;
        PlayerIcon.RotationDegrees = (-(playerStatus.GetPlayerAngle() + roomRotation)) + 90;

        //GD.Print($"PlayerMapPosUpdate: playerPos={playerPos} | isSideways={isSideways} | isBackwards={isBackwards} | playerX={playerX} | playerY={playerY} | backwardsRoomMultiplier={backwardsRoomMultiplier} | roomRotation={roomRotation} | scaledPlayerPos={scaledPlayerPos} | PlayerIcon.Position={PlayerIcon.Position} | PlayerIcon.RotationDegrees={PlayerIcon.RotationDegrees}");
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

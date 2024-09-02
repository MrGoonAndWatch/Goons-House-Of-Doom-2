using Godot;

public partial class MapData : Control
{
    [Export]
    public int AreaId;
    [Export]
    public string AreaName;
    [Export]
    public bool PlayerHasMap;
    [Export]
    public MapRoomData[] RoomData;

    [Export]
    private Label AreaLabel;

    public override void _Ready()
    {
        AreaLabel.Text = AreaName;
        RefreshMap();
    }

    public void RefreshMap()
    {
        for (var i = 0; i < RoomData.Length; i++)
            RoomData[i].UpdateStatus(PlayerHasMap);
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

using Godot;

public partial class MapRoomData : Control
{
    [Export]
    public string RoomName;
    [Export]
    public int RoomId;

    [Export]
    private Label RoomLabel;
    [Export]
    private ColorRect RoomStatus;

    public bool PlayerVisitedRoom;
    public bool PlayerClearedRoom;

    private static Color UnvisitedRoomColor = new Color(0.5f, 0.5f, 0.5f);
    private static Color ClearedRoomColor = new Color(0.0f, 1.0f, 0.5f);
    private static Color VisitedRoomColor = new Color(0.0f, 0.5f, 1.0f);

    public override void _Ready()
    {
        RoomLabel.Text = RoomName;
    }

    public void UpdateStatus(bool hasMap)
    {
        Visible = hasMap || PlayerVisitedRoom;

        var currentStatusColor = UnvisitedRoomColor;
        if (PlayerClearedRoom)
            currentStatusColor = ClearedRoomColor;
        else if (PlayerVisitedRoom)
            currentStatusColor = VisitedRoomColor;

        RoomStatus.Color = currentStatusColor;
    }
}

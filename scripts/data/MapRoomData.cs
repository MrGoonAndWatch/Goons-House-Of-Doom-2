using Godot;

public partial class MapRoomData : Control
{
    [Export]
    public string RoomName;
    [Export]
    public int RoomId;

    [Export]
    private bool RoomHasNoPickups;
    [Export]
    private Label RoomLabel;
    [Export]
    private ColorRect RoomStatus;
    [Export]
    public Vector2 PlayerMapPositionScale = new Vector2(1, 1);
    [Export]
    public Vector2 RoomOriginOffset = new Vector2(0, 0);

    public bool PlayerVisitedRoom;
    public bool PlayerClearedRoom;

    private static Color UnvisitedRoomColor = new Color(0.5f, 0.5f, 0.5f);
    private static Color ClearedRoomColor = new Color(0.0f, 1.0f, 0.5f);
    private static Color VisitedRoomColor = new Color(1.0f, 0.25f, 0.25f);
    private static Color InRoomModulateColor = new Color(2.0f, 2.0f, 2.0f);
    private static Color InRoomDefaultModulateColor = new Color(1, 1, 1);
    private const float InRoomFlashSpeed = 1.5f;

    private bool _playerIsInRoom;
    private int _flashingDirection = 1;

    public override void _Ready()
    {
        RoomLabel.Text = RoomName;
    }

    public override void _Process(double delta)
    {
        FlashWhenInRoom(delta);
    }

    private void FlashWhenInRoom(double delta)
    {
        if (!_playerIsInRoom) return;

        var change = _flashingDirection * InRoomFlashSpeed * (float)delta;
        var newModulate = new Color(RoomStatus.SelfModulate.R + change, RoomStatus.SelfModulate.G + change, RoomStatus.SelfModulate.B + change)
            //.Clamp(InRoomModulateColor, InRoomDefaultModulateColor);
            .Clamp(InRoomDefaultModulateColor, InRoomModulateColor);
        //GD.Print($"Changing SelfModulate from {SelfModulate} to {newModulate}");
        RoomStatus.SelfModulate = newModulate;

        //if (RoomStatus.SelfModulate.R <= InRoomModulateColor.R || RoomStatus.SelfModulate.R >= InRoomDefaultModulateColor.R)
        if (RoomStatus.SelfModulate.R <= InRoomDefaultModulateColor.R || RoomStatus.SelfModulate.R >= InRoomModulateColor.R)
            _flashingDirection *= -1;
    }

    public void UpdateStatus(bool hasMap, bool isInRoom)
    {
        Visible = hasMap || PlayerVisitedRoom;
        _playerIsInRoom = isInRoom;

        if (RoomHasNoPickups)
        {
            var mapStatus = MapStatus.GetInstance();
            mapStatus.ClearRoom(RoomId);
            PlayerClearedRoom = true;
        }

        var currentStatusColor = UnvisitedRoomColor;
        if (PlayerClearedRoom && PlayerVisitedRoom)
            currentStatusColor = ClearedRoomColor;
        else if (PlayerVisitedRoom)
            currentStatusColor = VisitedRoomColor;

        RoomStatus.SelfModulate = InRoomDefaultModulateColor;
        RoomStatus.Color = currentStatusColor;
    }
}

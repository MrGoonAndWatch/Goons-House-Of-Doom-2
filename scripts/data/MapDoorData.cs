using Godot;

public partial class MapDoorData : Control
{
    [Export]
    public int DoorId;
    [Export]
    public int FromRoomId;
    [Export]
    public int ToRoomId;

    [Export]
    private ColorRect DoorColor;

    private static Color UnknownDoorColor = new Color(0.5f, 0.5f, 0.5f);
    private static Color OpenedDoorColor = new Color(1.0f, 1.0f, 1.0f);
    private static Color LockedDoorColor = new Color(1.0f, 0.0f, 0.0f);

    public void UpdateStatus(GameConstants.DoorMapStatus status)
    {
        if (status == GameConstants.DoorMapStatus.Unseen)
            Visible = false;
        else
            Visible = true;

        switch (status)
        {
            case GameConstants.DoorMapStatus.Unknown:
                DoorColor.Color = UnknownDoorColor;
                break;
            case GameConstants.DoorMapStatus.Opened:
                DoorColor.Color = OpenedDoorColor;
                break;
            case GameConstants.DoorMapStatus.Locked:
                DoorColor.Color = LockedDoorColor;
                break;
        }
    }
}

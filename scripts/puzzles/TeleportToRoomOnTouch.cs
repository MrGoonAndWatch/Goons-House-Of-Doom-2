using Godot;

public partial class TeleportToRoomOnTouch : Teleporter
{
    private void _OnBodyEntered(Node3D other)
    {
        if (other is Player)
            ActivateTeleporter();
    }
}

using Godot;

public partial class TeleportToRoomOnTouch : Teleporter
{
    private bool _isTeleporting;
    private bool _teleporterUsed;

    public override void _Process(double delta)
    {
        // NOTE: This is done in the Process function instead of on the event because changing scenes during a physics callback is not recommended.
        if (_isTeleporting)
        {
            _isTeleporting = false;
            ActivateTeleporter();
        }
    }

    private void _OnBodyEntered(Node3D other)
    {
        if (!_teleporterUsed && other is Player)
        {
            _isTeleporting = true;
            _teleporterUsed = true;
        }
    }
}

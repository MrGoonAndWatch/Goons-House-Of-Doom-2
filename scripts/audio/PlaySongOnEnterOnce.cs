using Godot;

public partial class PlaySongOnEnterOnce : Node
{
    [Export]
    private GameConstants.GlobalEvent EventToTrigger;

    public override void _Ready()
    {
        var playerStatus = PlayerStatus.GetInstance();
        if (!playerStatus.TriggeredEvents.Contains(EventToTrigger))
        {
            GhodAudioManager.PlayDemoSong();
            playerStatus.TriggeredEvent(EventToTrigger);
        }
    }
}

using Godot;

public partial class NotePickup : Area3D
{
    [Export]
    public NoteData NoteData;

    public override void _Ready()
    {
        var playerStatus = PlayerStatus.GetInstance();
        if (playerStatus == null) return;

        for(int i = 0; i < playerStatus.NotesCollected.Count; i++)
        {
            if (playerStatus.NotesCollected[i].NoteId == NoteData.NoteId)
            {
                GetParent().QueueFree();
                break;
            }
        }
    }
}

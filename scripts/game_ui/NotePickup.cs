using Godot;

public partial class NotePickup : Area3D
{
    public NoteData NoteData;

    [Export]
    public Node3D LookAtTargetPoint;

    public override void _Ready()
    {
        NoteData = GetParent<NoteData>();
        var playerStatus = PlayerStatus.GetInstance();
        if (playerStatus == null) return;

        for (int i = 0; i < playerStatus.NotesCollected.Count; i++)
        {
            if (playerStatus.NotesCollected[i].NoteId == NoteData.NoteId)
            {
                GetParent().QueueFree();
                break;
            }
        }
    }
}

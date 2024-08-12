using Godot;

public partial class NotePickup : Area3D
{
    [Export]
    public NoteData NoteData;

    public override void _Ready()
    {
        if (PlayerStatus.GetInstance()?.NotesCollected?.Contains(NoteData.NoteId) ?? false)
            GetParent().QueueFree();
    }
}

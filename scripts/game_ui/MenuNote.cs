using Godot;

public partial class MenuNote : Control
{
    [Export]
    private Button NoteButton;
    
    private NotesStatusUi _notesStatusUi;
    private NoteData _noteData;

    public void Init(NotesStatusUi notesStatusUi, NoteData noteData)
    {
        _notesStatusUi = notesStatusUi;
        _noteData = noteData;
        NoteButton.Text = noteData.NoteTitle;
    }

    public void _OnNoteButtonPressed()
    {
        _notesStatusUi.StartReadingNote(_noteData);
    }
}

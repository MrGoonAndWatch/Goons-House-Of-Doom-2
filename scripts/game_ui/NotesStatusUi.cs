using Godot;

public partial class NotesStatusUi : StatusScreenTab
{
    [Export]
    private Control NoteListParent;
    [Export]
    private NoteReader NoteReader;

    private Button _currentlySelectedNote;

    public override void _Ready()
    {
        var viewport = GetNode<SubViewport>(GameConstants.NodePaths.FromSceneRoot.SceneRoot);
        if (viewport != null)
            viewport.GuiFocusChanged += _OnFocusChanged;
        else
            GD.PrintErr("Warning: scene loaded but failed to find viewport to wire up GuiFocusedChange event for notes UI in inventory menu.");
    }

    public override void _Process(double delta)
    {
        CheckForReturnToHeader();
    }

    public override void SwitchOnTab()
    {
        base.SwitchOnTab();

        if (NoteListParent.GetChildren().Count > 0)
            ((Button)NoteListParent.GetChild(0).GetChild(0)).GrabFocus();
    }

    public override void SwitchOffTab()
    {
        base.SwitchOffTab();

        Visible = false;
        Visible = true;
    }

    public void StartReadingNote(NoteData noteData)
    {
        NoteReader.StartReadingNote(noteData);
        SwitchOffTab();
    }

    public void StopReadingNote()
    {
        var focusOnButton = _currentlySelectedNote;
        SwitchOnTab();
        if (focusOnButton != null)
            focusOnButton.GrabFocus();
    }

    public override void OnOpenMenu()
    {
        ClearNoteUi();
        var notes = PlayerStatus.GetInstance().NotesCollected;
        for (var i = 0; i < notes.Count; i++)
        {
            //GD.Print($"Creating prefab for note {i} (noteId = {notes[i].NoteId}, noteTitle = {notes[i].NoteTitle})");
            AddNoteButton(notes[i]);
        }
    }

    private void ClearNoteUi()
    {
        var notes = NoteListParent.GetChildren();
        for (var i = 0; i < notes.Count; i++)
            notes[i].QueueFree();
    }

    private void AddNoteButton(NoteData noteData)
    {
        var menuNoteScene = GD.Load<PackedScene>(GameConstants.StatusNoteUi);
        var menuNoteUi = (menuNoteScene.Instantiate()) as MenuNote;
        menuNoteUi.Init(this, noteData);
        NoteListParent.AddChild(menuNoteUi);
    }

    public void _OnFocusChanged(Control newFocus)
    {
        if(newFocus is Button)
            _currentlySelectedNote = (Button)newFocus;
    }
}

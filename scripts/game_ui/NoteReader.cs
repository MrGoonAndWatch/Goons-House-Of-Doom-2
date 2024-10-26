using Godot;
using static GameConstants;

public partial class NoteReader : Control
{
    [Export]
    private TextureRect _noteBackgroundImage;
    [Export]
    private Control _previousPageArrow;
    [Export]
    private Control _nextPageArrow;
    [Export]
    private Label _currentPageTextLabel;
    [Export]
    private NotesStatusUi _notesStatusUi;

    /// <summary>
    /// 0 indexed number of what page is currently being displayed by the reader (1:1 w/ CurrentNoteText).
    /// </summary>
    private int CurrentNotePage;
    private string[] CurrentNoteText;
    private bool IsReadingNote;
    private bool IsChangingPage;

    public void StartReadingNote(NoteData noteData)
    {
        CurrentNotePage = 0;
        CurrentNoteText = noteData.NoteText;
        IsReadingNote = true;

        var image = Image.LoadFromFile(noteData.NoteTexturePath);
        _noteBackgroundImage.Texture = ImageTexture.CreateFromImage(image);

        SwitchToPage(CurrentNotePage);
        Visible = true;
    }

    public override void _Process(double delta)
    {
        if (!IsReadingNote)
            return;

        var input_dir = GameConstants.GetMovementVectorRaw();
        var confirmWasPressed = Input.IsActionJustPressed(Controls.confirm.ToString());
        var closeNotesFromConfirm = confirmWasPressed && CurrentNotePage == CurrentNoteText.Length - 1;

        if (!IsChangingPage && input_dir.X < 0)
            SwitchToPage(CurrentNotePage - 1);
        else if (!IsChangingPage && (input_dir.X > 0 || confirmWasPressed))
            SwitchToPage(CurrentNotePage + 1);

        if (IsChangingPage && input_dir.X == 0)
            IsChangingPage = false;

        if (Input.IsActionPressed(Controls.aim.ToString()) || closeNotesFromConfirm)
            CloseNote();
    }

    private void SwitchToPage(int pageNumber)
    {
        if (pageNumber < 0 || pageNumber >= CurrentNoteText.Length)
            return;

        // TODO: Play sound.

        IsChangingPage = true;
        CurrentNotePage = pageNumber;
        var actualText = CurrentNoteText[pageNumber].Replace("\\r\\n", "\r\n");
        _currentPageTextLabel.Text = actualText;
        
        UpdatePrevNextBtns();
    }

    private void UpdatePrevNextBtns()
    {
        if (CurrentNotePage == 0)
            _previousPageArrow.Visible = false;
        else
            _previousPageArrow.Visible = true;
        if (CurrentNotePage == CurrentNoteText.Length - 1)
            _nextPageArrow.Visible = false;
        else
            _nextPageArrow.Visible = true;
    }
    
    private void CloseNote()
    {
        // TODO: Play other sound?
        Visible = false;
        CurrentNotePage = 0;
        CurrentNoteText = null;
        IsReadingNote = false;

        if (PlayerStatus.GetInstance().MenuOpened)
            _notesStatusUi.StopReadingNote();
    }
}

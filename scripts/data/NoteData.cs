using Godot;

public partial class NoteData : Node
{
    [Export]
    public int NoteId;
    [Export]
    public string NoteTitle;
    // Note: One day Godot will fix this bug and let us have multiline string arrays in C# :(
    [Export(PropertyHint.MultilineText)]
    public string[] NoteText;
    [Export]
    public string NoteTexturePath;

    public NoteData Clone()
    {
        var copiedNoteText = new string[NoteText.Length];
        for (var i = 0; i < NoteText.Length; i++)
        {
            copiedNoteText[i] = NoteText[i].ToString();
        }

        return new NoteData()
        {
            NoteId = NoteId,
            NoteTitle = NoteTitle.ToString(),
            NoteText = copiedNoteText,
            NoteTexturePath = NoteTexturePath.ToString(),
        };
    }
}

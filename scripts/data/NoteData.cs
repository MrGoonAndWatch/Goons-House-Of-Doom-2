using Godot;

public partial class NoteData : Node
{
    [Export]
    public int NoteId;
    // Note: One day Godot will fix this bug and let us have multiline string arrays in C# :(
    [Export(PropertyHint.MultilineText)]
    public string[] NoteText;
    [Export]
    public string NoteTexturePath;
}

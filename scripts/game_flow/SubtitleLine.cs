using Godot;

public partial class SubtitleLine : Node
{
    [Export(PropertyHint.MultilineText)]
    public string SubtitleContent;
    [Export]
    public string CharacterSpeaking;
    [Export]
    public float SubtitleDisplayTimeInSeconds = 1.0f;
}

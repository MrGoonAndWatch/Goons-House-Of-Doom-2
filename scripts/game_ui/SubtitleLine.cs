using Godot;

public partial class SubtitleLine : Node
{
    [Export(PropertyHint.MultilineText)]
    public string SubtitleContent;
    [Export]
    public string CharacterSpeaking;
    [Export]
    public Color SubtitleColor = Color.Color8(255, 255, 255);
}

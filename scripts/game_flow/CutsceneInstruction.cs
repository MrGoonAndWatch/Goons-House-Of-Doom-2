using Godot;
using static GameConstants;

public partial class CutsceneInstruction : Node
{
    [Export]
    public CutsceneInstructionEndType EndType { get; set; }
    [Export]
    public float EndTimer { get; set; }

    [Export]
    public ICutsceneActor TargetActor { get; set; }
    [Export]
    public Vector3 MoveToPosition { get; set; }
    [Export]
    public float MoveSpeed { get; set; } = 200.0f;
    [Export]
    public string AnimationFlag { get; set; }
    [Export]
    public string VoiceLineFile { get; set; }
}
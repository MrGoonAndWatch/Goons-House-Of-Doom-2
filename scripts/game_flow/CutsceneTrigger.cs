using Godot;
using static GameConstants;

public partial class CutsceneTrigger : Node
{
    [Export]
    private Cutscene Cutscene { get; set; }
    
    // TODO: Eventually there will be more cutscene trigger types, use this to determine how the cutscene goes off!
    [Export]
    private CutsceneTriggerType TriggerType { get; set; } = CutsceneTriggerType.OnSceneLoaded;

    public override void _Ready()
    {
        var cutsceneManager = GetNode<CutsceneManager>(GameConstants.NodePaths.FromSceneRoot.CutsceneManager);
        cutsceneManager.StartCutscene(Cutscene);
    }
}

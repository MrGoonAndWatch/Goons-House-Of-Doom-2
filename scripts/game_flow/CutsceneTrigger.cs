using Godot;

public partial class CutsceneTrigger : Node
{
    [Export]
    private Cutscene Cutscene { get; set; }

    public override void _Ready()
    {
        var cutsceneManager = GetNode<CutsceneManager>(GameConstants.NodePaths.FromSceneRoot.CutsceneManager);
        cutsceneManager.StartCutscene(Cutscene);
    }
}

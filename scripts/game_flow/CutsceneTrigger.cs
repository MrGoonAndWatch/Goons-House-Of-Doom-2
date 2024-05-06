using Godot;

public partial class CutsceneTrigger : Node
{
    [Export]
    private Cutscene Cutscene { get; set; }

    public override void _Ready()
    {
        // TODO: Eventually make multiple types of cutscene triggers but for now this just triggers the cutscene on scene load.
        var cutsceneManager = GetNode<CutsceneManager>(GameConstants.NodePaths.FromSceneRoot.CutsceneManager);
        cutsceneManager.StartCutscene(Cutscene);
    }
}

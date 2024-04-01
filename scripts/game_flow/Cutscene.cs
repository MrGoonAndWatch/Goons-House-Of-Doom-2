using Godot;

public partial class Cutscene : Node
{
    [Export]
    private ICutsceneActor[] Actors;
    //[Export]
    //private List<CutsceneInstruction> Instructions;

    public void StartCutscene()
    {
    }

    public void NextInstruction()
    {
    }

    public bool HasCutsceneEnded()
    {
        return false;
    }
}

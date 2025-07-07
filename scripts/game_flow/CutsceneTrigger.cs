using Godot;
using static GameConstants;

public partial class CutsceneTrigger : Node
{
    [Export]
    private Cutscene Cutscene { get; set; }
    
    // TODO: Eventually there will be more cutscene trigger types, use this to determine how the cutscene goes off!
    [Export]
    private CutsceneTriggerType TriggerType { get; set; } = CutsceneTriggerType.OnSceneLoaded;

    private bool _triggered;
    private const float BufferTriggerTimeForCameraInit = 0.2f; 
    private float _timeUntilTriggered;

    public override void _Ready()
    {
        _timeUntilTriggered = BufferTriggerTimeForCameraInit;
    }
    
    public override void _Process(double delta)
    {
        ProcessCutsceneTriggerOnSceneLoad(delta);
    }

    private void ProcessCutsceneTriggerOnSceneLoad(double delta)
    {
        if (!_triggered)
        {
            _timeUntilTriggered -= (float)delta;

            if (_timeUntilTriggered <= 0)
            {
                //GD.Print($"Triggered cutscene '{this.Name}'");
                var cutsceneManager = GetNode<CutsceneManager>(GameConstants.NodePaths.FromSceneRoot.CutsceneManager);
                cutsceneManager.StartCutscene(Cutscene);
                _triggered = true;
            }
        }
    }
}

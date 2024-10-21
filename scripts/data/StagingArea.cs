using Godot;

public partial class StagingArea : Node
{
    private bool _initialized;

    public override void _Process(double delta)
    {
        if (!_initialized)
        {
            _initialized = true;
            var loadGameData = LoadGameData.GetInstance();
            loadGameData.FinishLoadingFromFile();
        }
    }
}

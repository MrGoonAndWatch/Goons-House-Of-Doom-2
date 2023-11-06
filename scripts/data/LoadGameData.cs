using Godot;

public partial class LoadGameData : Node3D
{
    private static LoadGameData _instance;
    private DataSaver.GameState _gameState;

    public override void _Ready()
    {
        if(_instance != null)
        {
            GD.PrintErr($"Multiple LoadGameData instances found! Destroying '{Name}'!");
            QueueFree();
            return;
        }

        _instance = this;
    }

    public static LoadGameData GetInstance()
    {
        return _instance;
    }

    public void SetGameState(DataSaver.GameState gameState)
    {
        _gameState = gameState;
        DataSaver.GetInstance().LoadGameStateFromFileData(_gameState);
        SceneChanger.GetInstance().ChangeScene(gameState.SceneLoadData);
    }

    public DataSaver.GameState GetGameState()
    {
        return _gameState;
    }
}

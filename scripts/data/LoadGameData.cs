using Godot;
using static DataSaver;

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
        PlayerStatus.GetInstance().ResetGame();
        _gameState = gameState;
        GetTree().ChangeSceneToFile(GameConstants.StagingAreaScenePath);
    }

    public void FinishLoadingFromFile()
    {
        GD.Print($"FinishLoadingFromFile. Loading scene {_gameState.SceneLoadData}...");
        DataSaver.GetInstance().LoadGameStateFromFileData(_gameState);
        SceneChanger.GetInstance().ChangeScene(_gameState.SceneLoadData);
    }

    public DataSaver.GameState GetGameState()
    {
        return _gameState;
    }
}

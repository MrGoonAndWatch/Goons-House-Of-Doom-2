using Godot;
using static GameConstants;

public partial class SceneChanger : Node
{
    private static SceneChanger Instance;

    private DataSaver _dataSaver;
    private bool _loadScene;

    public static SceneChanger GetInstance()
    {
        return Instance;
    }

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;

        if (_dataSaver == null)
            _dataSaver = DataSaver.GetInstance();
    }

    public override void _Process(double delta)
    {
        if (!_loadScene) return;

        OnNewSceneLoaded();
    }

    public void ChangeScene(SceneLoadData sceneLoadData, DoorLoadType doorScene = DoorLoadType.None)
    {
        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
        var playerItemBox = GetNode<PlayerItemBoxControl>(NodePaths.FromSceneRoot.ItemBoxControl);
        var playerInteract = GetNode<PlayerInteract>(NodePaths.FromSceneRoot.PlayerInteract);
        playerInteract.ResetState();
        _dataSaver.SaveGameStateFromScene(playerStatus, playerInventory, sceneLoadData, playerItemBox);

        if (doorScene == DoorLoadType.None)
            FinishSceneLoad();
        else
            GetTree().ChangeSceneToFile($"res://scenes/door_loads/{doorScene}.tscn");
    }

    public void FinishSceneLoad()
    {
        var sceneLoadData = _dataSaver.GetSceneLoadData();
        _loadScene = true;
        GetTree().ChangeSceneToFile(sceneLoadData.GetTargetSceneFullPath());
    }

    private void OnNewSceneLoaded()
    {
        _loadScene = false;
        var player = GetNode<Player>(NodePaths.FromSceneRoot.Player);
        var sceneLoadData = _dataSaver.GetSceneLoadData();
        player.Position = sceneLoadData.LoadPosition;
        player.RotationDegrees = sceneLoadData.LoadRotation;
        GD.Print($"Set player position to {sceneLoadData.LoadPosition} and rotation to {sceneLoadData.LoadRotation} in new scene.");

        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
        var playerItemBox = GetNode<PlayerItemBoxControl>(NodePaths.FromSceneRoot.ItemBoxControl);
        _dataSaver.LoadFromGameState(playerStatus, playerInventory, playerItemBox);
    }
}

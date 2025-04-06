using Godot;
using static GameConstants;

public partial class SceneChanger : Node
{
    private const string SceneBasePath = "res://scenes/";
    public const string GameOverScreen = $"{SceneBasePath}game-over.tscn";
    public const string StagingArea = $"{SceneBasePath}staging_area.tscn";
    public const string TitleScreenScene = $"{SceneBasePath}title_screen.tscn";

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

        var player = GetNodeOrNull<Player>(NodePaths.FromSceneRoot.Player);
        if(player != null)
            OnNewSceneLoaded();
    }

    public static (bool, string) IsValidSceneChange(SceneLoadData sceneLoadData, DoorLoadType doorSceneRaw)
    {
        var validSettings = true;
        var errorMessage = "";
        if (!ResourceLoader.Exists(sceneLoadData.GetTargetSceneFullPath()))
        {
            validSettings = false;
            errorMessage += $"target scene not found '{sceneLoadData.GetTargetSceneFullPath()}'\r\n";
        }
        else if (doorSceneRaw != DoorLoadType.None && !ResourceLoader.Exists($"res://scenes/door_loads/{doorSceneRaw}.tscn"))
        {
            validSettings = false;
            errorMessage += $"invalid DoorLoadType specified '{doorSceneRaw}'\r\n";
        }

        return (validSettings, errorMessage);
    }

    public void ChangeScene(SceneLoadData sceneLoadData, DoorLoadType doorScene = DoorLoadType.None)
    {
        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
        var playerItemBox = GetNode<PlayerItemBoxControl>(NodePaths.FromSceneRoot.ItemBoxControl);
        var playerInteract = GetNode<PlayerInteract>(NodePaths.FromSceneRoot.PlayerInteract);
        var mapStatus = MapStatus.GetInstance();
        playerInteract.ResetState();
        _dataSaver.SaveGameStateFromScene(playerStatus, playerInventory, sceneLoadData, playerItemBox, mapStatus);

        if (doorScene == DoorLoadType.None)
            FinishSceneLoad();
        else
            GetTree().ChangeSceneToFile($"res://scenes/door_loads/{doorScene}.tscn");
    }

    public void ChangeSceneDirectly(string targetScene)
    {
        GetTree().ChangeSceneToFile(targetScene);
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
        //GD.Print($"Set player position to {sceneLoadData.LoadPosition} and rotation to {sceneLoadData.LoadRotation} in new scene.");

        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
        var playerItemBox = GetNode<PlayerItemBoxControl>(NodePaths.FromSceneRoot.ItemBoxControl);
        var mapStatus = MapStatus.GetInstance();
        _dataSaver.LoadFromGameState(playerStatus, playerInventory, playerItemBox, mapStatus);
    }
}

using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class SaveGame : Node
{
    [Export]
    public Control SaveGameUi;
    [Export]
    public Control LoadingMessage;
    [Export]
    public Control SaveFileList;

    private const string SaveDirectoryPath = "user://saves";
    private const string ScenesDirectory = "res://scenes/";

    private bool _menuOpened;
    private bool _loadingSaveFiles;
    private bool _firstFrameSinceLoaded;
    private List<string> _saveFileNames;

    public override void _Ready()
	{
        //var size = new Vector2I(800, 600);
        //DisplayServer.WindowSetSize(size);
        DisplayServer.WindowSetSize(DisplayServer.ScreenGetSize());
        //GetViewport().Size = new

        // if (e.Pressed)
        // {
        //     InputMap.ActionEraseEvents("Aim");
        //     InputMap.ActionAddEvent("Aim", e);
        //     InputMap.ActionGetEvents("Aim")[0].AsText();
        // }

        DirAccess.MakeDirRecursiveAbsolute(SaveDirectoryPath);
        LoadingMessage.Visible = false;
    }

	public override void _Process(double delta)
	{
         if (Input.IsActionJustPressed("DEBUG_Save"))
             CreateSaveFile("test_scene2 - 24-01-07_21-45-51.sav");
         if(Input.IsActionJustPressed("DEBUG_Load"))
             LoadSaveFile(0);

        if (!_menuOpened)
            return;
        ProcessFileSelect();
    }

    private void ProcessFileSelect()
    {
        if (_firstFrameSinceLoaded)
            _firstFrameSinceLoaded = false;

        
        if (Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()) || Input.IsActionJustPressed(GameConstants.Controls.aim.ToString()))
        {
            CreateSaveFile();
        }
    }

    // TODO: Add error handling!
    private void CreateSaveFile(string filename = null)
    {
        var player = GetNode<Player>(GameConstants.NodePaths.FromSceneRoot.Player);
        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(GameConstants.NodePaths.FromSceneRoot.PlayerInventory);
        var playerItemBox = GetNode<PlayerItemBoxControl>(GameConstants.NodePaths.FromSceneRoot.ItemBoxControl);
        var sceneName = GetTree().CurrentScene.SceneFilePath.Replace(ScenesDirectory, "").Replace(".tscn", "");
        var sceneInfo = new SceneLoadData
        {
            TargetScene = sceneName,
            LoadPosition = player.Position,
            LoadRotation = player.RotationDegrees,
        };

        var saver = DataSaver.GetInstance();
        saver.SaveGameStateFromScene(playerStatus, playerInventory, sceneInfo, playerItemBox);
        var data = saver.GetGameState();

        var roomStr = sceneName;
        var dateStr = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss");
        var newFilename = $"{roomStr} - {dateStr}.sav";

        GD.Print($"Trying to save data to {newFilename}...");
        var fileAccess = FileAccess.Open($"{SaveDirectoryPath}/{newFilename}", FileAccess.ModeFlags.Write);

        var dataJson = JsonConvert.SerializeObject(data);
        fileAccess.StoreString(dataJson);
        if (!string.IsNullOrEmpty(filename))
            DirAccess.RemoveAbsolute($"{SaveDirectoryPath}/{filename}");

        Close();

        GD.Print($"Finished saving file '{newFilename}'");
    }

    public void Open()
    {
        var playerStatus = PlayerStatus.GetInstance();
        if (playerStatus != null)
            playerStatus.HasSaveUiOpen = true;

        _loadingSaveFiles = true;
        LoadingMessage.Visible = true;
        SaveGameUi.Visible = true;

        _menuOpened = true;
    }

    private void Close()
    {
        _menuOpened = false;

        var playerStatus = PlayerStatus.GetInstance();
        if (playerStatus != null)
            playerStatus.HasSaveUiOpen = false;
        SaveGameUi.Visible = false;
    }

    private void RefreshSaveFileList()
    {
        _saveFileNames = GetSaveFilesByMostRecentFirst();
        var saveFileDisplayStr = new StringBuilder("Create New Save File\r\n\r\n");
        foreach (var saveFileName in _saveFileNames)
        {
            var cleanFileName = CleanFileName(saveFileName);
            saveFileDisplayStr.AppendLine(cleanFileName);
        }

        var displayStr = saveFileDisplayStr.ToString();
    }

    private static List<string> GetSaveFilesByMostRecentFirst()
    {
        var saveDir = DirAccess.Open(SaveDirectoryPath);

        var files = saveDir.GetFiles().Where(f => f.EndsWith(".sav"));

        var filenamesWithLastModified = new List<Tuple<string, ulong>>();
        foreach (var file in files)
        {
            var lastModified = FileAccess.GetModifiedTime(file);
            filenamesWithLastModified.Add(new Tuple<string, ulong>(file, lastModified));
        }

        var saveFileNames = filenamesWithLastModified.OrderByDescending(f => f.Item2).Select(f => f.Item1).ToList();
        return saveFileNames;
    }

    public static string CleanFileName(string saveFileName)
    {
        var saveFileLastSlashIndex = saveFileName.LastIndexOfAny(new[] { '\\', '/' });
        var fileExtensionStartIndex = saveFileName.LastIndexOf('.');
        var cleanFileName = saveFileName.Substring(saveFileLastSlashIndex + 1, fileExtensionStartIndex - saveFileLastSlashIndex - 1);
        return cleanFileName;
    }

    // TODO: Move this to main menu UI at some point.
    public void LoadSaveFile(int fileSlot)
    {
        //if (fileSlot >= _saveFileSlots.Length)
        //    return;
        //
        //var targetFile = _saveFileSlots[fileSlot];

        // TODO: Load filename from slot # instead of hard coding!!!
        var targetFile = "test_scene2 - 24-01-08_01-03-42.sav";
        var targetFilePath = $"{SaveDirectoryPath}/{targetFile}";

        if (string.IsNullOrEmpty(targetFile) || !FileAccess.FileExists(targetFilePath))
            return;
        
        var file = FileAccess.Open(targetFilePath, FileAccess.ModeFlags.Read);
        var saveData = file.GetAsText();
        var gameState = JsonConvert.DeserializeObject<DataSaver.GameState>(saveData);

        LoadGameData.GetInstance().SetGameState(gameState);
    }
}

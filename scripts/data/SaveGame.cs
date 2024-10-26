using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SaveGame : Control
{
    [Export]
    private Control SaveGameUi;
    [Export]
    private Control LoadingMessage;
    [Export]
    private Control SaveFileList;
    [Export]
    private ScrollContainer SaveFilesScroll;

    private bool _menuOpened;
    private bool _loadingSaveFiles;
    private List<string> _saveFileNames;

    private List<SaveFileUi> _saveFiles;

    private bool _isLoading;

    public override void _Ready()
	{
        SaveGameUi.Visible = false;
        SaveFilesScroll.GetVScrollBar().Modulate = GameConstants.Colors.Clear;
        DirAccess.MakeDirRecursiveAbsolute(GameConstants.SaveDirectoryPath);
        LoadingMessage.Visible = false;
        RefreshSaveFileList();
    }

	public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("DEBUG_Save"))
            ShowSaveUi();
        if (Input.IsActionJustPressed("DEBUG_Load"))
            ShowLoadUi();
    }

    public void ShowSaveUi()
    {
        _isLoading = false;
        OpenSaveLoadUi();
    }

    public void ShowLoadUi()
    {
        _isLoading = true;
        OpenSaveLoadUi();
    }

    public void SaveSlotSelected(SaveFileUi saveFile)
    {
        if (_isLoading)
            LoadSaveFile(saveFile.SaveFileName);
        else
            CreateSaveFile(saveFile.IsNewFileSlot ? null : saveFile.SaveFileName);
    }

    private void CreateSaveFile(string filename = null)
    {
        var player = GetNode<Player>(GameConstants.NodePaths.FromSceneRoot.Player);
        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(GameConstants.NodePaths.FromSceneRoot.PlayerInventory);
        var playerItemBox = GetNode<PlayerItemBoxControl>(GameConstants.NodePaths.FromSceneRoot.ItemBoxControl);
        var mapStatus = MapStatus.GetInstance();
        var sceneName = GameConstants.GetCurrentSceneFilepath(this);
        var sceneInfo = new SceneLoadData
        {
            TargetScene = sceneName,
            LoadPosition = player.Position,
            LoadRotation = player.RotationDegrees,
        };

        var saver = DataSaver.GetInstance();
        playerStatus.HasSaveLoadUiOpen = false;
        saver.SaveGameStateFromScene(playerStatus, playerInventory, sceneInfo, playerItemBox, mapStatus);
        var data = saver.GetGameState();

        var roomStr = GameConstants.GetCurrentRoomName(this);
        var dateStr = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss");
        var newFilename = $"{roomStr} - {dateStr}.sav";

        GD.Print($"Trying to save data to {newFilename}...");
        var fileAccess = FileAccess.Open($"{GameConstants.SaveDirectoryPath}/{newFilename}", FileAccess.ModeFlags.Write);
        var dataJson = JsonConvert.SerializeObject(data);
        fileAccess.StoreString(dataJson);
        if (!string.IsNullOrEmpty(filename))
            DirAccess.RemoveAbsolute($"{GameConstants.SaveDirectoryPath}/{filename}");
        fileAccess.Close();

        CloseSaveUi();

        GD.Print($"Finished saving file '{newFilename}'");
    }

    private void OpenSaveLoadUi()
    {
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.HasSaveLoadUiOpen = true;

        _loadingSaveFiles = true;
        RefreshSaveFileList();
        SaveGameUi.Visible = true;

        _menuOpened = true;
    }

    public void CloseSaveUi()
    {
        _menuOpened = false;

        var playerStatus = PlayerStatus.GetInstance();
        if (playerStatus != null)
            playerStatus.HasSaveLoadUiOpen = false;
        SaveGameUi.Visible = false;
    }

    private void RefreshSaveFileList()
    {
        // GD.Print("RefreshSaveFileList start");
        SaveFilesScroll.Visible = false;
        LoadingMessage.Visible = true;
        _saveFileNames = GetSaveFilesByLeastRecentFirst();

        if (_saveFiles == null)
            _saveFiles = new List<SaveFileUi>();
        else
        {
            foreach (var file in _saveFiles)
                file.QueueFree();
            _saveFiles.Clear();
        }
        
        foreach (var saveFileName in _saveFileNames)
        {
            var cleanFileName = CleanFileName(saveFileName);

            var saveFileButtonScene = GD.Load<PackedScene>(GameConstants.SaveFileButtonUi);
            var saveFileButton = (saveFileButtonScene.Instantiate()) as SaveFileUi;
            saveFileButton.SaveFileButton.Text = cleanFileName;
            saveFileButton.SaveFileName = saveFileName;
            saveFileButton.SaveGameUi = this;
            _saveFiles.Add(saveFileButton);
            SaveFileList.AddSibling(saveFileButton);
        }

        if (!_isLoading)
        {
            var newSaveFileButtonScene = GD.Load<PackedScene>(GameConstants.SaveFileButtonUi);
            var newSaveFileButton = (newSaveFileButtonScene.Instantiate()) as SaveFileUi;
            newSaveFileButton.SaveFileButton.Text = GameConstants.SaveFileNewSaveText;
            newSaveFileButton.IsNewFileSlot = true;
            newSaveFileButton.SaveGameUi = this;
            _saveFiles.Add(newSaveFileButton);
            SaveFileList.AddSibling(newSaveFileButton);
        }

        if (_saveFiles.Any())
            _saveFiles.Last().SaveFileButton.GrabFocus();
        LoadingMessage.Visible = false;
        SaveFilesScroll.Visible = true;
    }

    private static List<string> GetSaveFilesByLeastRecentFirst()
    {
        var saveDir = DirAccess.Open(GameConstants.SaveDirectoryPath);

        var files = saveDir.GetFiles().Where(f => f.EndsWith(".sav"));

        var filenamesWithLastModified = new List<Tuple<string, ulong>>();
        foreach (var file in files)
        {
            var lastModified = FileAccess.GetModifiedTime($"{GameConstants.SaveDirectoryPath}/{file}");
            filenamesWithLastModified.Add(new Tuple<string, ulong>(file, lastModified));
            // GD.Print($"File = {file}, LastModified = {lastModified}");
        }

        var saveFileNames = filenamesWithLastModified.OrderBy(f => f.Item2).Select(f => f.Item1).ToList();
        return saveFileNames;
    }

    public static string CleanFileName(string saveFileName)
    {
        var saveFileLastSlashIndex = saveFileName.LastIndexOfAny(new[] { '\\', '/' });
        var fileExtensionStartIndex = saveFileName.LastIndexOf('.');
        var cleanFileName = saveFileName.Substring(saveFileLastSlashIndex + 1, fileExtensionStartIndex - saveFileLastSlashIndex - 1);
        return cleanFileName;
    }

    public void LoadSaveFile(string targetFile)
    {
        var targetFilePath = $"{GameConstants.SaveDirectoryPath}/{targetFile}";
        GD.Print($"Loading save file '{targetFilePath}'");

        if (string.IsNullOrEmpty(targetFile) || !FileAccess.FileExists(targetFilePath))
            return;
        
        var file = FileAccess.Open(targetFilePath, FileAccess.ModeFlags.Read);
        var saveData = file.GetAsText();
        file.Close();
        var gameState = JsonConvert.DeserializeObject<DataSaver.GameState>(saveData);

        GD.Print($"Save data as game state==null? {gameState == null}, saveData:\r\n{saveData}");
        LoadGameData.GetInstance().SetGameState(gameState);

        CloseSaveUi();
    }

    private void _OnBackButtonPressed()
    {
        CloseSaveUi();
    }
}

using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameConstants;

public partial class DataSaver : Node3D
{
    private static DataSaver Instance;

    private GameState _gameState;
    private GlobalSettings _globalSettings;

    private const string GlobalSettingsFullFilepath = $"{SaveDirectoryPath}/{GlobalSettingsFilename}";

    public static DataSaver GetInstance()
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

        ResetState();

        _globalSettings = new GlobalSettings
        {
            TotalVolume = 80.0f,
            MusicVolume = 80.0f,
            SfxVolume = 80.0f,
            VoiceVolume = 80.0f,
            Resolution = "1680x1050",
        };

        LoadGlobalSettingsFromFile();
    }

    public static void ResetState()
    {
        if (Instance == null) { return; }

        Instance._gameState = new GameState
        {
            DeadEnemies = new int[0],
            DoorsUnlocked = new int[0],
            GrabbedItems = new int[0],
            TriggeredEvents = new int[0],
            CutscenesWatched = new int[0],
            NotesCollected = new NoteData[0],
            Inventory = new ItemState[0],
            ItemBox = new ItemState[0],
            Health = PlayerStatus.MaxHealth,
            MapsCollected = new int[0],
            RoomsCleared = new int[0],
            RoomsVisited = new int[0],
            DoorsFound = new int[0],
            DoorsEntered = new int[0],
            LockedDoorsInspected = new int[0],
            SceneLoadData = new SceneLoadData
            {
                TargetScene = Instance.GetTree().CurrentScene.Name,
                LoadPosition = Vector3.Zero,
                LoadRotation = Vector3.Zero,
            },
            EquipedWeaponIndex = null,
        };
    }

    private void LoadGlobalSettingsFromFile()
    {
        DirAccess.MakeDirRecursiveAbsolute(SaveDirectoryPath);

        if (!FileAccess.FileExists(GlobalSettingsFullFilepath)) return;

        var settingsFile = FileAccess.Open(GlobalSettingsFullFilepath, FileAccess.ModeFlags.Read);
        var settingsFileJson = settingsFile.GetAsText();
        settingsFile.Close();

        try
        {
            var settings = JsonConvert.DeserializeObject<GlobalSettings>(settingsFileJson);
            _globalSettings = settings;
            GD.Print($"Successfully loaded global settings");
            GD.Print(settingsFileJson);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load global settings, file was found but not deserialzable: {e}");
        }
    }

    public GameState GetGameState()
    {
        return _gameState;
    }

    public static GlobalSettings GetGlobalSettings()
    {
        if(Instance == null)
        {
            GD.PrintErr("Failed to get global settings, DataSaver Instance was null!");
            throw new System.NullReferenceException();
        }

        return Instance._globalSettings;
    }

    public void SaveGlobalSettings(GlobalSettings settings)
    {
        _globalSettings = settings;
        var fileAccess = FileAccess.Open(GlobalSettingsFullFilepath, FileAccess.ModeFlags.Write);
        var settingsJson = JsonConvert.SerializeObject(_globalSettings);
        fileAccess.StoreString(settingsJson);
        fileAccess.Close();
    }

    public void SaveGameStateFromScene(PlayerStatus playerStatus, PlayerInventory playerInventory, SceneLoadData sceneLoadData, PlayerItemBoxControl playerItemBox, MapStatus mapStatus)
    {
        SavePlayerStatus(playerStatus, playerInventory);
        SaveInventory(playerInventory);
        SaveItemBox(playerItemBox);
        SaveSceneLoadData(sceneLoadData);
        SaveMapStatus(mapStatus);
    }

    private void SavePlayerStatus(PlayerStatus playerStatus, PlayerInventory playerInventory)
    {
        _gameState.Health = playerStatus.Health;
        _gameState.DeadEnemies = _gameState.DeadEnemies.Union(playerStatus.DeadEnemies).Distinct().ToArray();
        _gameState.DoorsUnlocked = _gameState.DoorsUnlocked.Union(playerStatus.DoorsUnlocked).Distinct().ToArray();
        _gameState.GrabbedItems = _gameState.GrabbedItems.Union(playerStatus.GrabbedItems).Distinct().ToArray();
        _gameState.TriggeredEvents = _gameState.TriggeredEvents.Union(playerStatus.TriggeredEvents.Select(e => (int)e)).Distinct().ToArray();
        _gameState.NotesCollected = _gameState.NotesCollected.Union(playerStatus.NotesCollected).ToArray();
        _gameState.CutscenesWatched = _gameState.CutscenesWatched.Union(playerStatus.CutscenesWatched).Distinct().ToArray();
        if (playerStatus.EquipedWeapon != null)
            for (var i = 0; i < playerInventory.Items.Length; i++)
                if (playerInventory.Items[i].Item != null && playerStatus.EquipedWeapon.ItemId == playerInventory.Items[i].Item.ItemId)
                    _gameState.EquipedWeaponIndex = i;
    }

    private void SaveInventory(PlayerInventory playerInventory)
    {
        var inventoryData = new List<ItemState>();
        foreach (var itemSlot in playerInventory.Items)
        {
            ItemState itemState;
            if (itemSlot.Item == null)
                itemState = new ItemState();
            else
                itemState = new ItemState
                {
                    ItemType = itemSlot.Item.GetPrefabPath(),
                    Qty = (itemSlot.Item is Weapon) ? (itemSlot.Item as Weapon).Ammo : itemSlot.Qty,
                };
            inventoryData.Add(itemState);
        }

        _gameState.Inventory = inventoryData.ToArray();
    }

    private void SaveItemBox(PlayerItemBoxControl playerItemBox)
    {
        var itemBoxData = new List<ItemState>();
        foreach (var itemSlot in playerItemBox.ItemBoxItems)
        {
            ItemState itemState;
            if (itemSlot.Item == null)
                itemState = new ItemState();
            else
                itemState = new ItemState
                {
                    ItemType = itemSlot.Item.GetPrefabPath(),
                    Qty = (itemSlot.Item is Weapon) ? (itemSlot.Item as Weapon).Ammo : itemSlot.Qty,
                };
            itemBoxData.Add(itemState);
        }

        _gameState.ItemBox = itemBoxData.ToArray();
    }

    private void SaveSceneLoadData(SceneLoadData sceneLoadData)
    {
        _gameState.SceneLoadData = sceneLoadData;
    }

    private void SaveMapStatus(MapStatus mapStatus)
    {
        _gameState.MapsCollected = mapStatus._mapsCollected.ToArray();
        _gameState.RoomsVisited = mapStatus._roomsVisited.ToArray();
        _gameState.RoomsCleared = mapStatus._roomsCleared.ToArray();
        _gameState.DoorsFound = mapStatus._doorsFound.ToArray();
        _gameState.DoorsEntered = mapStatus._doorsEntered.ToArray();
        _gameState.LockedDoorsInspected = mapStatus._lockedDoorsInspected.ToArray();
    }

    public void LoadGameStateFromFileData(GameState data)
    {
        _gameState = data;
        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
        var playerItemBox = GetNode<PlayerItemBoxControl>(NodePaths.FromSceneRoot.ItemBoxControl);
        var mapStatus = MapStatus.GetInstance();
        LoadFromGameState(playerStatus, playerInventory, playerItemBox, mapStatus);
    }

    public void LoadFromGameState(PlayerStatus playerStatus, PlayerInventory playerInventory, PlayerItemBoxControl playerItemBox, MapStatus mapStatus)
    {
        LoadInventory(playerInventory, playerItemBox);
        LoadItemBox(playerItemBox);
        LoadPlayerStatus(playerStatus, playerInventory);
        LoadMapStatus(mapStatus);
    }

    public SceneLoadData GetSceneLoadData()
    {
        return _gameState.SceneLoadData;
    }

    private void LoadPlayerStatus(PlayerStatus playerStatus, PlayerInventory playerInventory)
    {
        playerStatus.SetHealth(_gameState.Health);
        if (_gameState.EquipedWeaponIndex.HasValue)
        {
            var targetWeapon = playerInventory.Items[_gameState.EquipedWeaponIndex.Value].Item as Weapon;
            if (targetWeapon != null)
            {
                playerStatus.EquipWeapon(targetWeapon);
                playerInventory.EquipDirty = true;
            }
            else
                GD.Print("Tried to equip non-weapon item from item slot #" + _gameState.EquipedWeaponIndex);
        }
        playerStatus.GrabbedItems = _gameState.GrabbedItems.ToList();
        playerStatus.DeadEnemies = _gameState.DeadEnemies.ToList();
        playerStatus.NotesCollected = _gameState.NotesCollected.ToList();
        playerStatus.CutscenesWatched = _gameState.CutscenesWatched.ToList();
    }

    private void LoadMapStatus(MapStatus mapStatus)
    {
        mapStatus._mapsCollected = mapStatus._mapsCollected.Union(_gameState.MapsCollected).Distinct().ToList();
        mapStatus._roomsVisited = mapStatus._roomsVisited.Union(_gameState.RoomsVisited).Distinct().ToList();
        mapStatus._roomsCleared = mapStatus._roomsCleared.Union(_gameState.RoomsCleared).Distinct().ToList();
        mapStatus._doorsFound = mapStatus._doorsFound.Union(_gameState.DoorsFound).Distinct().ToList();
        mapStatus._doorsEntered = mapStatus._doorsEntered.Union(_gameState.DoorsEntered).Distinct().ToList();
        mapStatus._lockedDoorsInspected = mapStatus._lockedDoorsInspected.Union(_gameState.LockedDoorsInspected).Distinct().ToList();
    }

    private void LoadInventory(PlayerInventory playerInventory, PlayerItemBoxControl playerItemBox)
    {
        if (_gameState.Inventory == null || _gameState.ItemBox == null)
            return;

        for (var i = 0; i < playerInventory.Items.Length; i++)
        {
            if (string.IsNullOrEmpty(_gameState.Inventory[i].ItemType))
            {
                playerInventory.Items[i].InitUi(null, 0);
                playerItemBox.PlayerItems[i].InitUi(null, 0);
                continue;
            }

            var item = ItemGenerator.CreateItem(_gameState.Inventory[i].ItemType);
            playerInventory.Items[i].InitUi(item, _gameState.Inventory[i].Qty);
            playerItemBox.PlayerItems[i].InitUi(item, _gameState.Inventory[i].Qty);
            playerInventory.ItemDirty[i] = true;
        }
    }

    private void LoadItemBox(PlayerItemBoxControl playerItemBox)
    {
        //GD.Print($"LoadItemBox for {playerItemBox.ItemBoxItems.Length} slots...");
        for (var i = 0; i < playerItemBox.ItemBoxItems.Length; i++)
        {
            if (string.IsNullOrEmpty(_gameState.ItemBox[i].ItemType))
            {
                playerItemBox.ItemBoxItems[i].InitUi(null, 0);
                continue;
            }

            GD.Print($"Found Item '{_gameState.ItemBox[i].ItemType}' in item box slot {i}");

            var item = ItemGenerator.CreateItem(_gameState.ItemBox[i].ItemType);
            playerItemBox.ItemBoxItems[i].InitUi(item, _gameState.ItemBox[i].Qty);
        }
    }

    public class GameState
    {
        public SceneLoadData SceneLoadData;
        public ItemState[] Inventory;
        public ItemState[] ItemBox;
        public int? EquipedWeaponIndex;
        public double Health;
        public int[] DeadEnemies;
        public int[] GrabbedItems;
        public int[] TriggeredEvents;
        public int[] DoorsUnlocked;
        public int[] CutscenesWatched;
        public NoteData[] NotesCollected;
        public int[] MapsCollected;
        public int[] RoomsVisited;
        public int[] RoomsCleared;
        public int[] DoorsFound;
        public int[] DoorsEntered;
        public int[] LockedDoorsInspected;
    }

    public class ItemState
    {
        public string ItemType;
        public int Qty;
    }
}

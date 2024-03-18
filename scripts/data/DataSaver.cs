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

        _gameState = new GameState
        {
            DeadEnemies = new int[0],
            DoorsUnlocked = new int[0],
            GrabbedItems = new int[0],
            TriggeredEvents = new int[0],
            Inventory = new ItemState[0],
            ItemBox = new ItemState[0],
            Health = PlayerStatus.MaxHealth,
            SceneLoadData = new SceneLoadData
            {
                TargetScene = GetTree().CurrentScene.Name,
                LoadPosition = Vector3.Zero,
                LoadRotation = Vector3.Zero,
            },
            EquipedWeaponIndex = null,
        };

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

    public void SaveGameStateFromScene(PlayerStatus playerStatus, PlayerInventory playerInventory, SceneLoadData sceneLoadData, PlayerItemBoxControl playerItemBox)
    {
        SavePlayerStatus(playerStatus, playerInventory);
        SaveInventory(playerInventory);
        SaveItemBox(playerItemBox);
        SaveSceneLoadData(sceneLoadData);
    }

    private void SavePlayerStatus(PlayerStatus playerStatus, PlayerInventory playerInventory)
    {
        _gameState.Health = playerStatus.Health;
        _gameState.DeadEnemies = _gameState.DeadEnemies.Union(playerStatus.DeadEnemies).Distinct().ToArray();
        _gameState.DoorsUnlocked = _gameState.DoorsUnlocked.Union(playerStatus.DoorsUnlocked).Distinct().ToArray();
        _gameState.GrabbedItems = _gameState.GrabbedItems.Union(playerStatus.GrabbedItems).Distinct().ToArray();
        _gameState.TriggeredEvents = _gameState.TriggeredEvents.Union(playerStatus.TriggeredEvents.Select(e => (int)e)).Distinct().ToArray();
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
        GD.Print($"Saved {_gameState.ItemBox.Length} ItemBox items to _gameState...");
    }

    private void SaveSceneLoadData(SceneLoadData sceneLoadData)
    {
        _gameState.SceneLoadData = sceneLoadData;
    }

    public void LoadGameStateFromFileData(GameState data)
    {
        _gameState = data;
        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
        var playerItemBox = GetNode<PlayerItemBoxControl>(NodePaths.FromSceneRoot.ItemBoxControl);
        LoadFromGameState(playerStatus, playerInventory, playerItemBox);
    }

    public void LoadFromGameState(PlayerStatus playerStatus, PlayerInventory playerInventory, PlayerItemBoxControl playerItemBox)
    {
        LoadInventory(playerInventory, playerItemBox);
        LoadItemBox(playerItemBox);
        LoadPlayerStatus(playerStatus, playerInventory);
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
    }

    public class ItemState
    {
        public string ItemType;
        public int Qty;
    }
}

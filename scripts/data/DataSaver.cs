using Godot;
using System.Collections.Generic;
using System.Linq;
using static GameConstants;

public partial class DataSaver : Node3D
{
    private static DataSaver Instance;

    private GameState _gameState;

    public static DataSaver GetInstance()
    {
        return Instance;
    }

    public override void _Ready()
	{
        if(Instance != null)
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
            Inventory = null,
            Health = PlayerStatus.MaxHealth,
            SceneLoadData = new SceneLoadData
            {
                TargetScene = GetTree().CurrentScene.Name,
                LoadPosition = Vector3.Zero,
                LoadRotation = Vector3.Zero,
            },
            EquipedWeaponIndex = null,
        };
    }

    public GameState GetGameState()
    {
        return _gameState;
    }

    public void SaveGameStateFromScene(PlayerStatus playerStatus, PlayerInventory playerInventory, SceneLoadData sceneLoadData)
    {
        SavePlayerStatus(playerStatus, playerInventory);
        SaveInventory(playerInventory);
        SaveSceneLoadData(sceneLoadData);
    }

    private void SavePlayerStatus(PlayerStatus playerStatus, PlayerInventory playerInventory)
    {
        _gameState.Health = playerStatus.Health;
        _gameState.DeadEnemies = _gameState.DeadEnemies.Union(playerStatus.KilledEnemies).Distinct().ToArray();
        _gameState.DoorsUnlocked = _gameState.DoorsUnlocked.Union(playerStatus.DoorsUnlocked).Distinct().ToArray();
        _gameState.GrabbedItems = _gameState.GrabbedItems.Union(playerStatus.GrabbedItems).Distinct().ToArray();
        _gameState.TriggeredEvents = _gameState.TriggeredEvents.Union(playerStatus.TriggeredEvents.Select(e => (int)e)).Distinct().ToArray();
        if (playerStatus.EquipedWeapon != null)
            for (var i = 0; i < playerInventory.Items.Length; i++)
                if (playerInventory.Items[i].Item != null && playerStatus.EquipedWeapon.GetInstanceId() == playerInventory.Items[i].Item.GetInstanceId())
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

    private void SaveSceneLoadData(SceneLoadData sceneLoadData)
    {
        _gameState.SceneLoadData = sceneLoadData;
    }

    public void LoadGameStateFromFileData(GameState data)
    {
        _gameState = data;
        var playerStatus = PlayerStatus.GetInstance();
        var playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
        LoadFromGameState(playerStatus, playerInventory);
    }

    public void LoadFromGameState(PlayerStatus playerStatus, PlayerInventory playerInventory)
    {
        LoadInventory(playerInventory);
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
            {
                GD.Print("Tried to equip non-weapon item from item slot #" + _gameState.EquipedWeaponIndex);
            }
        }
    }

    private void LoadInventory(PlayerInventory playerInventory)
    {
        if (_gameState.Inventory == null)
            return;

        for (var i = 0; i < playerInventory.Items.Length; i++)
        {
            if (string.IsNullOrEmpty(_gameState.Inventory[i].ItemType))
                continue;

            playerInventory.Items[i].Item = ItemGenerator.CreateItem(_gameState.Inventory[i].ItemType);
            playerInventory.Items[i].ItemSprite.Texture = playerInventory.Items[i].Item.MenuIcon;
            // TODO: Probably don't need this in Godot?
            //playerInventory.Items[i].ItemSprite.Color = Color.White;
            if (playerInventory.Items[i].Item is Weapon)
            {
                (playerInventory.Items[i].Item as Weapon).Ammo = _gameState.Inventory[i].Qty;
                playerInventory.Items[i].Qty = 1;
            }
            else
                playerInventory.Items[i].Qty = _gameState.Inventory[i].Qty;

            playerInventory.ItemDirty[i] = true;
        }
    }

    public class GameState
    {
        public SceneLoadData SceneLoadData;
        public ItemState[] Inventory;
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
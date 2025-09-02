using Godot;
using static GameConstants;

public partial class ItemSpawnPoint : Node3D
{
    [Export(hintString: "Unique identifier for the item, use this number ONCE per the entire game!")]
    private int ItemId;
    [Export]
    private int QtyOnPickup = 1;
    [Export]
    private PickupType PickupType;

    [Export]
    private ItemSpawnType ItemSpawnOnEasy;
    [Export]
    private ItemSpawnType ItemSpawnOnNormal;
    [Export]
    private ItemSpawnType ItemSpawnOnHard;
    [Export]
    private ItemSpawnType ItemSpawnOnImpossible;

    public override void _Ready()
	{
        var playerStatus = PlayerStatus.GetInstance();
        if (playerStatus.GrabbedItems.Contains(ItemId)) return;

        ItemSpawnType itemSpawnType;
        switch (playerStatus.GameSettings.GameDifficulty)
        {
            case GameDifficulty.Easy:
                itemSpawnType = ItemSpawnOnEasy;
                break;
            case GameDifficulty.Normal:
                itemSpawnType = ItemSpawnOnNormal;
                break;
            case GameDifficulty.Hard:
                itemSpawnType = ItemSpawnOnHard;
                break;
            case GameDifficulty.Impossible:
            default:
                itemSpawnType = ItemSpawnOnImpossible;
                break;
        }

        if (playerStatus.GameSettings.IsRandomized && playerStatus.GameSettings.RandomizerSeed.RandomizedItems.ContainsKey(ItemId) &&
            (itemSpawnType != ItemSpawnType.None || playerStatus.GameSettings.RandomizerSeed.AllowSpawnsOnEmptyItemSlotsForDifficulty))
        {
            itemSpawnType = playerStatus.GameSettings.RandomizerSeed.RandomizedItems[ItemId];
            if (playerStatus.GameSettings.RandomizerSeed.RandomizedItemQty.ContainsKey(ItemId))
                QtyOnPickup = playerStatus.GameSettings.RandomizerSeed.RandomizedItemQty[ItemId];
            else
                QtyOnPickup = 1;
        }

        if (itemSpawnType != ItemSpawnType.None)
            SpawnItem(itemSpawnType);
    }

    private void SpawnItem(ItemSpawnType itemSpawnType)
    {
        if (!ItemPrefabLookup.ContainsKey(itemSpawnType))
        {
            GD.PrintErr($"Failed to spawn item, no prefab path found for item type '{itemSpawnType}'.");
            return;
        }

        var itemSceneLoad = GD.Load<PackedScene>(ItemPrefabLookup[itemSpawnType]);
        if (itemSceneLoad == null)
        {
            GD.PrintErr($"Failed to spawn item of type '{itemSpawnType}', scene not found '{ItemPrefabLookup[itemSpawnType]}' .");
            return;
        }
        var itemScene = itemSceneLoad.Instantiate();
        var itemContainer = itemScene as ItemContainer;
        if (itemContainer == null || itemContainer.Item == null)
        {
            GD.PrintErr($"Failed to spawn item of type '{itemSpawnType}'. Scene '{ItemPrefabLookup[itemSpawnType]}' does not have an ItemContainer script at its root node or Item parameter on it is null!");
            return;
        }

        itemContainer.Item.ItemId = ItemId;
        itemContainer.Item.QtyOnPickup = QtyOnPickup;
        itemContainer.Item.PickupType = PickupType;

        AddChild(itemContainer);
    }
}

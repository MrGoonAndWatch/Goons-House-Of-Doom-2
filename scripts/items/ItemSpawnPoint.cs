using Godot;

public partial class ItemSpawnPoint : Node3D
{
    [Export(hintString: "Unique identifier for the item, use this number ONCE per the entire game!")]
    private int ItemId;
    [Export]
    private int QtyOnPickup = 1;

    [Export]
    private GameConstants.ItemSpawnType ItemSpawnOnEasy;
    [Export]
    private GameConstants.ItemSpawnType ItemSpawnOnNormal;
    [Export]
    private GameConstants.ItemSpawnType ItemSpawnOnHard;
    [Export]
    private GameConstants.ItemSpawnType ItemSpawnOnImpossible;

    public override void _Ready()
	{
        if (PlayerStatus.GetInstance().GrabbedItems.Contains(ItemId)) return;

        GameConstants.ItemSpawnType itemSpawnType;
        switch (PlayerStatus.GetInstance().GameDifficulty)
        {
            case GameConstants.GameDifficulty.Easy:
                itemSpawnType = ItemSpawnOnEasy;
                break;
            case GameConstants.GameDifficulty.Normal:
                itemSpawnType = ItemSpawnOnNormal;
                break;
            case GameConstants.GameDifficulty.Hard:
                itemSpawnType = ItemSpawnOnHard;
                break;
            case GameConstants.GameDifficulty.Impossible:
            default:
                itemSpawnType = ItemSpawnOnImpossible;
                break;
        }

        // TODO: Get randomizer settings to check if this needs to be swapped.

        if (itemSpawnType != GameConstants.ItemSpawnType.None)
            SpawnItem(itemSpawnType);
    }

    private void SpawnItem(GameConstants.ItemSpawnType itemSpawnType)
    {
        if (!GameConstants.ItemPrefabLookup.ContainsKey(itemSpawnType))
        {
            GD.PrintErr($"Failed to spawn item, no prefab path found for item type '{itemSpawnType}'.");
            return;
        }

        var itemSceneLoad = GD.Load<PackedScene>(GameConstants.ItemPrefabLookup[itemSpawnType]);
        if (itemSceneLoad == null)
        {
            GD.PrintErr($"Failed to spawn item of type '{itemSpawnType}', scene not found '{GameConstants.ItemPrefabLookup[itemSpawnType]}' .");
            return;
        }
        var itemScene = itemSceneLoad.Instantiate();
        var itemContainer = itemScene as ItemContainer;
        if (itemContainer == null || itemContainer.Item == null)
        {
            GD.PrintErr($"Failed to spawn item of type '{itemSpawnType}'. Scene '{GameConstants.ItemPrefabLookup[itemSpawnType]}' does not have an ItemContainer script at its root node or Item parameter on it is null!");
            return;
        }

        itemContainer.Item.ItemId = ItemId;
        itemContainer.Item.QtyOnPickup = QtyOnPickup;

        AddChild(itemContainer);
    }
}

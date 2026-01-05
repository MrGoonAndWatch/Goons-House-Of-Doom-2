public partial class PistolAmmo : Item
{
    public override ComboResult Combine(Item otherItem)
    {
        // TODO: Setup Combos w/ other items.
        return new ComboResult
        {
            ItemA = ItemGenerator.CreateItem(GameConstants.GarbagePrefabPath, 0),
            ItemB = null,
        };
    }

    public override string GetDescription()
    {
        return "Ammunition for the pistol";
    }

    public override string GetItemName()
    {
        return "Pistol Ammo";
    }

    public override int? GetMaxStackSize()
    {
        return 800;
    }

    public override string GetPrefabPath()
    {
        return GameConstants.ItemPrefabLookup[GameConstants.ItemSpawnType.PistolAmmo];
    }

    public override string GetExamineModelPrefab()
    {
        return "res://models/shambler.tscn";
    }

    public override bool IsStackable()
    {
        return true;
    }

    public override bool UseItem()
    {
        return false;
    }
}

using Godot;
using System;

public partial class GreenJuice : Item
{
    public override string GetDescription()
    {
        return "A special mixture of chemicals that treat injuries almost instantly.";
    }

    public override string GetPrefabPath()
    {
        return GameConstants.ItemPrefabLookup[GameConstants.ItemSpawnType.GreenJuice];
    }

    public override bool IsStackable()
    {
        return false;
    }

    public override int? GetMaxStackSize()
    {
        return null;
    }

    public override bool UseItem()
    {
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.AddHealth(GameConstants.GreenMedicineHp);
        return true;
    }

    public override ComboResult Combine(Item otherItem)
    {
        // TODO: Setup Combos w/ other items.
        return new ComboResult
        {
            ItemA = ItemGenerator.CreateItem(GameConstants.GarbagePrefabPath),
            ItemB = null,
        };
    }
}

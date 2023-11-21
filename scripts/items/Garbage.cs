public partial class Garbage : Item
{
    public override string GetDescription()
    {
        return "It's a useless pile of junk.";
    }

    public override string GetPrefabPath()
    {
        return GameConstants.GarbagePrefabPath;
    }

    public override bool IsStackable()
    {
        return true;
    }

    public override int? GetMaxStackSize()
    {
        return 99;
    }

    public override bool UseItem()
    {
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.SetHealth(40);
        return true;
    }

    public override ComboResult Combine(Item otherItem)
    {
        return new ComboResult
        {
            ItemA = this,
            ItemB = otherItem,
        };
    }
}

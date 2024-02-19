using static GameConstants;

public partial class BigKey : Key
{
    public override string GetItemName()
    {
        return "Big Key";
    }

    public override string GetDescription()
    {
        return "Wow! This key is huge!";
    }

    public override string GetPrefabPath()
    {
        return ItemPrefabFolderPath + "big-key.tscn";
    }

    public override KeyType GetKeyType()
    {
        return KeyType.BigKey;
    }
}

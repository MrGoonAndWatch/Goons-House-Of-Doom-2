using static GameConstants;

public partial class BigKey : Key
{
    public override string GetDescription()
    {
        return "Wow! This key is huge!";
    }

    public override string GetPrefabPath()
    {
        return ItemPrefabFolderPath + "BigKey.tscn";
    }

    public override KeyType GetKeyType()
    {
        return KeyType.BigKey;
    }
}

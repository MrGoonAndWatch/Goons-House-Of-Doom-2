using static GameConstants;

public partial class MuseumFrontDoorKey : Key
{
    public override string GetItemName()
    {
        return "Museum Key";
    }

    public override string GetDescription()
    {
        return "Looks like the key to the museum!";
    }

    public override string GetPrefabPath()
    {
        return ItemPrefabFolderPath + "museum-front-door-key.tscn";
    }

    public override KeyType GetKeyType()
    {
        return KeyType.MuseumFrontDoor;
    }
}

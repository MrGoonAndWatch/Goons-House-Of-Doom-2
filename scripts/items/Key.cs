using static GameConstants;

public abstract partial class Key : Item
{
    private PlayerInteract _useKey;

    public override void _Ready()
    {
        _useKey = GetNode<PlayerInteract>(NodePaths.FromSceneRoot.PlayerInteract);
    }

    public abstract KeyType GetKeyType();

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
        // HACK: due to the items not existing in scene when loading between rooms we need to call a persistent object to retrieve nodes in the scene, thus we're delagating this call off to PlayerStatus.
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.UseKey(this);
        return false;
    }

    public override ComboResult Combine(Item otherItem)
    {
        return new ComboResult
        {
            ItemA = ItemGenerator.CreateItem(new Garbage().GetPrefabPath(), 0),
            ItemB = null,
        };
    }
}

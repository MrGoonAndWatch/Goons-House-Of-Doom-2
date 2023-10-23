using static GameConstants;

public abstract partial class Key : Item
{
    private PlayerInteract _useKey;

    void Start()
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
        // HACK: when key is created by DataSaver Start() is not called and this doesn't get properly initialized.
        if (_useKey == null)
            _useKey = _useKey = GetNode<PlayerInteract>(NodePaths.FromSceneRoot.PlayerInteract);
        _useKey.Use(this);
        return false;
    }

    public override ComboResult Combine(Item otherItem)
    {
        return new ComboResult
        {
            ItemA = ItemGenerator.CreateItem(new Garbage().GetPrefabPath()),
            ItemB = null,
        };
    }
}

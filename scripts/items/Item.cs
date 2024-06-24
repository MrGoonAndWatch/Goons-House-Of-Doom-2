using Godot;

public abstract partial class Item : Area3D
{
    [Export]
    public AnimatedSprite3D aniSprite;

    public int ItemId;
	public int QtyOnPickup;
    [Export]
    public Texture2D MenuIcon;

    public override void _Ready() {
        if (aniSprite != null)
            aniSprite.Play();
    }

    public abstract string GetItemName();
    public abstract string GetDescription();

    protected const string ItemPrefabFolderPath = "res://prefabs/spawnables/items/";

    public abstract string GetPrefabPath();
    public abstract bool IsStackable();
    public abstract int? GetMaxStackSize();

    public abstract bool UseItem();
    public abstract ComboResult Combine(Item otherItem);

    public void ForceDestroy()
    {
        if (this is Weapon) (this as Weapon).AddAmmo(QtyOnPickup);
        GetParent().QueueFree();
    }
}

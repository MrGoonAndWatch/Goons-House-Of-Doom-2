using Godot;

public abstract partial class Item : Node3D
{
	[Export(hintString: "Unique identifier for the item, use this number ONCE per the entire game!")]
	public int ItemId;
	[Export]
	public int QtyOnPickup = 1;
    // TODO: Check if this is the right way to store this, can it be staticly defined by inherited classes?
    [Export]
    public Texture2D MenuIcon;

    public abstract string GetDescription();

    protected const string ItemPrefabFolderPath = "res://prefabs/spawnables/items/";

    public abstract string GetPrefabPath();
    public abstract bool IsStackable();
    public abstract int? GetMaxStackSize();

    public abstract bool UseItem();
    public abstract ComboResult Combine(Item otherItem);

    public void ForceDestroy()
    {
        // TODO: Check if we actually want to hard delete this or just set visible = false.
        QueueFree();
    }
}

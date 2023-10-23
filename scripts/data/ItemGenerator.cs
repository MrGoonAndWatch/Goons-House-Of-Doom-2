using Godot;
using System;

public partial class ItemGenerator : Node3D
{
    private static ItemGenerator Instance;

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public static Item CreateItem(string itemType)
    {
        if (Instance == null)
        {
            GD.PrintErr("Could not create item, no instance of ItemGenerator has been initialized!");
            throw new ApplicationException("Could not create item, no instance of ItemGenerator has been initialized!");
        }

        var resource = GD.Load<PackedScene>(itemType);
        var itemObj = resource.Instantiate();
        Instance.AddChild(itemObj);
        var item = itemObj as Item;
        if (item == null)
            throw new ApplicationException("Could not find item at the root on resource for prefab: " + itemType);
        return item;
    }
}

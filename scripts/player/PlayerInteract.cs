using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerInteract : Node
{
    [Export]
    private PlayerInventory _inventory;
    private PlayerStatus _playerStatus;

    private List<Item> _touchingItems;

    public override void _Ready()
	{
        //_inventory = GetParent().GetNode<PlayerInventory>(GameConstants.NodePaths.FromPlayerRoot.PlayerInventory);
        _playerStatus = PlayerStatus.GetInstance();

        _touchingItems = new List<Item>();
    }

	public override void _Process(double delta)
	{
        if (/*_playerStatus.CanInteract() &&*/ _touchingItems.Any() &&
            !Input.IsActionPressed(GameConstants.Controls.Aim) &&
            Input.IsActionJustPressed(GameConstants.Controls.Confirm))
            PickupCurrentItem();
    }

    public void ResetState()
    {
        _touchingItems.RemoveAll(i => true);
    }

    public void _OnBodyEntered(Node3D item)
    {
        if(item is Item)
            _touchingItems.Add(item as Item);
    }

    public void _OnBodyExited(Node3D item)
    {
        if(item is Item)
            RemoveItem(item);
    }

    void PickupCurrentItem()
    {
        var validItems = _touchingItems.Where(i => i != null).ToArray();
        if (!validItems.Any())
            return;

        var item = validItems.Last();
        
        var remainingQty = _inventory.AddItem(item);
        if (item.ItemId != 0 && remainingQty != item.QtyOnPickup)
        {
            //_playerStatus.GrabItem(item.ItemId);
        }

        // TODO: Confirm we shouldn't just set visible = false here instead!
        if (remainingQty == 0)
            item.QueueFree();
        else
            item.QtyOnPickup = remainingQty;

        RemoveItem(item);
    }

    private void RemoveItem(Node3D item)
    {
        _touchingItems.RemoveAll(i => i.GetInstanceId() == item.GetInstanceId());
    }
}

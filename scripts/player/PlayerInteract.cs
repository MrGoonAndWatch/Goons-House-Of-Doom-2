using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerInteract : Node
{
    [Export]
    private PlayerInventory _inventory;
    private PlayerStatus _playerStatus;

    private List<Item> _touchingItems;
    private List<SimpleLock> _collidedSimpleLocks;
    private List<Door> _collidedDoors;

    public override void _Ready()
	{
        _playerStatus = PlayerStatus.GetInstance();
        _touchingItems = new List<Item>();
        _collidedSimpleLocks = new List<SimpleLock>();
        _collidedDoors = new List<Door>();
    }

	public override void _Process(double delta)
	{
        if (_playerStatus.CanInteract() &&
            !Input.IsActionPressed(GameConstants.Controls.Aim) &&
            Input.IsActionJustPressed(GameConstants.Controls.Confirm))
        {
            if(_touchingItems.Any())
                PickupCurrentItem();
            else if (_collidedSimpleLocks.Any())
                _collidedSimpleLocks.First().Inspect();
            else if (_collidedDoors.Any())
                _collidedDoors.First().Inspect();
        }
    }

    public void ResetState()
    {
        _touchingItems.RemoveAll(i => true);
        _collidedDoors.RemoveAll(d => true);
        _collidedSimpleLocks.RemoveAll(l => true);
    }

    public void _OnBodyEntered(Node3D obj)
    {
        if(obj is Item)
            _touchingItems.Add(obj as Item);
        if (obj is SimpleLock)
            _collidedSimpleLocks.Add(obj as SimpleLock);
        if (obj is Door)
            _collidedDoors.Add(obj as Door);
    }

    public void _OnBodyExited(Node3D obj)
    {
        if(obj is Item)
            RemoveItem(obj);
        if (obj is SimpleLock)
            _collidedSimpleLocks.RemoveAll(l => l.GetInstanceId() == obj.GetInstanceId());
        if (obj is Door)
            _collidedDoors.RemoveAll(d => d.GetInstanceId() == obj.GetInstanceId());
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

    public void Use(Key key)
    {
        if (_collidedSimpleLocks.Any())
            _collidedSimpleLocks.First().Unlock(key);
        else if (_collidedDoors.Any())
            _collidedDoors.First().Unlock(key);
    }
}

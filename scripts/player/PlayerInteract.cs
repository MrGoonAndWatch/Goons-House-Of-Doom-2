using Godot;
using System;
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
    private List<ItemBox> _collidedItemBoxes;

    public override void _Ready()
	{
        _playerStatus = PlayerStatus.GetInstance();
        _touchingItems = new List<Item>();
        _collidedSimpleLocks = new List<SimpleLock>();
        _collidedDoors = new List<Door>();
        _collidedItemBoxes = new List<ItemBox>();
    }

	public override void _Process(double delta)
	{
        if (_playerStatus.CanInteract() &&
            !Input.IsActionPressed(GameConstants.Controls.aim.ToString()) &&
            Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
        {
            if (_touchingItems.Any())
                PickupCurrentItem();
            else if (_collidedSimpleLocks.Any())
                _collidedSimpleLocks.First().Inspect();
            else if (_collidedItemBoxes.Any())
                _collidedItemBoxes.First().OpenBox();
            else if (_collidedDoors.Any())
                _collidedDoors.First().Inspect();
        }
    }

    public void ResetState()
    {
        _touchingItems.RemoveAll(_ => true);
        _collidedDoors.RemoveAll(_ => true);
        _collidedSimpleLocks.RemoveAll(_ => true);
        _collidedItemBoxes.RemoveAll(_ => true);
    }

    public void _OnBodyEntered(Node3D obj)
    {
        if (obj is SimpleLock)
            _collidedSimpleLocks.Add(obj as SimpleLock);
        if (obj is Door)
            _collidedDoors.Add(obj as Door);
        if (obj is ItemBox)
            _collidedItemBoxes.Add(obj as ItemBox);
    }

    public void _OnBodyExited(Node3D obj)
    {
        var matchByInstanceId = new Predicate<GodotObject>(o => o.GetInstanceId() == obj.GetInstanceId());
        if (obj is SimpleLock)
            _collidedSimpleLocks.RemoveAll(matchByInstanceId);
        if (obj is Door)
            _collidedDoors.RemoveAll(matchByInstanceId);
        if (obj is ItemBox)
            _collidedItemBoxes.RemoveAll(matchByInstanceId);
    }

    public void _OnAreaEntered(Area3D obj)
    {
        if (obj is Item)
            _touchingItems.Add(obj as Item);
    }

    public void _OnAreaExited(Area3D obj)
    {
        if (obj is Item)
            RemoveItem(obj as Item);
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
            _playerStatus.GrabItem(item.ItemId);
        }

        if (remainingQty == 0)
            item.ForceDestroy();
        else
            item.QtyOnPickup = remainingQty;

        RemoveItem(item);
    }

    private void RemoveItem(Item item)
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

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerInteract : Node
{
    [Export]
    private PlayerInventory _inventory;
    [Export]
    private NoteReader _noteReader;
    private PlayerStatus _playerStatus;

    private List<Item> _touchingItems;
    private List<NotePickup> _touchingNotes;
    private List<SimpleLock> _touchingSimpleLocks;
    private List<Door> _touchingDoors;
    private List<ItemBox> _touchingItemBoxes;
    private List<PassCode> _touchingPassCodes;

    public override void _Ready()
	{
        _playerStatus = PlayerStatus.GetInstance();
        _touchingItems = new List<Item>();
        _touchingNotes = new List<NotePickup>();
        _touchingSimpleLocks = new List<SimpleLock>();
        _touchingDoors = new List<Door>();
        _touchingItemBoxes = new List<ItemBox>();
        _touchingPassCodes = new List<PassCode>();
    }

	public override void _Process(double delta)
	{
        if (_playerStatus.CanInteract() &&
            !Input.IsActionPressed(GameConstants.Controls.aim.ToString()) &&
            Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
        {
            if (_touchingItems.Any())
                PickupCurrentItem();
            else if (_touchingNotes.Any())
                PickupCurrentNote();
            else if (_touchingSimpleLocks.Any())
                _touchingSimpleLocks.First().Inspect();
            else if (_touchingPassCodes.Any())
                _touchingPassCodes.First().Inspect();
            else if (_touchingItemBoxes.Any())
                _touchingItemBoxes.First().OpenBox();
            else if (_touchingDoors.Any())
                _touchingDoors.First().Inspect();
        }
    }

    public void ResetState()
    {
        _touchingItems.RemoveAll(_ => true);
        _touchingNotes.RemoveAll(_ => true);
        _touchingDoors.RemoveAll(_ => true);
        _touchingSimpleLocks.RemoveAll(_ => true);
        _touchingItemBoxes.RemoveAll(_ => true);
        _touchingPassCodes.RemoveAll(_ => true);
    }

    public void _OnBodyEntered(Node3D obj)
    {
        if (obj is SimpleLock)
            _touchingSimpleLocks.Add(obj as SimpleLock);
        if (obj is Door)
            _touchingDoors.Add(obj as Door);
        if (obj is ItemBox)
            _touchingItemBoxes.Add(obj as ItemBox);
        if (obj is PassCode)
            _touchingPassCodes.Add(obj as PassCode);
    }

    public void _OnBodyExited(Node3D obj)
    {
        var matchByInstanceId = new Predicate<GodotObject>(o => o.GetInstanceId() == obj.GetInstanceId());
        if (obj is SimpleLock)
            _touchingSimpleLocks.RemoveAll(matchByInstanceId);
        if (obj is Door)
            _touchingDoors.RemoveAll(matchByInstanceId);
        if (obj is ItemBox)
            _touchingItemBoxes.RemoveAll(matchByInstanceId);
        if (obj is PassCode)
            _touchingPassCodes.RemoveAll(matchByInstanceId);
    }

    public void _OnAreaEntered(Area3D obj)
    {
        if (obj is Item)
            _touchingItems.Add(obj as Item);
        if (obj is NotePickup)
            _touchingNotes.Add(obj as NotePickup);
    }

    public void _OnAreaExited(Area3D obj)
    {
        var matchByInstanceId = new Predicate<GodotObject>(o => o.GetInstanceId() == obj.GetInstanceId());
        if (obj is Item)
            _touchingItems.RemoveAll(matchByInstanceId);
        if (obj is NotePickup)
            _touchingNotes.RemoveAll(matchByInstanceId);
    }

    void PickupCurrentItem()
    {
        var validItems = _touchingItems.Where(i => i != null).ToArray();
        if (!validItems.Any())
            return;

        var item = validItems.First();
        
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

    private void PickupCurrentNote()
    {
        var validNotes = _touchingNotes.Where(i => i != null).ToArray();
        if (!validNotes.Any())
            return;

        var note = validNotes.First();

        PlayerStatus.CollectNote(note.NoteData);
        _noteReader.StartReadingNote(note.NoteData);

        var noteParent = note.GetParent();
        _touchingNotes.RemoveAll(i => i.GetInstanceId() == note.GetInstanceId());
        noteParent.QueueFree();
    }

    private void RemoveItem(Item item)
    {
        _touchingItems.RemoveAll(i => i.GetInstanceId() == item.GetInstanceId());
    }

    public void UseKey(Key key)
    {
        if (_touchingSimpleLocks.Any())
            _touchingSimpleLocks.First().Unlock(key);
        else if (_touchingDoors.Any())
            _touchingDoors.First().Unlock(key);
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameConstants;

public partial class PlayerInteract : Area3D
{
    [Export]
    private PlayerInventory _inventory;
    [Export]
    private NoteReader _noteReader;
    [Export]
    private InspectTextUi _inspectTextUi;
    [Export]
    private SaveGame _saveUi;
    private PlayerStatus _playerStatus;

    private List<Item> _touchingItems;
    private List<NotePickup> _touchingNotes;
    private List<MapPickup> _touchingMaps;
    private List<SimpleLock> _touchingSimpleLocks;
    private List<Door> _touchingDoors;
    private List<ItemBox> _touchingItemBoxes;
    private List<PassCode> _touchingPassCodes;
    private List<ShowTextOnInspectHitbox> _touchingShowTextOnInspects;
    private List<SaveOnInspect> _touchingSaveOnInspects;

    public override void _Ready()
	{
        _playerStatus = PlayerStatus.GetInstance();
        _touchingItems = new List<Item>();
        _touchingNotes = new List<NotePickup>();
        _touchingMaps = new List<MapPickup>();
        _touchingSimpleLocks = new List<SimpleLock>();
        _touchingDoors = new List<Door>();
        _touchingItemBoxes = new List<ItemBox>();
        _touchingPassCodes = new List<PassCode>();
        _touchingShowTextOnInspects = new List<ShowTextOnInspectHitbox>();
        _touchingSaveOnInspects = new List<SaveOnInspect>();
    }

	public override void _Process(double delta)
	{
        if (_playerStatus.CanInteract() &&
            !Input.IsActionPressed(Controls.aim.ToString()) &&
            Input.IsActionJustPressed(Controls.confirm.ToString()))
        {
            if (_touchingSaveOnInspects.Any())
                _touchingSaveOnInspects.First().StartSave(_inspectTextUi, _saveUi);
            else if (_touchingItems.Any())
                PickupCurrentItem();
            else if (_touchingNotes.Any())
                PickupCurrentNote();
            else if (_touchingMaps.Any())
                PickupCurrentMap();
            else if (_touchingSimpleLocks.Any())
                _touchingSimpleLocks.First().Inspect(_inspectTextUi);
            else if (_touchingPassCodes.Any())
                _touchingPassCodes.First().Inspect(_inspectTextUi);
            else if (_touchingItemBoxes.Any())
                _touchingItemBoxes.First().OpenBox();
            else if (_touchingDoors.Any())
                _touchingDoors.First().Inspect(_inspectTextUi);
            else if (_touchingShowTextOnInspects.Any())
                _touchingShowTextOnInspects.First().ShowTextOnInspect.StartInspection(_inspectTextUi);
        }
    }

    // TODO: This _should_ be not needed any longer because this script runs on a per-scene existance, unlike the Unity ver. where the entire player became a singleton on game start.
    public void ResetState()
    {
        _touchingItems.RemoveAll(_ => true);
        _touchingNotes.RemoveAll(_ => true);
        _touchingDoors.RemoveAll(_ => true);
        _touchingSimpleLocks.RemoveAll(_ => true);
        _touchingItemBoxes.RemoveAll(_ => true);
        _touchingPassCodes.RemoveAll(_ => true);
        _touchingShowTextOnInspects.RemoveAll(_ => true);
        _touchingSaveOnInspects.RemoveAll(_ => true);
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
        if (obj is SaveOnInspect)
            _touchingSaveOnInspects.Add(obj as SaveOnInspect);
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
        if (obj is SaveOnInspect)
            _touchingSaveOnInspects.RemoveAll(matchByInstanceId);
    }

    public void _OnAreaEntered(Area3D obj)
    {
        if (obj is Item)
            _touchingItems.Add(obj as Item);
        if (obj is NotePickup)
            _touchingNotes.Add(obj as NotePickup);
        if (obj is MapPickup)
            _touchingMaps.Add(obj as MapPickup);
        if (obj is ShowTextOnInspectHitbox)
            _touchingShowTextOnInspects.Add(obj as ShowTextOnInspectHitbox);
    }

    public void _OnAreaExited(Area3D obj)
    {
        var matchByInstanceId = new Predicate<GodotObject>(o => o.GetInstanceId() == obj.GetInstanceId());
        if (obj is Item)
            _touchingItems.RemoveAll(matchByInstanceId);
        if (obj is NotePickup)
            _touchingNotes.RemoveAll(matchByInstanceId);
        if (obj is MapPickup)
            _touchingMaps.RemoveAll(matchByInstanceId);
        if (obj is ShowTextOnInspectHitbox)
            _touchingShowTextOnInspects.RemoveAll(matchByInstanceId);
    }

    void PickupCurrentItem()
    {
        var validItems = _touchingItems.Where(i => i != null).ToArray();
        if (!validItems.Any())
            return;

        var item = validItems.First();
        // NOTE: This is jank, we are using the parent's instance id because we check for ItemPickup rather than Item since Item gets instantiated inside the player's inventory.
        //      THINGS WILL BREAK if you change the hirearchy of item prefabs.
        var itemInstanceId = item.GetParent().GetInstanceId();
        
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
        MapStatus.CheckForRoomCleared(itemInstanceId);
    }

    private void PickupCurrentNote()
    {
        var validNotes = _touchingNotes.Where(i => i != null).ToArray();
        if (!validNotes.Any())
            return;

        var note = validNotes.First();
        var noteInstanceId = note.GetInstanceId();

        PlayerStatus.CollectNote(note.NoteData);
        _noteReader.StartReadingNote(note.NoteData);

        var noteParent = note.GetParent();
        _touchingNotes.RemoveAll(i => i.GetInstanceId() == note.GetInstanceId());
        noteParent.QueueFree();
        MapStatus.CheckForRoomCleared(noteInstanceId);
    }

    private void PickupCurrentMap()
    {
        var validMaps = _touchingMaps.Where(i => i != null).ToArray();
        if (!validMaps.Any())
            return;

        var map = validMaps.First();
        var mapInstanceId = map.GetInstanceId();

        MapStatus.GetInstance().PickupMap(map.MapPickupData.AreaId);
        var inspectText = GetNode<InspectTextUi>(NodePaths.FromSceneRoot.InspectTextUi);
        inspectText.ReadText(new[] { $"Picked up the {map.MapPickupData.MapName} map" });

        var mapParent = map.GetParent();
        _touchingMaps.RemoveAll(i => i.GetInstanceId() == map.GetInstanceId());
        mapParent.QueueFree();
        MapStatus.CheckForRoomCleared(mapInstanceId);
    }

    private void RemoveItem(Item item)
    {
        _touchingItems.RemoveAll(i => i.GetInstanceId() == item.GetInstanceId());
    }

    public void UseKey(Key key)
    {
        if (_touchingSimpleLocks.Any())
            _touchingSimpleLocks.First().Unlock(key, _inspectTextUi);
        else if (_touchingDoors.Any())
            _touchingDoors.First().Unlock(key, _inspectTextUi);
    }
}

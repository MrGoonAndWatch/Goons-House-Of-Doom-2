using Godot;
using Godot.Collections;
using System.Diagnostics;
using System.Linq;

public partial class MapStatus : Node
{
    [Export]
    private Node MapParent;

    private static MapStatus Instance;

    private Dictionary<int, MapData> AreaToMapLookup;
    private Dictionary<int, int> RoomToAreaLookup;

    private bool _initialized;

    public System.Collections.Generic.List<int> _mapsCollected;
    public System.Collections.Generic.List<int> _roomsVisited;
    public System.Collections.Generic.List<int> _roomsCleared;
    public System.Collections.Generic.List<int> _doorsFound;
    public System.Collections.Generic.List<int> _doorsEntered;
    public System.Collections.Generic.List<int> _lockedDoorsInspected;

    private const string MapFolder = $"res://prefabs/maps/{GameConstants.Mode}/";

    public override void _Ready()
    {
        if (this == Instance) return;

        if (Instance != null)
        {
            GD.PrintErr($"Found multiple instances of MapStatus class! Terminating '{Name}'!");
            QueueFree();
        }

        Instance = this;

        InitializeMapStatus();
    }

    public void InitializeMapStatus()
    {
        var mapScenes = DirAccess.GetFilesAt(MapFolder);

        _mapsCollected = new System.Collections.Generic.List<int>();
        _roomsVisited = new System.Collections.Generic.List<int>();
        _roomsCleared = new System.Collections.Generic.List<int>();
        _doorsFound = new System.Collections.Generic.List<int>();
        _doorsEntered = new System.Collections.Generic.List<int>();
        _lockedDoorsInspected = new System.Collections.Generic.List<int>();

        AreaToMapLookup = new Dictionary<int, MapData>();
        RoomToAreaLookup = new Dictionary<int, int>();

        for (var i = 0; i < mapScenes.Length; i++)
        {
            if (!mapScenes[i].EndsWith(".tscn")) continue;

            var mapScene = GD.Load<PackedScene>($"{MapFolder}{mapScenes[i]}");
            var mapSceneLoaded = mapScene.Instantiate();
            var mapData = mapSceneLoaded as MapData;

            var areaId = mapData.AreaId;
            GD.Print($"Adding area {areaId}...");
            AreaToMapLookup.Add(areaId, mapData);
            for (var j = 0; j < mapData.RoomData.Length; j++)
            {
                var roomData = mapData.RoomData[j];
                if (roomData == null)
                {
                    GD.PrintErr($"RoomData at index {j} of area {mapData.AreaId} map ({mapData.AreaName}) is null! Please check the map scene's root object's 'RoomData' and remove null rows, though this shouldn't impact gameplay if you don't.");
                    continue;
                }

                GD.Print($"Mapping room {mapData.RoomData[j].RoomId} to area {areaId}");
                RoomToAreaLookup.Add(mapData.RoomData[j].RoomId, areaId);
            }
            // Note: this for loop is strictly just here for console warnings if a scene is setup weird. I was doing this in the spot where the actual null reference error occurs, but that causes the error to print out every time you open your menu.
            for (var j = 0; j < mapData.DoorData.Length; j++)
            {
                if (mapData.DoorData[j] == null)
                    GD.PrintErr($"Door at index {i} of area {mapData.AreaId} map ({mapData.AreaName}) is null! Please check this map's scene root object's 'DoorData' property and remove any null rows, though this shouldn't impact gameplay if you don't.");
            }
            MapParent.AddChild(mapData);
        }

        _initialized = true;
    }

    public void LoadMapData(System.Collections.Generic.List<int> mapsCollected, System.Collections.Generic.List<int> roomsVisited, System.Collections.Generic.List<int> roomsCleared)
    {
        _mapsCollected = mapsCollected;
        _roomsVisited = roomsVisited;
        _roomsCleared = roomsCleared;
    }

    public static MapStatus GetInstance()
    {
        if (Instance == null)
        {
            GD.PrintErr("No MapStatus Instance found!");
            return null;
        }

        return Instance;
    }

    public static void PrintDebug()
    {
        if (Instance == null)
        {
            GD.Print("MapStatus Instance is null!");
            return;
        }

        var mapsCollected = string.Join(",", Instance._mapsCollected);
        var roomsVisitedStr = string.Join(",", Instance._roomsVisited);
        var roomsClearedStr = string.Join(",", Instance._roomsCleared);
        GD.Print($"Maps collected = ({mapsCollected}) | rooms visited = ({roomsVisitedStr}) | rooms cleared = ({roomsClearedStr})");

    }

    public bool IsInitialized()
    {
        return _initialized;
    }

    public Dictionary<int, MapData> GetMapData()
    {
        return AreaToMapLookup;
    }

    public MapData GetMapDataForRoom(int roomId)
    {
        if (RoomToAreaLookup.ContainsKey(roomId))
            if (AreaToMapLookup.ContainsKey(RoomToAreaLookup[roomId]))
                return AreaToMapLookup[RoomToAreaLookup[roomId]];
            else
            {
                GD.PrintErr($"Found area id '{RoomToAreaLookup[roomId]}' for room '{roomId}' but didn't find that area in the area lookup!");
                return null;
            }
        else
        {
            GD.PrintErr($"Did not find area mapped to room '{roomId}' ({GameConstants.GetCurrentRoomName(this)}). Ensure this map has a room_info object with valid parameters!");
            return null;
        }
    }

    public bool HasMap(int areaId)
    {
        return GameConstants.ListContainsValue(areaId, _mapsCollected);
    }

    public bool VisitedRoom(int roomId)
    {
        var result = GameConstants.ListContainsValue(roomId, _roomsVisited);
        return result;
    }

    public bool ClearedRoom(int roomId)
    {
        return GameConstants.ListContainsValue(roomId, _roomsCleared);
    }

    public bool HasSeenDoor(int doorId)
    {
        return GameConstants.ListContainsValue(doorId, _doorsFound);
    }

    public bool EnteredDoor(int doorId)
    {
        return GameConstants.ListContainsValue(doorId, _doorsEntered);
    }

    public bool LockedDoorFound(int doorId)
    {
        return GameConstants.ListContainsValue(doorId, _lockedDoorsInspected);
    }

    public void PickupMap(int areaId)
    {
        if (HasMap(areaId)) return;
        _mapsCollected.Add(areaId);
    }

    public void VisitRoom(int roomId)
    {
        if (VisitedRoom(roomId)) return;
        _roomsVisited.Add(roomId);
    }

    public void MarkRoomCleared(int roomId)
    {
        var stack = new StackTrace();
        GD.Print($"MarkRoomCleared: {stack.ToString()}");
        if(ClearedRoom(roomId)) return;
        GD.Print($"Marked room {roomId} as cleared!");
        _roomsCleared.Add(roomId);
    }

    public void FoundDoor(int doorId)
    {
        if (HasSeenDoor(doorId)) return;
        _doorsFound.Add(doorId);
    }

    public void EnterDoor(int doorId)
    {
        if (EnteredDoor(doorId)) return;
        _doorsEntered.Add(doorId);
    }

    public void FoundLockedDoor(int doorId)
    {
        if (LockedDoorFound(doorId)) return;
        _lockedDoorsInspected.Add(doorId);
    }

    public void ReturnMaps()
    {
        foreach (var mapAreaId in AreaToMapLookup.Keys)
            AreaToMapLookup[mapAreaId].Reparent(MapParent);
    }

    // TODO: Revisit this approach. Cycling through every object in the scene can't be efficient but it is a nice way to dynamically handle checking whether a room is cleared.
    // TODO: Potentially skip checks on certain objects that have a lot of children but will never have an uncleared item in it (Player, enemies, etc.).
    public static void CheckForRoomCleared(ulong? ignoreNodeId = null)
    {
        var instance = GetInstance();
        if (instance == null || !instance.IsInitialized())
        {
            GD.PrintErr("Could not CheckForRoomCleared, MapStatus instance was null!");
            return;
        }

        var root = instance.GetNode(GameConstants.NodePaths.FromSceneRoot.SceneRoot);
        var containsPendingItem = ContainsPendingItem(root, ignoreNodeId);

        if (!containsPendingItem)
        {
            var roomId = GameConstants.GetCurrentRoomId(instance);
            instance.MarkRoomCleared(roomId);
            var mapData = instance.GetMapDataForRoom(roomId);
            mapData.RefreshMap();
        }
    }

    private static bool ContainsPendingItem(Node node, ulong? ignoreNodeId)
    {
        if (GameConstants.RoomClearCheckBlacklist.Contains(node.GetPath().GetConcatenatedNames()))
            return false;
        if ((ignoreNodeId == null || node.GetInstanceId() != ignoreNodeId.Value) && IsUnclearedNode(node))
            return true;
        var children = node.GetChildren();
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (ContainsPendingItem(child, ignoreNodeId))
                return true;
        }

        return false;
    }

    private static bool IsUnclearedNode(Node node)
    {
        GD.Print($"IsUnclearedNode for '{node.GetPath().GetConcatenatedNames()}'");

        if (node is ItemContainer || node is MapPickup || node is NotePickup)
        {
            GD.Print($"node '{node.GetPath().GetConcatenatedNames()}' was an ItemContainer, MapPickup, or NotePickup!");
            return true;
        }

        if (node is PassCode && !(node as PassCode).IsSolved())
        {
            GD.Print($"node '{node.GetPath().GetConcatenatedNames()}' was a puzzle and is unsolved!");
            return true;
        }

        return false;
    }
}

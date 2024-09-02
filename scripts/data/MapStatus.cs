using Godot;
using Godot.Collections;

public partial class MapStatus : Node
{
    [Export]
    private Node MapParent;

    private static MapStatus Instance;

    private Dictionary<int, MapData> AreaNameToMapDataLookup;
    private Dictionary<int, int> RoomNameToAreaNameLookup;

    private bool _initialized;

    public System.Collections.Generic.List<int> _mapsCollected;
    public System.Collections.Generic.List<int> _roomsVisited;
    public System.Collections.Generic.List<int> _roomsCleared;
    public System.Collections.Generic.List<int> _doorsFound;
    public System.Collections.Generic.List<int> _doorsEntered;
    public System.Collections.Generic.List<int> _lockedDoorsInspected;

    private const string MapFolder = "res://prefabs/maps/debug/";

    public override void _Ready()
    {
        if (this == Instance) return;

        if (Instance != null)
        {
            GD.PrintErr($"Found multiple instances of MapStatus class! Terminating '{Name}'!");
            QueueFree();
        }

        Instance = this;

        var mapScenes = DirAccess.GetFilesAt(MapFolder);

        _mapsCollected = new System.Collections.Generic.List<int>();
        _roomsVisited = new System.Collections.Generic.List<int>();
        _roomsCleared = new System.Collections.Generic.List<int>();
        _doorsFound = new System.Collections.Generic.List<int>();
        _doorsEntered = new System.Collections.Generic.List<int>();
        _lockedDoorsInspected = new System.Collections.Generic.List<int>();

        AreaNameToMapDataLookup = new Dictionary<int, MapData>();
        RoomNameToAreaNameLookup = new Dictionary<int, int>();

        for (var i = 0; i < mapScenes.Length; i++)
        {
            if (!mapScenes[i].EndsWith(".tscn")) continue;

            var mapScene = GD.Load<PackedScene>($"{MapFolder}{mapScenes[i]}");
            var mapSceneLoaded = mapScene.Instantiate();
            var mapData = mapSceneLoaded as MapData;

            var areaId = mapData.AreaId;
            GD.Print($"Adding area {areaId}...");
            AreaNameToMapDataLookup.Add(areaId, mapData);
            for (var j = 0; j < mapData.RoomData.Length; j++) {
                GD.Print($"Mapping room {mapData.RoomData[j].RoomId} to area {areaId}");
                RoomNameToAreaNameLookup.Add(mapData.RoomData[j].RoomId, areaId);
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
        return AreaNameToMapDataLookup;
    }

    public MapData GetMapDataForRoom(int roomId)
    {
        if (RoomNameToAreaNameLookup.ContainsKey(roomId))
            if (AreaNameToMapDataLookup.ContainsKey(RoomNameToAreaNameLookup[roomId]))
                return AreaNameToMapDataLookup[RoomNameToAreaNameLookup[roomId]];
            else
            {
                GD.PrintErr($"Found area id '{RoomNameToAreaNameLookup[roomId]}' for room '{roomId}' but didn't find that area in the area lookup!");
                return null;
            }
        else
        {
            GD.PrintErr($"Did not find area mapped to room '{roomId}'. Ensure this map has a room_info object with valid parameters!");
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

    public void ClearRoom(int roomId)
    {
        if(ClearedRoom(roomId)) return;
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
        foreach (var mapAreaId in AreaNameToMapDataLookup.Keys)
            AreaNameToMapDataLookup[mapAreaId].Reparent(MapParent);
    }
}

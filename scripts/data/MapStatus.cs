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

    // TODO: Add load data from file method!

    public static MapStatus GetInstance()
    {
        if (Instance == null)
        {
            GD.PrintErr("No MapStatus Instance found!");
            return null;
        }

        return Instance;
    }

    public bool IsInitialized()
    {
        return _initialized;
    }

    // TODO: Call this when saving data!
    public Dictionary<int, MapData> GetMapData()
    {
        GD.Print($"GetMapData() - type is {AreaNameToMapDataLookup?.GetType()?.ToString() ?? "Null"}");
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

    public void PickupMap(int areaId)
    {
        AreaNameToMapDataLookup[areaId].PlayerHasMap = true;
    }

    public void VisitRoom(int roomId)
    {
        AreaNameToMapDataLookup[RoomNameToAreaNameLookup[roomId]].GetRoom(roomId).PlayerVisitedRoom = true;
    }

    public void ClearRoom(int roomId)
    {
        AreaNameToMapDataLookup[RoomNameToAreaNameLookup[roomId]].GetRoom(roomId).PlayerClearedRoom = true;
    }

    public void ReturnMaps()
    {
        foreach (var mapAreaId in AreaNameToMapDataLookup.Keys)
            AreaNameToMapDataLookup[mapAreaId].Reparent(MapParent);
    }
}

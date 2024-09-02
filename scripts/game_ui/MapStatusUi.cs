using Godot;
using System.Collections.Generic;

public partial class MapStatusUi : StatusScreenTab
{
    private Dictionary<int, int> _mapIdToChildIndexLookup;
    private bool _initialized;

    public override void _Process(double delta)
    {
        Initialize();
        CheckForReturnToHeader();
    }

    private void Initialize()
    {
        if (_initialized) return;

        TreeExiting += _OnSceneExit;

        GD.Print("Initializing MapStatusUi...");

        var mapStatus = MapStatus.GetInstance();
        if(mapStatus == null || !mapStatus.IsInitialized()) return;

        var allMaps = mapStatus.GetMapData();

        foreach (var areaMapId in allMaps.Keys)
            allMaps[areaMapId].Reparent(this);

        _mapIdToChildIndexLookup = new Dictionary<int, int>();
        var numMaps = GetChildren().Count;
        for (var i = 0; i < numMaps; i++)
        {
            var areaMap = GetChild(i) as MapData;
            areaMap.Visible = false;
            areaMap.Position = Vector2.Zero;
            _mapIdToChildIndexLookup.Add(areaMap.AreaId, i);
            GD.Print($"Area {areaMap.AreaId} map: Position={areaMap.Position}  | global position={areaMap.GlobalPosition}");
        }

        _initialized = true;
    }

    public override void OnOpenMenu()
    {
        var childrenCount = GetChildren().Count;
        for (var i = 0; i < childrenCount; i++)
            ((Control)GetChild(i)).Visible = false;
        var mapStatus = MapStatus.GetInstance();

        GD.Print("Got MapStatus instance!");
        var roomId = GameConstants.GetCurrentRoomId(this);
        GD.Print($"Got current room id {roomId}");
        var mapData = mapStatus.GetMapDataForRoom(roomId);
        GD.Print($"Got mapData for room id {roomId} (area id {mapData.AreaId})");

        mapData.RefreshMap();
        ((Control)GetChild(_mapIdToChildIndexLookup[mapData.AreaId])).Visible = true;
    }

    private void _OnSceneExit()
    {
        MapStatus.GetInstance().ReturnMaps();
    }
}

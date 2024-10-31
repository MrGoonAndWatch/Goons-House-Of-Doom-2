using Godot;
using System.Collections.Generic;

public partial class MapStatusUi : StatusScreenTab
{
    private const float MapScrollSpeed = 400;

    private Dictionary<int, int> _mapIdToChildIndexLookup;
    private bool _initialized;
    private Control _currentMapNode;

    public override void _Process(double delta)
    {
        Initialize();
        CheckForMovement(delta);
        CheckForReturnToHeader();
    }

    private void Initialize()
    {
        if (_initialized) return;

        TreeExiting += _OnSceneExit;

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
        }

        _initialized = true;
    }

    private void CheckForMovement(double delta)
    {
        if (!IsActiveTab || _currentMapNode == null) return;

        var movement = GameConstants.GetMovementVectorWithDeadzone();

        if (movement.Equals(Vector2.Zero)) return;
        movement *= -1;

        var newPos = _currentMapNode.Position + (movement * MapScrollSpeed * (float)delta);
        _currentMapNode.SetPosition(newPos);
    }

    public override void OnOpenMenu()
    {
        var childrenCount = GetChildren().Count;
        for (var i = 0; i < childrenCount; i++)
            ((Control)GetChild(i)).Visible = false;
        var mapStatus = MapStatus.GetInstance();

        var roomId = GameConstants.GetCurrentRoomId(this);
        var mapData = mapStatus.GetMapDataForRoom(roomId);
        // TODO: Handle case where room is not on a map? Default map to open?
        if (mapData == null) return;

        mapData.SetCurrentRoom(roomId);
        mapData.RefreshMap();
        _currentMapNode = GetCurrentMapNode(mapData.AreaId);
        _currentMapNode.SetPosition(Vector2.Zero);
        _currentMapNode.Visible = true;
    }

    private void _OnSceneExit()
    {
        MapStatus.GetInstance().ReturnMaps();
    }

    private Control GetCurrentMapNode(int mapId)
    {
        return (Control)GetChild(_mapIdToChildIndexLookup[mapId]);
    }
}

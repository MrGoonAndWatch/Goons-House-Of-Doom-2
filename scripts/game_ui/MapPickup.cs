using Godot;

public partial class MapPickup : Area3D
{
    [Export]
    public MapPickupData MapPickupData;
    [Export]
    public Node3D LookAtTargetPoint;

    public override void _Ready()
    {
        var mapStatus = MapStatus.GetInstance();
        if (mapStatus == null) return;

        for (int i = 0; i < mapStatus._mapsCollected.Count; i++)
        {
            if (mapStatus._mapsCollected[i] == MapPickupData.AreaId)
            {
                GetParent().QueueFree();
                break;
            }
        }
    }
}

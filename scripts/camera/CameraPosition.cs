using Godot;

public partial class CameraPosition : Node3D
{
    [Export]
    private bool FollowPlayer;

    private bool _isActive;
    private Camera3D _camera;
    private PlayerStatus _playerStatus;

    public override void _Ready()
    {
        _playerStatus = PlayerStatus.GetInstance();
    }

    public void Activate(Camera3D camera)
    {
        _isActive = true;
        _camera = camera;
        
        camera.GlobalPosition = GlobalPosition;
        camera.GlobalRotation = GlobalRotation;
    }

    public void Deactivate()
    {
        _isActive = false;
        _camera = null;
    }
    
    public override void _Process(double delta)
    {
        if (!_isActive) return;
        
        if (FollowPlayer) RotateCameraTowardsPlayer();
    }

    private void RotateCameraTowardsPlayer()
    {
        _camera.LookAt(_playerStatus.GetPlayerPosition());
    }
}

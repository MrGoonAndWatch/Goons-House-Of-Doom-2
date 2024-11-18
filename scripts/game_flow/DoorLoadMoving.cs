using Godot;

public partial class DoorLoadMoving : Node3D
{
    [Export]
    private Vector3 MoveDirection = new Vector3(0, 0, -1);
    [Export]
    private float MoveSpeed = 1.0f;
    [Export]
    private float LoadingTimeSeconds = 5.0f;
    private const float _defaultSpeed = 1f;

    private Vector3 _moveDirection;
    private double _remainingTimeS;
    private bool _finished;

    public override void _Ready()
    {
        _moveDirection = MoveDirection.Normalized();
        _remainingTimeS = LoadingTimeSeconds;
    }

    public override void _Process(double delta)
    {
        if (_finished) return;

        var moveAmount = (float)(delta * MoveSpeed * _defaultSpeed);
        var movement = (_moveDirection) * new Vector3(moveAmount, moveAmount, moveAmount);
        Position = Position + movement;

        _remainingTimeS -= delta;
        if (_remainingTimeS <= 0 || Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
        {
            _finished = true;
            var sceneChanger = SceneChanger.GetInstance();
            sceneChanger.FinishSceneLoad();
        }
    }
}

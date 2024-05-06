using Godot;
using static GameConstants.Animation;

public abstract partial class ICutsceneActor: CharacterBody3D
{
    protected Vector3? _currentTargetPosition;
    private float _currentSpeed;
    private bool _rotatedToTargetPosition;

    public abstract void SetAnimationFlag(string flagName, Variant value);

    public void SetMoveToPosition(Vector3 targetPosition, float speed)
    {
        _rotatedToTargetPosition = false;
        // TODO: This will break if the destination's Y is different than whatever this actor's current Y pos is!
        _currentTargetPosition = new Vector3(targetPosition.X, Position.Y, targetPosition.Z);
        _currentSpeed = speed;
        var tween = CreateTween();

        var targetFull = targetPosition - Position;
        var target = new Vector2(targetFull.X, targetFull.Z).Normalized();
        var forward = new Vector2(Basis.Z.X, Basis.Z.Z);
        var targetAngle = Rotation.Y - (forward.AngleTo(target) + (Mathf.Pi / 2));

        tween.TweenProperty(this, "rotation:y", targetAngle, 0.25f);
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenCallback(Callable.From(EndRotateToTargetPosition));
    }

    public bool MoveTowardsPosition(double delta)
    {
        if (_currentTargetPosition.HasValue)
        {
            if (!_rotatedToTargetPosition)
                return false;

            var direction = (_currentTargetPosition.Value - Position).Normalized();
            Velocity = direction * _currentSpeed * (float) delta;
            var reachedPosition = Position.DistanceSquaredTo(_currentTargetPosition.Value) <= 0.01;
            
            if (reachedPosition)
            {
                _currentTargetPosition = null;
                Velocity = Vector3.Zero;
            }
            return reachedPosition;
        }
        else
            return true;
    }

    public void MoveToPositionInstantly(Vector3 position)
    {
        _currentTargetPosition = null;
        LookAt(position);
        // Note: idk why I have to rotate 90 degrees after doing LookAt but screw this engine tbh.
        Rotate(Vector3.Up, Mathf.Pi / 2);
        Position = position;
        Velocity = Vector3.Zero;
    }

    private void EndRotateToTargetPosition()
    {
        _rotatedToTargetPosition = true;
    }
}

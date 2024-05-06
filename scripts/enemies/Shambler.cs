using Godot;

public partial class Shambler : Enemy
{
    [Export]
    private float _chaseDistance = 6.0f;
    [Export]
    private float _attackRange = 3.0f;
    [Export]
    private float _timeToWaitAfterAttack = 2.0f;
    [Export]
    private RayCast3D _rayCast;

    private float _chaseDistanceSquared;
    private float _attackRangeSquared;

    private float _postAttackIdleTime;

    public override void _Ready()
    {
        base._Ready();
        _chaseDistanceSquared = _chaseDistance * _chaseDistance;
        _attackRangeSquared = _attackRange * _attackRange;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_postAttackIdleTime > 0)
            _postAttackIdleTime -= (float)delta;

        if (_isAttacking) {
            // TODO: Eventually won't need this.
            _attackTime -= (float)delta;
            if (_attackTime <= 0)
                OnAttackAnimationFinished();
            else
                return;
        }

        var distanceToPlayerSquared = GlobalPosition.DistanceSquaredTo(_player.GlobalPosition);
        if (distanceToPlayerSquared <= _chaseDistanceSquared)
            HandleChasePlayer(distanceToPlayerSquared, delta);
    }

    private void HandleChasePlayer(float distanceToPlayerSquared, double delta)
    {
        _rayCast.LookAt(_player.GlobalPosition);
        _rayCast.ForceRaycastUpdate();
        var lineOfSightCollider = _rayCast.GetCollider();
        if (lineOfSightCollider != null && !(lineOfSightCollider is Player)) return;

        if (distanceToPlayerSquared > _attackRangeSquared)
        {
            LookAtPoint(_player.GlobalPosition);
            MoveTowardsPosition(_player.GlobalPosition, _speed, delta);
        }
        else if (_postAttackIdleTime <= 0)
        {
            AttackPlayer();
            _postAttackIdleTime = _timeToWaitAfterAttack;
        }
    }
}

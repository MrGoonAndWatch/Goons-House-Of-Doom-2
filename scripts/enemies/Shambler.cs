using Godot;

public partial class Shambler : Enemy
{
    [Export]
    private float _chaseDistance = 6.0f;
    [Export]
    private float _attackRange = 3.0f;
    [Export]
    private float _timeToWaitAfterAttack = 2.0f;

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
        {
            if (distanceToPlayerSquared > _attackRangeSquared)
            {
                LookAtPlayer();
                MoveTowardsPlayer(delta);
            }
            else if (_postAttackIdleTime <= 0)
            {
                AttackPlayer();
                _postAttackIdleTime = _timeToWaitAfterAttack;
            }
        }
    }
}

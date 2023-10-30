using Godot;

public partial class Chaser : Enemy
{
	[Export]
	private float _chaseSpeed = 6.0f;
	[Export]
	private float _maxWanderDistance = 5.0f;
	[Export]
	private float _minWanderDistance = 1.0f;

	private Vector3 _wanderPoint;
	private Vector3 _chasePoint;
	private bool _chasing;

	private RandomNumberGenerator _rng;

	public override void _Ready()
	{
		base._Ready();
		_rng = new RandomNumberGenerator();
		_chasePoint = Vector3.Zero;
		PickNewWanderPoint();
	}

    public override void _PhysicsProcess(double delta)
    {
		var movement = Input.GetAxis(GameConstants.Controls.Down, GameConstants.Controls.Up);
		if(Input.IsActionPressed(GameConstants.Controls.Run) && movement > 0)
		{
			_chasePoint = _player.GlobalPosition;
			_chasing = true;
		}

        if (_isAttacking)
        {
            // TODO: Eventually won't need this.
            _attackTime -= (float)delta;
            if (_attackTime <= 0)
                OnAttackAnimationFinished();
            else
                return;
        }

        if (_chasing)
		{
			MoveTowardsPosition(_chasePoint, _chaseSpeed, delta);
			LookAtPoint(_chasePoint);
			if(IsAtPoint())
			{
				AttackPlayer();
				_chasing = false;
				PickNewWanderPoint();
			}
		}
		else
		{
            // TODO: Pause after reaching wander point?
            LookAtPoint(_wanderPoint);
            MoveTowardsPosition(_wanderPoint, _speed, delta);
			if (IsAtPoint())
				PickNewWanderPoint();
		}
    }

	private bool IsAtPoint()
	{
        var dist = GlobalPosition.DistanceSquaredTo(_navigation.GetFinalPosition());
        return dist < 2;
    }

    private void PickNewWanderPoint()
	{
		var distanceX = 2*(_rng.Randf() - 0.5f) * (_maxWanderDistance - _minWanderDistance);
        var distanceZ = 2*(_rng.Randf() - 0.5f) * (_maxWanderDistance - _minWanderDistance);
		if (distanceX < 0)
			distanceX -= _minWanderDistance;
		else
			distanceX += _minWanderDistance;
        if (distanceZ < 0)
            distanceZ -= _minWanderDistance;
        else
            distanceZ += _minWanderDistance;

        _wanderPoint = new Vector3(distanceX, 0, distanceZ);
	}

    public override void OnBodyEntered(Node3D node)
    {
        if(node is Player && !_isAttacking)
			AttackPlayer();
		base.OnBodyEntered(node);
    }
}

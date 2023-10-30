using Godot;

public abstract partial class Enemy : CharacterBody3D
{
    [Export]
    private NavigationAgent3D _navigation;

    [Export]
    private float _speed = 2.0f;
    [Export]
    private float _acceleration = 10.0f;
    [Export]
    private float _damage = 20.0f;

    protected bool _isAttacking;
    private bool _hitPlayerThisAttack;
    private bool _touchingPlayer;
    // TODO: Eventually won't need either of these vars, handle through animation events/signals.
    protected float _attackTime;
    protected const float AttackDuration = 2.0f;

    protected Player _player;

    public override void _Ready()
	{
        _player = GetNode<Player>(GameConstants.NodePaths.FromSceneRoot.Player);
    }

    protected void LookAtPlayer()
    {
        LookAt(new Vector3(_player.GlobalPosition.X, GlobalPosition.Y, _player.GlobalPosition.Z), Vector3.Up);
        RotateY(Mathf.Pi);
    }

    protected void MoveTowardsPlayer(double delta)
    {
        _navigation.TargetPosition = _player.GlobalPosition;
        var direction = (_navigation.GetNextPathPosition() - GlobalPosition).Normalized();
        Velocity = Velocity.Lerp(direction * _speed, _acceleration * (float)delta);
        MoveAndSlide();
    }

    protected void AttackPlayer()
    {
        // TODO: Implement...
        _attackTime = AttackDuration;
        _isAttacking = true;
        GD.Print("Start attacking!");

        if (_touchingPlayer)
            HitPlayer();
    }

    protected virtual void HitPlayer()
    {
        if (_hitPlayerThisAttack) return;

        // TODO: Set animation here!
        PlayerStatus.GetInstance().HitByAttack(_damage, "");
        _hitPlayerThisAttack = true;
    }

    public void OnAttackAnimationFinished()
    {
        _hitPlayerThisAttack = false;
        _isAttacking = false;
        GD.Print("End attack!");
    }

    public void OnBodyEntered(Node3D node)
    {
        if (node is Player)
        {
            _touchingPlayer = true;
            if (_isAttacking)
            {
                HitPlayer();
            }
        }
    }

    public void OnBodyExited(Node3D node)
    {
        if (node is Player)
        {
            _touchingPlayer = false;
        }
    }
}

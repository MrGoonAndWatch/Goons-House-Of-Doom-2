using Godot;

public abstract partial class Enemy : CharacterBody3D
{
    public int EnemyId;
    [Export]
    protected NavigationAgent3D Navigation;

    [Export]
    protected float _speed = 2.0f;
    [Export]
    private float _acceleration = 10.0f;
    [Export]
    private float _damage = 20.0f;
    [Export]
    public float _maxHp = 1.0f;
    private float _currentHp;

    protected bool _isAttacking;
    private bool _hitPlayerThisAttack;
    private bool _touchingPlayer;
    // TODO: Eventually won't need either of these vars, handle through animation events/signals.
    protected float _attackTime;
    protected const float AttackDuration = 2.0f;

    protected Player _player;

    public override void _Ready()
	{
        _currentHp = _maxHp;
        _player = GetNode<Player>(GameConstants.NodePaths.FromSceneRoot.Player);
    }

    protected virtual bool CanMove()
    {
        var playerStatus = PlayerStatus.GetInstance();
        return !playerStatus.HasAnyUiOpen();
    }

    protected void LookAtPoint(Vector3 lookAt)
    {
        LookAt(new Vector3(lookAt.X, GlobalPosition.Y, lookAt.Z), Vector3.Up);
        RotateY(Mathf.Pi);
    }

    protected void MoveTowardsPosition(Vector3 position, float speed, double delta)
    {
        Navigation.TargetPosition = position;
        var direction = (Navigation.GetNextPathPosition() - GlobalPosition).Normalized();
        Velocity = Velocity.Lerp(direction * speed, _acceleration * (float)delta);
        MoveAndSlide();
    }

    protected void AttackPlayer()
    {
        // TODO: Implement...
        _attackTime = AttackDuration;
        _isAttacking = true;

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
    }

    public virtual void OnBodyEntered(Node3D node)
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

    public virtual void OnBodyExited(Node3D node)
    {
        if (node is Player)
        {
            _touchingPlayer = false;
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHp -= damage;
        if (_currentHp < 0)
            ForceDead();
    }

    // TODO: Improve this! Also move other stuff in from DamageHandler.
    public void ForceDead()
    {
        PlayerStatus.GetInstance().KillEnemy(EnemyId);
        QueueFree();
    }
}

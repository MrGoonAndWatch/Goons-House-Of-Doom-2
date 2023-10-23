using Godot;
using System;
using System.Collections.Generic;
using static GameConstants;

public partial class PlayerStatus : Node
{
    public Weapon EquipedWeapon;
    //public Animator PlayerAnimator;
    // TODO: Need to hook this up with non-export since this is an autoloaded component.
    [Export]
    private Node3D GameOverUi;

    public double Health;
    public const double MaxHealth = 200.22;

    public bool MenuOpened;
    public bool Reading;
    public bool LockMovement;
    public bool QuickTurning;
    public bool TakingDamage;
    public bool Aiming;
    public bool Shooting;
    public bool HasSaveUiOpen;
    public bool Paused;

    public List<int> KilledEnemies;
    public List<GlobalEvent> TriggeredEvents;
    public List<int> GrabbedItems;
    public List<int> DoorsUnlocked;

    [Export(hintString: "Time (in seconds) between when player hp reaches 0 and when the game over screen comes up.")]
    public float GameOverUiDelay = 6.0f;
    private double _timeUntilShowGameOverUi;
    private bool _showingGameOverUi;

    [Export(hintString: "Time (in seconds) from when a player gets hit to when they can get hit again.")]
    public float HitCooldown = 1.0f;
    private double _remainingHitCooldown;

    private static PlayerStatus _instance;

    public override void _Ready()
    {
        GD.Print("PlayerStatus _Ready called!");

        // TODO: May not need this, can just rely on Godot singleton logic?
        if (_instance != null && _instance != this)
        {
            QueueFree();
            return;
        }
        _instance = this;

        KilledEnemies = new List<int>();
        TriggeredEvents = new List<GlobalEvent>();
        GrabbedItems = new List<int>();
        DoorsUnlocked = new List<int>();

        Health = MaxHealth;
    }

    public override void _Process(double delta)
    {
        ProcessGameOverUi(delta);
        ProcessExitInput();
        ProcessHitCooldown(delta);
    }

    public static PlayerStatus GetInstance() { return _instance; }

    private void ProcessGameOverUi(double delta)
    {
        if (Health > 0 || _timeUntilShowGameOverUi <= 0)
            return;

        _timeUntilShowGameOverUi -= delta;

        if (_timeUntilShowGameOverUi <= 0)
        {
            EnableGameOverUi();
        }
    }

    public void ForceGameOverUi()
    {
        EnableGameOverUi();
    }

    private void EnableGameOverUi()
    {
        var hordeModeManager = GetNode<HordeModeManager>(NodePaths.FromSceneRoot.HordeModeManager);
        if (hordeModeManager == null)
        {
            GameOverUi.Visible = true;
            _showingGameOverUi = true;
        }
        else
            hordeModeManager.OnGameEnd();
    }

    private void ProcessExitInput()
    {
        if (!_showingGameOverUi)
            return;

        if (Input.IsActionJustPressed(Controls.Pause))
            GetTree().Quit();
    }

    private void ProcessHitCooldown(double delta)
    {
        if (_remainingHitCooldown <= 0)
            return;

        _remainingHitCooldown -= delta;
        if (_remainingHitCooldown <= 0)
            TakingDamage = false;
    }

    public void KillEnemy(int enemyId)
    {
        KilledEnemies.Add(enemyId);
    }

    public void UnlockDoor(int doorId)
    {
        DoorsUnlocked.Add(doorId);
    }

    public void GrabItem(int itemId)
    {
        GrabbedItems.Add(itemId);
    }

    public void TriggeredEvent(GlobalEvent eventTriggered)
    {
        TriggeredEvents.Add(eventTriggered);
    }

    public void HitByAttack(double damage, string hitAnimationVariable)
    {
        if (GetHealthStatus() == HealthStatus.Dead)
            return;

        //SoundManager.PlayHitSfx();
        TakingDamage = true;
        AddHealth(-damage);
        //PlayerAnimator.SetBool(hitAnimationVariable, true);
        // TODO: Instead of a hard coded cooldown should have event handling from the animator to check when hittable again.
        _remainingHitCooldown = HitCooldown;
    }

    public void AddHealth(double value)
    {
        Health = Math.Max(0, Math.Min(MaxHealth, Health + value));
        HandleDeath();
    }

    public void SetHealth(double value)
    {
        Health = value;
        HandleDeath();
    }

    private void HandleDeath()
    {
        if (Health > 0)
            return;

        //SoundManager.PauseSong();

        _timeUntilShowGameOverUi = GameOverUiDelay;

        if (MenuOpened)
        {
            // Note: Getting node here instead of _Ready() or something because this is a singleton and we need to recalc this every scene!
            var inv = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
            inv.ToggleMenu();
        }

        if (Reading)
        {
            // Note: Getting node here instead of _Ready() or something because this is a singleton and we need to recalc this every scene!
            var textReader = GetNode<InspectTextUi>(NodePaths.FromSceneRoot.InspectTextUi);
            textReader.ForceCloseTextbox();
        }

        //SoundManager.PlayDeathSfx();
        //PlayerAnimator.SetBool(AnimationVariables.Player.Dead, true);
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (EquipedWeapon == weapon)
            EquipedWeapon = null;
        else
            EquipedWeapon = weapon;
    
        //var layerIndex = PlayerAnimator.GetLayerIndex(AnimationLayers.Player.EquipLayer);
        var weight = EquipedWeapon == null ? 0 : 1;
        //PlayerAnimator.SetLayerWeight(layerIndex, weight);
    }

    public HealthStatus GetHealthStatus()
    {
        if (Health == 0)
            return HealthStatus.Dead;
        if (Health <= 1)
            return HealthStatus.Special;
        if (Health <= 40)
            return HealthStatus.SpeedyBoi;
        if (Health <= 80)
            return HealthStatus.BadTummyAche;
        if (Health <= 120)
            return HealthStatus.TummyAche;
        return HealthStatus.Healthy;
    }

    public bool CanPause()
    {
        return !LockMovement && Health > 0;
    }

    public bool CanOpenMenu()
    {
        return !Paused && !Reading && Health > 0 && !TakingDamage && !HasSaveUiOpen && !LockMovement;
    }

    public bool IsMovementPrevented()
    {
        return Paused || MenuOpened || LockMovement || TakingDamage || Shooting || Reading || HasSaveUiOpen || Health <= 0;
    }

    public bool CanInteract()
    {
        return !Paused && !MenuOpened && !Reading && !TakingDamage && !Shooting && Health > 0;
    }

    public bool CanShoot()
    {
        return !Paused && !MenuOpened && !Reading && !TakingDamage && Health > 0 && !HasSaveUiOpen && !Reading && !LockMovement;
    }
}

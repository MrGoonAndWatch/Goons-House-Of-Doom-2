using Godot;
using System;
using System.Collections.Generic;
using static GameConstants;

public partial class PlayerStatus : Node
{
    public GameSettings GameSettings;

    public Weapon EquipedWeapon;
    //public Animator PlayerAnimator;
    // TODO: Need to hook this up with non-export since this is an autoloaded component.
    [Export]
    private Node3D GameOverUi;

    public double Health;
    public const double MaxHealth = 100;

    public bool MenuOpened;
    public bool Reading;
    public bool QuickTurning;
    public bool TakingDamage;
    public bool Aiming;
    public bool Shooting;
    public bool HasSaveLoadUiOpen;
    public bool Paused;
    public bool ItemBoxOpened;
    public bool ReadyToShoot;
    public bool IsInCutscene;

    public List<int> DeadEnemies;
    public List<GlobalEvent> TriggeredEvents;
    public List<int> GrabbedItems;
    public List<int> DoorsUnlocked;
    public List<int> CutscenesWatched;
    public List<NoteData> NotesCollected;

    [Export(hintString: "Time (in seconds) between when player hp reaches 0 and when the game over screen comes up.")]
    public float GameOverUiDelay = 6.0f;
    private double _timeUntilShowGameOverUi;
    private bool _showingGameOverUi;

    [Export(hintString: "Time (in seconds) from when a player gets hit to when they can get hit again.")]
    public float HitCooldown = 1.0f;
    private double _remainingHitCooldown;

    private Vector3 _playerPosition;
    private float _playerAngle;

    private static PlayerStatus _instance;

    public override void _Ready()
    {
        // Note: this gets overwritten by SetupNewGame and LoadGame calls during real gameplay, this is here so you testing from individual rooms will work.
        GameSettings = new GameSettings
        {
            GameDifficulty = GameDifficulty.Normal,
            FunnyMode = false,
        };

        GD.Print("PlayerStatus _Ready called!");

        // TODO: May not need this, can just rely on Godot singleton logic?
        if (_instance != null && _instance != this)
        {
            QueueFree();
            return;
        }
        _instance = this;

        ResetGame();
    }

    public void ResetGame()
    {
        DeadEnemies = new List<int>();
        TriggeredEvents = new List<GlobalEvent>();
        GrabbedItems = new List<int>();
        DoorsUnlocked = new List<int>();
        CutscenesWatched = new List<int>();
        NotesCollected = new List<NoteData>();

        Health = MaxHealth;

        MenuOpened = false;
        Reading = false;
        QuickTurning = false;
        TakingDamage = false;
        Aiming = false;
        Shooting = false;
        HasSaveLoadUiOpen = false;
        Paused = false;
        ItemBoxOpened = false;
        ReadyToShoot = false;
        IsInCutscene = false;

        DataSaver.ResetState();
        MapStatus.GetInstance()?.InitializeMapStatus();
    }

    public override void _Process(double delta)
    {
        // if (Input.IsActionJustPressed("DEBUG_Save"))
        // {
        //     GameDifficulty = (GameDifficulty)((((int)GetInstance().GameDifficulty) + 1) % Enum.GetValues(typeof(GameDifficulty)).Length);
        // }
        // if (Input.IsActionJustPressed("DEBUG_Load"))
        // {
        //     RandomizerEnabled = !RandomizerEnabled;
        //     var settings = new RandomizerSettings
        //     {
        //         AllowSpawnsOnEmptyEnemySlotsForDifficulty = true,
        //         AllowSpawnsOnEmptyItemSlotsForDifficulty = true,
        //         RandomizeEnemies = true,
        //         RandomizeItems = true,
        //     };
        //     if(RandomizerEnabled)
        //         RandomizerSeed = RandomizerSeed.GenerateRandomizer(settings);
        // }
        ProcessGameOverUi(delta);
        ProcessExitInput();
        ProcessHitCooldown(delta);
    }

    public static PlayerStatus GetInstance() { return _instance; }

    public void SetupNewGame(GameSettings settings)
    {
        GameSettings = settings;
        ResetGame();
    }

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

        if (Input.IsActionJustPressed(Controls.pause.ToString()))
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
        DeadEnemies.Add(enemyId);
    }

    public void UnlockDoor(int doorId)
    {
        GD.Print($"Unlocked door {doorId}");
        DoorsUnlocked.Add(doorId);
    }

    public void SetWatchedCutscene(int cutsceneId)
    {
        CutscenesWatched.Add(cutsceneId);
    }

    public bool HasWatchedCutscene(int cutsceneId)
    {
        return CutscenesWatched.Contains(cutsceneId);
    }

    public void GrabItem(int itemId)
    {
        GrabbedItems.Add(itemId);
    }

    public void TriggeredEvent(GlobalEvent eventTriggered)
    {
        // TODO: Apply this to anything that a triggered event can effect!!!
        var doors = ProcessGameState.FindObjectsOfType<Door>(GetTree().Root);
        foreach (var door in doors) {
            door.OnEvent(eventTriggered);
        }

        TriggeredEvents.Add(eventTriggered);
    }

    public void HitByAttack(double damage, string hitAnimationVariable)
    {
        //GD.Print($"Player was hit for {damage} damage and started animation '{hitAnimationVariable}'");

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

        GD.Print("Player died!");
        //SoundManager.PauseSong();

        _timeUntilShowGameOverUi = GameOverUiDelay;

        if (MenuOpened)
        {
            // TODO: Probably won't need this once we make the game pause while inventory is open!
            // Note: Getting node here instead of _Ready() or something because this is a singleton and we need to recalc this every scene!
            var inv = GetNode<StatusScreenHeader>(NodePaths.FromSceneRoot.PlayerStatusScreenHeader);
            inv.ToggleMenu();
        }

        if (ItemBoxOpened)
        {
            var itemBox = GetNode<PlayerItemBoxControl>(NodePaths.FromSceneRoot.ItemBoxControl);
            itemBox.OpenMenu();
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

    public void EquipWeapon(Weapon weapon, bool forceEquip = false)
    {
        var player = GetNode<Player>(NodePaths.FromSceneRoot.Player);

        if (!forceEquip && EquipedWeapon != null && EquipedWeapon.ItemId == weapon.ItemId)
        {
            player.WeaponUnequipped(weapon);
            EquipedWeapon = null;
        }
        else
        {
            player.WeaponEquipped(weapon);
            EquipedWeapon = weapon;
        }
    
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
        if (Health <= 20)
            return HealthStatus.SpeedyBoi;
        if (Health <= 40)
            return HealthStatus.BadTummyAche;
        if (Health <= 80)
            return HealthStatus.TummyAche;
        return HealthStatus.Healthy;
    }

    public bool CanPause()
    {
        return !HasAnyUiOpen() && !IsInCutscene && Health > 0;
    }

    public bool CanOpenMenu()
    {
        return !IsInCutscene && !Paused && !ItemBoxOpened && !Reading && Health > 0 && !TakingDamage && !HasSaveLoadUiOpen;
    }

    public bool HasAnyUiOpen()
    {
        return Paused || MenuOpened || Reading || ItemBoxOpened || HasSaveLoadUiOpen;
    }

    public bool IsRotationPrevented()
    {
        return IsInCutscene || TakingDamage || Shooting || Health <= 0 || HasAnyUiOpen();
    }

    public bool IsMovementPrevented()
    {
        return IsInCutscene || TakingDamage || Aiming || Shooting || Health <= 0 || HasAnyUiOpen();
    }

    public bool CanInteract()
    {
        return !IsInCutscene && !TakingDamage && !Aiming && !Shooting && Health > 0 && !HasAnyUiOpen();
    }

    public bool CanAim()
    {
        return !IsInCutscene && !Aiming && !Paused && !TakingDamage && Health > 0 && !HasAnyUiOpen();
    }

    public bool CanShoot()
    {
        return ReadyToShoot && !IsInCutscene && !TakingDamage && Health > 0 && EquipedWeapon != null && !HasAnyUiOpen();
    }

    public bool HasAmmoInInventory(PlayerInventory playerInventory)
    {
        return GetFirstCompatibleAmmoSlot(EquipedWeapon, playerInventory) != null;
    }

    public void AddAmmoToCurrentWeaponFromInventory(PlayerInventory playerInventory)
    {
        var ammoSlot = GetFirstCompatibleAmmoSlot(EquipedWeapon, playerInventory);
        if (ammoSlot == null) return;

        foreach(var item in playerInventory.Items)
            if(item?.Item != null && item.Item.ItemId == EquipedWeapon.ItemId)
                item.Combine(ammoSlot);
        playerInventory.SetAllDirty();
    }

    private static ItemSlot GetFirstCompatibleAmmoSlot(Weapon weapon, PlayerInventory inv)
    {
        if (weapon == null) return null;

        var ammoType = weapon.GetAmmoType();
        foreach (var item in inv.Items)
        {
            if (item?.Item != null && item.Item.GetType() == ammoType)
                return item;
        }

        return null;
    }

    public static void CollectNote(NoteData noteData)
    {
        if (_instance == null) return;

        _instance.NotesCollected.Add(noteData.Clone());
    }

    public void UseKey(Key key)
    {
        var playerInteract = GetNode<PlayerInteract>(NodePaths.FromSceneRoot.PlayerInteract);
        playerInteract.UseKey(key);
    }

    public void UpdatePlayerPosition(Vector3 position, float angle)
    {
        _playerPosition = position;
        _playerAngle = angle;
    }

    public Vector3 GetPlayerPosition()
    {
        return _playerPosition;
    }

    public float GetPlayerAngle()
    {
        return _playerAngle;
    }

    public static void DebugPrintPlayerStatus()
    {
        GD.Print(DebugGetStatusFlagsString());
    }

    public static string DebugGetStatusFlagsString()
    {
        if (_instance == null)
            return "";

        return $"MenuOpened={_instance.MenuOpened}\r\n" +
            $"Reading={_instance.Reading}\r\n" +
            $"QuickTurning={_instance.QuickTurning}\r\n" +
            $"TakingDamage={_instance.TakingDamage}\r\n" +
            $"Aiming={_instance.Aiming}\r\n" +
            $"Shooting={_instance.Shooting}\r\n" +
            $"HasSaveLoadUiOpen={_instance.HasSaveLoadUiOpen}\r\n" +
            $"Paused={_instance.Paused}\r\n" +
            $"ItemBoxOpened={_instance.ItemBoxOpened}\r\n" +
            $"ReadyToShoot={_instance.ReadyToShoot}\r\n" +
            $"IsInCutscene={_instance.IsInCutscene}\r\n" +
            $"IsRotationPrevented={_instance.IsRotationPrevented()}\r\n" +
            $"IsMovementPrevented={_instance.IsMovementPrevented()}";
    }
}

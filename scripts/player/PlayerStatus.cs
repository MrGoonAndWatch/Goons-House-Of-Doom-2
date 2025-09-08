using Godot;
using System;
using System.Collections.Generic;
using static GameConstants;

public partial class PlayerStatus : Node
{
    public GameSettings GameSettings;
    public Weapon EquipedWeapon;
    
    private PlayerAnimationControl _playerAnimationControl;
    private bool _initialized;

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
    private bool _isInCutscene;
    private bool _isPickingUpItem;

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

    private Vector3? _storedCameraPosition;
    private Vector3? _storedCameraRotation;

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
        _isInCutscene = false;
        EquipedWeapon = null;
        _storedCameraPosition = null;
        _storedCameraRotation = null;
        _isPickingUpItem = false;

        DataSaver.ResetState();
        MapStatus.GetInstance()?.InitializeMapStatus();
    }

    public override void _Process(double delta)
    {
        if (!_initialized) Initialize();
        ProcessExitInput();
        ProcessHitCooldown(delta);
    }

    private void Initialize()
    {
        _playerAnimationControl = PlayerAnimationControl.GetInstance();
        _initialized = true;
    }

    public static PlayerStatus GetInstance() { return _instance; }

    public void SetIsInCutscene(bool isInCutscene, bool revertCamera = false)
    {
        _isInCutscene = isInCutscene;
        if (!_isInCutscene && revertCamera)
            ResetCamera();
    }

    public void SetIsPickingUpItem(bool isPickingUpItem)
    {
        _isPickingUpItem = isPickingUpItem;
    }

    private void ResetCamera()
    {
        // TODO: Would really prefer to not rely on the camera being named a specific thing, and also using GetNode, but w/e.
        var camera = GetNode<Camera3D>(GameConstants.NodePaths.FromSceneRoot.Camera);
        if (_storedCameraPosition.HasValue)
            camera.GlobalPosition = _storedCameraPosition.Value;
        if (_storedCameraRotation.HasValue)
            camera.GlobalRotation = _storedCameraRotation.Value;
        _storedCameraPosition = null;
        _storedCameraRotation = null;
        //GD.Print($"ResetCamera called (camera.GlobalPosition={camera.GlobalPosition} from {_storedCameraRotation}, camera.GlobalRotation={camera.GlobalRotation} from {_storedCameraRotation})");
    }

    public void SetupNewGame(GameSettings settings)
    {
        GameSettings = settings;
        ResetGame();
    }

    private void ProcessExitInput()
    {
        if (!_showingGameOverUi)
            return;

        if (Input.IsActionJustPressed(Controls.pause.ToString()) && !DebugManager.IsDebugConsoleActive())
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
        var doors = FindObjectsOfType<Door>(GetTree().Root);
        foreach (var door in doors) {
            door.OnEvent(eventTriggered);
        }

        TriggeredEvents.Add(eventTriggered);
    }

    public void HitByAttack(double damage, string hitAnimationVariable)
    {
        //GD.Print($"Player was hit for {damage} damage and started animation '{hitAnimationVariable}'");

        if (Health <= 0)
            return;

        //SoundManager.PlayHitSfx();
        TakingDamage = true;
        AddHealth(-damage);
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

        CloseAllOpenUi();

        GhodAudioManager.PlayDeathSfx();
        var player = GetNode<Player>(NodePaths.FromSceneRoot.Player);
        player.StartDeath();
    }

    private void CloseAllOpenUi()
    {
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
    }

    public void EquipWeapon(Weapon weapon, bool forceEquip = false)
    {
        if (!forceEquip && EquipedWeapon != null && EquipedWeapon.ItemId == weapon.ItemId)
        {
            _playerAnimationControl.UnequipWeapon(weapon);
            EquipedWeapon = null;
        }
        else
        {
            _playerAnimationControl.EquipWeapon(weapon);
            EquipedWeapon = weapon;
        }
    }

    public bool CanPause()
    {
        return !HasAnyUiOpen() && !_isInCutscene && Health > 0;
    }

    public bool CanOpenMenu()
    {
        return !_isInCutscene && !Paused && !ItemBoxOpened && !Reading && Health > 0 && !TakingDamage && !HasSaveLoadUiOpen && !_isPickingUpItem && !DebugManager.IsDebugConsoleActive();
    }

    public bool HasAnyUiOpen()
    {
        return Paused || MenuOpened || Reading || ItemBoxOpened || HasSaveLoadUiOpen || _isPickingUpItem || DebugManager.IsDebugConsoleActive();
    }

    public bool IsRotationPrevented()
    {
        return _isInCutscene || TakingDamage || Shooting || Health <= 0 || HasAnyUiOpen();
    }

    public bool IsMovementPrevented()
    {
        return _isInCutscene || TakingDamage || Aiming || Shooting || Health <= 0 || HasAnyUiOpen();
    }

    public bool CanInteract()
    {
        return !_isInCutscene && !TakingDamage && !Aiming && !Shooting && Health > 0 && !HasAnyUiOpen();
    }

    public bool CanAim()
    {
        return !_isInCutscene && !Aiming && !Paused && !TakingDamage && Health > 0 && !HasAnyUiOpen();
    }

    public bool CanShoot()
    {
        return ReadyToShoot && !_isInCutscene && !TakingDamage && Health > 0 && EquipedWeapon != null && !HasAnyUiOpen();
    }

    public static bool CanChangeCameraAngle()
    {
        if (_instance == null) return true;
        
        return !_instance._isInCutscene;
    }

    public static void StoreCameraPositioning(Vector3 cameraPosition, Vector3 cameraRotation)
    {
        if (_instance == null) return;
        
        _instance._storedCameraPosition = cameraPosition;
        _instance._storedCameraRotation = cameraRotation;
        
        //GD.Print($"StoreCameraAngle called (_instance._storedCameraPosition={_instance._storedCameraPosition} , _instance._storedCameraRotation={_instance._storedCameraRotation})");
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
            $"IsInCutscene={_instance._isInCutscene}\r\n" +
            $"IsRotationPrevented={_instance.IsRotationPrevented()}\r\n" +
            $"IsMovementPrevented={_instance.IsMovementPrevented()}";
    }
}

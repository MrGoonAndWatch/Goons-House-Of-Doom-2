using Godot;
using Godot.Collections;

public partial class GameConstants : GodotObject
{
    public const float GreenMedicineHp = 25.0f;
    public const float ControllerMenuDeadzone = 0.72f;

    public const string SaveDirectoryPath = "user://saves";
    public const string ScenesDirectory = "res://scenes/";
    public const string GlobalSettingsFilename = "global.config";

    public static class Colors
    {
        public static Color Clear = Color.Color8(0, 0, 0, 0);
        public static Color White = Color.Color8(255, 255, 255);
    }

    public enum GameDifficulty
    {
        Easy,
        Normal,
        Hard,
        Impossible
    }

    public enum HealthStatus
    {
        None = 0,
        Dead,
        Special,
        SpeedyBoi,
        BadTummyAche,
        TummyAche,
        Healthy
    }

    public enum MenuActionType
    {
        Use,
        Combine,
        Examine,
        Discard,
    }

    public enum KeyType
    {
        None = 0,
        BigKey = 1,
    }

    public enum DoorLoadType
    {
        None = 0,
        WoodDoor1 = 1,
    }

    public enum GlobalEvent
    {
        None = 0,
        TestKeypadUnlocked = 1,
    }

    // Note: These are powers of 2 to allow the randomizer settings to use a singular field to store which of these are enabled.
    public enum EnemySpawnType
    {
        None = 1,
        Shambler = 2,
        Chaser = 4
    }

    // Note: These are powers of 2 to allow the randomizer settings to use a singular field to store which of these are enabled.
    public enum ItemSpawnType
    {
        None = 1,
        GreenJuice = 2,
        Pistol = 3,
        PistolAmmo = 4,
        BigKey = 5,
    }

    private const string EnemyPrefabFolder = "res://prefabs/spawnables/enemies/";

    public static Dictionary<EnemySpawnType, string> EnemyPrefabLookup = new Dictionary<EnemySpawnType, string>()
    {
        {EnemySpawnType.Shambler, $"{EnemyPrefabFolder}/shambler.tscn"},
        {EnemySpawnType.Chaser, $"{EnemyPrefabFolder}/chaser.tscn"}
    };

    private const string ItemPrefabFolderPath = "res://prefabs/spawnables/items/";
    public const string GarbagePrefabPath = $"{ItemPrefabFolderPath}/garbage.tscn";

    public const string SaveFileButtonUi = "res://prefabs/ui/save_menu_file_ui.tscn";
    public const string SaveFileNewSaveText = "(create new save)";

    public static Dictionary<ItemSpawnType, string> ItemPrefabLookup = new Dictionary<ItemSpawnType, string>()
    {
        {ItemSpawnType.GreenJuice, $"{ItemPrefabFolderPath}/green-juice.tscn"},
        {ItemSpawnType.Pistol, $"{ItemPrefabFolderPath}/pistol.tscn"},
        {ItemSpawnType.PistolAmmo, $"{ItemPrefabFolderPath}/pistol-ammo.tscn"},
        {ItemSpawnType.BigKey, $"{ItemPrefabFolderPath}/big-key.tscn" },
    };

    public enum Controls
    {
        left,
        right,
        up,
        down,
        run,
        inventory,
        confirm,
        aim,
        pause
    }

    public enum ControlBinding
    {
        Primary,
        Secondary,
        Tertiary
    }

    public enum PassCodeType
    {
        Invalid = 0,
        TestCode = 1,
    }

    public static Dictionary<GameDifficulty, Dictionary<PassCodeType, string>> PassCodeLookup = new()
    {
        { GameDifficulty.Easy, new Dictionary<PassCodeType, string> {
            {PassCodeType.TestCode, "1234" }
        } },
        { GameDifficulty.Normal, new Dictionary<PassCodeType, string> {
            {PassCodeType.TestCode, "4321" }
        } },
        { GameDifficulty.Hard, new Dictionary<PassCodeType, string> {
            {PassCodeType.TestCode, "1111" }
        } },
        { GameDifficulty.Impossible, new Dictionary<PassCodeType, string> {
            {PassCodeType.TestCode, "9999" }
        } },
    };

    public const int ItemBoxSize = 50;

    public static class UiLabels
    {
        public const string EmptyItemBoxSlotText = "(empty)";
    }

    public static class NodePaths
    {
        public static class FromSceneRoot
        {
            public const string Player = "/root/root/Player";
            public const string PlayerInventory = Player + "/PlayerInventory";
            public const string ItemBoxControl = Player + "/PlayerItemBoxControl";
            public const string PlayerInteract = Player + "/InteractHitbox";
            public const string InspectTextUi = Player + "/InspectTextUI";
            public const string PlayerStatus = "/root/root/PlayerStatus";
            public const string HordeModeManager = "/root/root/HordeModeManager";
            public const string SaveGameUi = Player + "/save_game_ui";
        }
    }

    public static class Animation
    {
        public static class Player
        {
            public const string IdleLegs = "parameters/Legs/conditions/idle";
            public const string IdleHands = "parameters/Arms/conditions/idle";
            public const string Walking = "parameters/Legs/conditions/walking";
            
            public const string EquipPistol = "parameters/Arms/conditions/equipPistol";
            public const string Aiming = "parameters/Arms/conditions/aiming";
            public const string Fire = "parameters/Arms/conditions/fire";
        }
    }

    public static class AudioBusNames
    {
        public const string MasterAudioBusName = "Master";
        public const string MusicAudioBusName = "Music";
        public const string SfxAudioBusName = "Sfx";
        public const string VoiceAudioBusName = "Voice";
    }

    public static class AudioAssetPaths
    {
        private const string AudioPrefabFolderPath = "res://audio/";
        private const string MusicPrefabFolderPath = $"{AudioPrefabFolderPath}/music";
        private const string SfxPrefabFolderPath = $"{AudioPrefabFolderPath}/sound";

        public const string CountdownSongPath = $"{MusicPrefabFolderPath}/10MinutesTillBadEnd.wav";
        public const string ClownSongPath = $"{MusicPrefabFolderPath}/Hall of Confused Clowns.wav";

        public const string PainSfxPath = $"{SfxPrefabFolderPath}/Pain.ogg";
    }
}

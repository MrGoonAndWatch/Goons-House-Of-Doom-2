using Godot;
using Godot.Collections;

public partial class GameConstants : GodotObject
{
    public const float GreenMedicineHp = 25.0f;

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
        GreenJuice = 2
    }

    private const string EnemyPrefabFolder = "res://prefabs/spawnables/enemies/";

    public static Dictionary<EnemySpawnType, string> EnemyPrefabLookup = new Dictionary<EnemySpawnType, string>()
    {
        {EnemySpawnType.Shambler, $"{EnemyPrefabFolder}/shambler.tscn"},
        {EnemySpawnType.Chaser, $"{EnemyPrefabFolder}/chaser.tscn"}
    };

    private const string ItemPrefabFolderPath = "res://prefabs/spawnables/items/";
    public const string GarbagePrefabPath = $"{ItemPrefabFolderPath}/garbage.tscn";

    public static Dictionary<ItemSpawnType, string> ItemPrefabLookup = new Dictionary<ItemSpawnType, string>()
    {
        {ItemSpawnType.GreenJuice, $"{ItemPrefabFolderPath}/green-juice.tscn"},
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

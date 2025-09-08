using Godot;
using Godot.Collections;


public partial class GameConstants : GodotObject
{
    public const float GreenMedicineHp = 25.0f;
    public const float ControllerMenuDeadzone = 0.52f;

    /// <summary>
    /// Targets different folders when loading map and scene data.
    /// </summary>
    public const string Mode = "demo";
    public const string SaveDirectoryPath = "user://saves";
    public const string ScenesDirectory = "res://scenes/";
    public const string GlobalSettingsFilename = "global.config";

    // Starting room for the /demo/ and /full/ maps.
    public const string NewGameStartingScenePath = $"{ScenesDirectory}/{Mode}/park_start.tscn";
    // Starting room for the /debug/ maps
    //public const string NewGameStartingScenePath = $"{ScenesDirectory}/{Mode}/test_scene.tscn";

    public static string[] RoomClearCheckBlacklist = {
        "root/root/SubViewport/Player"
    };

    public static class Colors
    {
        public static Color Clear = Color.Color8(0, 0, 0, 0);
        public static Color White = Color.Color8(255, 255, 255);
        public static Color Red   = Color.Color8(255, 0, 0);
    }

    public enum CutsceneTriggerType
    {
        OnSceneLoaded = 0,
    }

    public enum CutsceneInstructionType
    {
        InGameInstruction = 0,
        FmvCutscene = 1,
        ChangeCamera = 2,
    }

    public enum CutsceneInstructionEndType
    {
        EndAfterTime,
        EndWhenMovementEnds,
        EndWhenVideoEnds,
    }

    public enum GameDifficulty
    {
        Easy,
        Normal,
        Hard,
        Impossible
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
        MuseumFrontDoor = 2,
    }

    public enum DoorLoadType
    {
        None = 0,
        WoodDoor1 = 1,
        ParkWalkway = 2,
        MuseumFrontDoor = 3,
    }

    public enum GlobalEvent
    {
        None = 0,
        TestKeypadUnlocked = 1,
        StartedDemoSong = 2,
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
        MuseumFrontDoorKey = 6,
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

    public const string StatusNoteUi = "res://prefabs/ui/menu_note.tscn";

    public static Dictionary<ItemSpawnType, string> ItemPrefabLookup = new Dictionary<ItemSpawnType, string>()
    {
        {ItemSpawnType.GreenJuice, $"{ItemPrefabFolderPath}/green-juice.tscn"},
        {ItemSpawnType.Pistol, $"{ItemPrefabFolderPath}/pistol.tscn"},
        {ItemSpawnType.PistolAmmo, $"{ItemPrefabFolderPath}/pistol-ammo.tscn"},
        {ItemSpawnType.BigKey, $"{ItemPrefabFolderPath}/big-key.tscn" },
        {ItemSpawnType.MuseumFrontDoorKey, $"{ItemPrefabFolderPath}/museum-front-door-key.tscn" }
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
        pause,
        debug_console,
        debug_console_enter,
        debug_console_backspace,
        left_joy,
        right_joy,
        up_joy,
        down_joy,
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

    public enum DoorMapStatus
    {
        Unseen = 0,
        Unknown = 1,
        Opened = 2,
        Locked = 3,
    }

    public enum RoomOrientation
    {
        Normal = 0,
        Rotated90Degrees = 90,
        Rotated180Degrees = 180,
        Rotated270Degrees = 270
    }

    public enum PickupType
    {
        OnTheGround = 0,
        AtTableLevel = 1
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
            public const string TitleScreen = "/root/TitleScreen";
            public const string GameOverScreen = $"{SceneRoot}/GameOver";

            public const string SceneRoot = "/root/root/SubViewport";
            public const string Camera = SceneRoot + "/Camera3D";
            public const string Player = SceneRoot + "/Player";
            public const string PlayerInventory = Player + "/PlayerInventory";
            public const string PlayerStatusScreenHeader = Player + "PlayerStatusScreen/Header";
            public const string ItemBoxControl = Player + "/PlayerItemBoxControl";
            public const string PlayerInteract = Player + "/InteractHitbox";
            public const string InspectTextUi = Player + "/InspectTextUI";
            public const string PlayerStatus = "/root/root/PlayerStatus";
            public const string SaveGameUi = Player + "/save_game_ui";
            public const string CutsceneManager = Player + "/cutscene_ui";
            public const string FmvPlayer = Player + "/fmv_cutscene_ui";
            public const string GammaCorrectionPlayer = Player + "/GammaCorrection/GammaRect";
            public const string GammaCorrectionSolo = SceneRoot + "/GammaCorrection/GammaRect";
            public const string RoomInfo = SceneRoot + "/room_info";
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
            
            public const string PickupOnGround = "parameters/Arms/conditions/pickupOnGround";
            public const string PickupOnTable = "parameters/Arms/conditions/pickupOnTable";

            public const string DeathBlendAmount = "parameters/DeathStateMachine/conditions/death-generic";
            public const string DeathGeneric = "parameters/";
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
        public const string DemoSongPath = $"{MusicPrefabFolderPath}/atmo.wav";

        public const string PainSfxPath = $"{SfxPrefabFolderPath}/Pain.ogg";
        public const string GunshotSfxPath = $"{SfxPrefabFolderPath}/Gunshot.ogg";
    }

    public static class FunnyModeProbabilities
    {
        public const float ChanceToWipeSaveFileOnDeath = 0.1f;
    }

    public static Dictionary<int, ZoneRequirements> ZoneKeyMap = new()
    {
        {1, new ZoneRequirements { KeysRequiredToPassZone = new System.Collections.Generic.List<ItemSpawnType> { ItemSpawnType.MuseumFrontDoorKey, ItemSpawnType.Pistol }, ItemIdsInZone = new System.Collections.Generic.List<int>{ 1, 2, 3, 4, 5 } } },
    };

    public static System.Collections.Generic.List<ItemSpawnType> ItemsWithQty = new() { ItemSpawnType.Pistol, ItemSpawnType.PistolAmmo };

    public partial class ZoneRequirements: Node
    {
        public System.Collections.Generic.List<ItemSpawnType> KeysRequiredToPassZone;
        public System.Collections.Generic.List<int> ItemIdsInZone;
    }

    public static class ShaderParameters
    {
        public const string Gamma = "gamma";
    }

    private static Vector2 GetDigitalMovementVectorRaw()
    {
        var inputDir = Input.GetVector(Controls.left.ToString(), Controls.right.ToString(), Controls.up.ToString(), Controls.down.ToString());
        return inputDir;
    }

    private static Vector2 GetAnalogueMovementVectorRaw()
    {
        var inputDir = Input.GetVector(Controls.left_joy.ToString(), Controls.right_joy.ToString(), Controls.up_joy.ToString(), Controls.down_joy.ToString());
        return inputDir;
    }

    // Note: For some reason the Deadzone property in the project's InputMap wasn't being respected, leading to weird menu movement some of the time.
    private static float ApplyDeadzoneToInputReading(float input)
    {
        if ((input < 0 && input > -ControllerMenuDeadzone) ||
            (input > 0 && input < ControllerMenuDeadzone))
            return 0;
        else
            return input;
    }

    public static (Vector2, bool) GetMovementVectorWithDeadzone()
    {
        var inputDirDigital = GetDigitalMovementVectorRaw();
        var inputDirAnalogue = GetAnalogueMovementVectorRaw();

        var horizontalInputAnalogue = ApplyDeadzoneToInputReading(inputDirAnalogue.X);
        var horizontalInputDigital = ApplyDeadzoneToInputReading(inputDirDigital.X);
        var verticalInputAnalogue = ApplyDeadzoneToInputReading(inputDirAnalogue.Y);
        var verticalInputDigital = ApplyDeadzoneToInputReading(inputDirDigital.Y);

        if (horizontalInputAnalogue != 0 || verticalInputAnalogue != 0)
            return (new Vector2(horizontalInputAnalogue, verticalInputAnalogue), true);
        else
            return (new Vector2(horizontalInputDigital, verticalInputDigital), false);
    }

    public static string GetCurrentRoomName(Node node)
    {
        if (node.HasNode(NodePaths.FromSceneRoot.RoomInfo))
            return node.GetNode<RoomInfo>(NodePaths.FromSceneRoot.RoomInfo).RoomName;
        else // Fallback on scene filename if the scene didn't specify a room name
        {
            var sceneName = GetCurrentSceneFilepath(node);
            var roomStr = sceneName.Contains('/') ? sceneName.Substring(sceneName.LastIndexOf('/') + 1) : sceneName;
            return roomStr;
        }
    }

    public static string GetCurrentSceneFilepath(Node node)
    {
        var roomFileName = node.GetTree().CurrentScene.SceneFilePath.Replace(ScenesDirectory, "").Replace(".tscn", "");
        return roomFileName;
    }

    public static int GetCurrentRoomId(Node node)
    {
        if (node.HasNode(NodePaths.FromSceneRoot.RoomInfo))
            return node.GetNode<RoomInfo>(NodePaths.FromSceneRoot.RoomInfo).RoomId;
        else
        {
            if (!node.HasNode(NodePaths.FromSceneRoot.TitleScreen) && !node.HasNode(NodePaths.FromSceneRoot.GameOverScreen))
                GD.PrintErr($"Couldn't find room_info object in scene {GetCurrentRoomName(node)}!");
            //else
            //    GD.Print("Couldn't find room_info object in scene but this is the title screen so that's expected!");
            return -1;
        }
    }

    public static bool ListContainsValue(int value, System.Collections.Generic.List<int> list)
    {
        for (var i = 0; i < list.Count; i++)
            if (list[i] == value)
                return true;
        return false;
    }
    
    public static System.Collections.Generic.List<T> FindObjectsOfType<T>(Node parent) where T: class
    {
        var matchingNodes = new System.Collections.Generic.List<T>();

        var children = parent.GetChildren();
        foreach (var node in children)
        {
            if (node is T) matchingNodes.Add(node as T);
            matchingNodes.AddRange(FindObjectsOfType<T>(node));
        }

        return matchingNodes;
    }
}

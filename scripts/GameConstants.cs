using Godot;

public partial class GameConstants : GodotObject
{
    public static class Colors
    {
        public static Color Clear = Color.Color8(0, 0, 0, 0);
        public static Color White = Color.Color8(255, 255, 255);
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

    public static class Controls
    {
        public const string Left = "left";
        public const string Right = "right";
        public const string Up = "up";
        public const string Down = "down";

        public const string Run = "run";
        public const string Inventory = "inventory";
        public const string Confirm = "confirm";
        public const string Aim = "aim";
        public const string Pause = "pause";
    }

    public static class NodePaths
    {
        public static class FromSceneRoot
        {
            public const string Player = "/root/root/Player";
            public const string PlayerInventory = Player + "/PlayerInventory";
            public const string PlayerInteract = Player + "/InteractHitbox";
            public const string InspectTextUi = Player + "/InspectTextUi";
            public const string PlayerStatus = "/root/root/PlayerStatus";
            public const string HordeModeManager = "/root/root/HordeModeManager";
        }
    }
}

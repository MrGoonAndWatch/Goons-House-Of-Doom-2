using Godot;
using static GameConstants;
using System.Linq;

public partial class Door : Node3D
{
    [Export]
    public int DoorId;

    [Export]
    private string GoesToRoom;
    [Export]
    private DoorLoadType DoorLoadType;
    [Export]
    private Vector3 StartAtPosition;
    [Export(PropertyHint.Range, "-360,360")]
    private Vector3 StartAtAngle;

    [Export]
    private string[] LockedText;
    [Export]
    private string[] UnlockText;

    [Export]
    private KeyType LocksWith;
    [Export]
    private GlobalEvent UnlocksOnEvent;

    private bool _unlocked;
    private InspectTextUi _textReader;
    private PlayerInventory _playerInventory;

    public override void _Ready()
	{
        _textReader = GetNode<InspectTextUi>(NodePaths.FromSceneRoot.InspectTextUi);
        var gameState = DataSaver.GetInstance().GetGameState();
        if (gameState.DoorsUnlocked.Contains(DoorId))
            _unlocked = true;
        if (gameState.TriggeredEvents.Contains((int)UnlocksOnEvent))
            _unlocked = true;
        if (LocksWith == KeyType.None && UnlocksOnEvent == GlobalEvent.None)
            _unlocked = true;
        _playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
    }

    public void Inspect()
    {
        if (_unlocked)
        {
            var sceneChanger = SceneChanger.GetInstance();
            var sceneChangeInfo = new SceneLoadData
            {
                TargetScene = GoesToRoom,
                LoadPosition = StartAtPosition,
                LoadRotation = StartAtAngle,
            };
            sceneChanger.ChangeScene(sceneChangeInfo, DoorLoadType);
        }
        else if (LocksWith != KeyType.None)
        {
            var key = _playerInventory.GetKeyOfType(LocksWith);
            if (key != null)
                Unlock(key);
            else if (LockedText?.Any() ?? false)
                _textReader.ReadText(LockedText);
        }
        else if (UnlocksOnEvent != GlobalEvent.None && (LockedText?.Any() ?? false))
            _textReader.ReadText(LockedText);
    }

    public void Unlock(Key key)
    {
        if (_unlocked || key.GetKeyType() != LocksWith)
            return;

        _unlocked = true;
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.UnlockDoor(DoorId);
        if (UnlockText?.Any() ?? false)
            _textReader.ReadText(UnlockText);
    }

    public void OnEvent(GlobalEvent globalEvent)
    {
        if (_unlocked || globalEvent != UnlocksOnEvent)
            return;

        _unlocked = true;
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.UnlockDoor(DoorId);
    }

    public void ForceUnlock()
    {
        _unlocked = true;
    }
}

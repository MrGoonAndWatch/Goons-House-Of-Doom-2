using Godot;
using static GameConstants;
using System.Linq;

public partial class Door : Teleporter
{
    [Export]
    private string[] LockedText;
    [Export]
    private string[] UnlockText;

    [Export]
    private KeyType LocksWith;
    [Export]
    private GlobalEvent UnlocksOnEvent;

    private bool _unlocked;
    private PlayerInventory _playerInventory;

    public override void _Ready()
	{
        var gameState = DataSaver.GetInstance().GetGameState();
        if (gameState.DoorsUnlocked.Contains(DoorId))
            _unlocked = true;
        if (gameState.TriggeredEvents.Contains((int)UnlocksOnEvent))
            _unlocked = true;
        if (LocksWith == KeyType.None && UnlocksOnEvent == GlobalEvent.None)
            _unlocked = true;
        _playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
    }

    public void Inspect(InspectTextUi inspectTextUi)
    {
        if (_unlocked)
        {
            ActivateTeleporter();
        }
        else if (LocksWith != KeyType.None)
        {
            var key = _playerInventory.GetKeyOfType(LocksWith);
            if (key != null)
                Unlock(key, inspectTextUi);
            else
            {
                MapStatus.GetInstance().FoundLockedDoor(DoorId);
                if (LockedText?.Any() ?? false)
                    inspectTextUi.ReadText(LockedText);
            }
        }
        else if (UnlocksOnEvent != GlobalEvent.None && (LockedText?.Any() ?? false))
        {
            MapStatus.GetInstance().FoundLockedDoor(DoorId);
            inspectTextUi.ReadText(LockedText);
        }
    }

    public void Unlock(Key key, InspectTextUi inspectTextUi)
    {
        if (_unlocked || key.GetKeyType() != LocksWith)
            return;

        _unlocked = true;
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.UnlockDoor(DoorId);
        var mapStatus = MapStatus.GetInstance();
        mapStatus.EnterDoor(DoorId);
        if (UnlockText?.Any() ?? false)
            inspectTextUi.ReadText(UnlockText);
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

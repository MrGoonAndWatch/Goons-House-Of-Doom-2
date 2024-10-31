using Godot;
using static GameConstants;

public partial class SimpleLock : Node3D
{
    public string[] LockedText;
    public string[] UnlockText;
    public string LootedText = "There's nothing else inside.";
    public KeyType UnlocksWith;
    public Item ContainsItem;

    private bool _unlocked;
    private bool _looted;
    private PlayerInventory _playerInventory;

    public override void _Ready()
	{
        _playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
    }

    public virtual void Inspect(InspectTextUi inspectTextUi)
    {
        if (_looted)
            inspectTextUi.ReadText(new[] { LootedText });
        else if (_unlocked)
        {
            _playerInventory.AddItem(ContainsItem);
            _looted = true;
        }
        else
            inspectTextUi.ReadText(LockedText);
    }

    public virtual void Unlock(Key key, InspectTextUi inspectTextUi)
    {
        if (_unlocked || key.GetKeyType() != UnlocksWith)
            return;

        _unlocked = true;
        inspectTextUi.ReadText(UnlockText);
    }
}

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
    private InspectTextUi _textReader;
    private PlayerInventory _playerInventory;

    public override void _Ready()
	{
        _textReader = GetNode<InspectTextUi>(NodePaths.FromSceneRoot.InspectTextUi);
        _playerInventory = GetNode<PlayerInventory>(NodePaths.FromSceneRoot.PlayerInventory);
    }

    public virtual void Inspect()
    {
        if (_looted)
            _textReader.ReadText(new[] { LootedText });
        else if (_unlocked)
        {
            _playerInventory.AddItem(ContainsItem);
            _looted = true;
        }
        else
            _textReader.ReadText(LockedText);
    }

    public virtual void Unlock(Key key)
    {
        if (_unlocked || key.GetKeyType() != UnlocksWith)
            return;

        _unlocked = true;
        _textReader.ReadText(UnlockText);
    }
}

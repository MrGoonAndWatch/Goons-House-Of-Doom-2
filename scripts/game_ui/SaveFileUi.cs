using Godot;

public partial class SaveFileUi : Control
{
    [Export]
    public Button SaveFileButton;

    public bool IsNewFileSlot;
    public string SaveFileName;

    private SaveGame _saveGameUi;

    public override void _Ready()
    {
        _saveGameUi = GetNode<SaveGame>(GameConstants.NodePaths.FromSceneRoot.SaveGameUi);
    }

    public void _OnButtonPressed()
    {
        _saveGameUi.SaveSlotSelected(this);
    }
}

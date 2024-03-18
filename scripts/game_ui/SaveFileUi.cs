using Godot;

public partial class SaveFileUi : Control
{
    [Export]
    public Button SaveFileButton;

    public bool IsNewFileSlot;
    public string SaveFileName;

    public void _OnButtonPressed()
    {
        var saveGameUi = GetNode<SaveGame>(GameConstants.NodePaths.FromSceneRoot.SaveGameUi);
        saveGameUi.SaveSlotSelected(this);
    }
}

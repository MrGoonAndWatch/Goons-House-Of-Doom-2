using Godot;

public partial class SaveFileUi : Control
{
    [Export]
    public Button SaveFileButton;
    public SaveGame SaveGameUi;

    public bool IsNewFileSlot;
    public string SaveFileName;

    public void _OnButtonPressed()
    {
        SaveGameUi.SaveSlotSelected(this);
    }
}

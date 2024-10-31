using Godot;
using System.Collections.Generic;

public partial class SaveOnInspect : Node3D
{
    [Export]
    private string[] InspectNotes;

    private SaveGame _saveUi;

    public void StartSave(InspectTextUi inspectTextUi, SaveGame saveUi)
    {
        _saveUi = saveUi;
        if (InspectNotes != null && InspectNotes.Length > 0)
            inspectTextUi.ReadText(InspectNotes, onChoiceConfirmed: OpenSaveUi);
        else
            OpenSaveUi(null);
    }

    private void OpenSaveUi(List<string> list)
    {
        _saveUi.ShowSaveUi();
    }
}

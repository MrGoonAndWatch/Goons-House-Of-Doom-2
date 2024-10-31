using Godot;

public partial class ShowTextOnInspect : Node3D
{
    /// <summary>
    /// What text to display when the player inspects this area. Each array element represents one screen of text that will appear before going to the next line.
    /// </summary>
    [Export]
    private string[] InspectLines;
    /// <summary>
    /// Check this if any of your lines use the %var_name% variable replacer string (i.e. for codes and such).
    /// </summary>
    // TODO: Implement this!
    [Export]
    private bool PerformStringReplace;

    public void StartInspection(InspectTextUi inspectTextUi)
    {
        inspectTextUi.ReadText(InspectLines);
    }
}

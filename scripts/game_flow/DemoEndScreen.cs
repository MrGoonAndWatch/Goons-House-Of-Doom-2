using Godot;

public partial class DemoEndScreen : Node
{
    [Export]
    private Button ReturnToTitleButton;

    public override void _Ready()
    {
        ReturnToTitleButton.GrabFocus();
    }

    private void _OnReturnToTitlePressed()
    {
        GetTree().ChangeSceneToFile(GameConstants.TitleScreenScenePath);
    }
}

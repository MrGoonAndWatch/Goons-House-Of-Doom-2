using Godot;

public partial class GameOverScreen : Control
{
    [Export]
    private Button _titleScreenButton;

    public override void _Ready()
    {
        if (_titleScreenButton != null)
            _titleScreenButton.GrabFocus();
    }

    public void _OnReturnToTitle()
    {
        SceneChanger.GetInstance().ChangeSceneDirectly(SceneChanger.TitleScreenScene);
    }
}

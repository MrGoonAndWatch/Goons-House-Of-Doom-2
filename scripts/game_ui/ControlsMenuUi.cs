using Godot;

public partial class ControlsMenuUi : Control
{
    [Export]
    private InputMapper _inputMapper;
    [Export]
    private PauseScreenUi _pauseScreenUi;

    public void OnVisible()
    {
        _inputMapper.OnVisible();
    }

    public void _OnBackClicked()
    {
        _pauseScreenUi.OnControlsClosed();
    }
}
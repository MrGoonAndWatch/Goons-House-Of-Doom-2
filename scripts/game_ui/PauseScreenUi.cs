using Godot;

public partial class PauseScreenUi : Control
{
    [Export]
    private Control PauseMenu;
    [Export]
    private OptionsMenuUi OptionsMenu;
    [Export]
    private Control DefaultPauseMenuFocus;
    [Export]
    private Control DefaultOptionsMenuFocus;
    [Export]
    private ControlsMenuUi ControlsMenu; 

    public void OnPauseMenuOpened()
    {
        Visible = true;
        DefaultPauseMenuFocus.GrabFocus();
    }

    public bool OnPauseMenuClosed()
    {
        if (OptionsMenu.Visible)
        {
            OptionsMenu._OnCancelPressed();
            return true;
        }
        else if (ControlsMenu.Visible)
        {
            ControlsMenu._OnBackClicked();
            return true;
        }
        else
        {
            Visible = false;
            return false;
        }
    }

    public void _OnResumePressed()
    {
        OnPauseMenuClosed();
        PlayerStatus.GetInstance().Paused = false;
    }

    public void _OnOptionsPressed()
    {
        PauseMenu.Visible = false;
        OptionsMenu.Visible = true;
        DefaultOptionsMenuFocus.GrabFocus();
    }

    public void _OnControlsPressed()
    {
        PauseMenu.Visible = false;
        ControlsMenu.Visible = true;
        ControlsMenu.OnVisible();
    }

    public void OnControlsClosed()
    {
        BackToMainPauseMenu(ControlsMenu);
    }

    public void OnOptionsMenuClosed()
    {
        BackToMainPauseMenu(OptionsMenu);
    }

    private void BackToMainPauseMenu(Control menu)
    {
        menu.Visible = false;
        PauseMenu.Visible = true;
        DefaultPauseMenuFocus.GrabFocus();
    }

    public void _OnExitToMainMenu()
    {
        // TODO: Confirm before exiting!
        GetTree().ChangeSceneToFile(GameConstants.TitleScreenScenePath);
    }

    public void _OnExitToDesktop()
    {
        // TODO: Confirm before exiting!
        GetTree().Quit();
    }
}

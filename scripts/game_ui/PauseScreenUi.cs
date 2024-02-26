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

    public void OnOptionsMenuClosed()
    {
        OptionsMenu.Visible = false;
        PauseMenu.Visible = true;
        DefaultPauseMenuFocus.GrabFocus();
    }

    public void _OnExitToMainMenu()
    {
        // TODO: Here I'd change the scene to the main menu screen... IF I HAD ONE
    }

    public void _OnExitToDesktop()
    {
        GetTree().Quit();
    }
}

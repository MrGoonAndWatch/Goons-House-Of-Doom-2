using Godot;

public partial class DebugConsole : Node
{
    [Export]
    private Control _consoleUi;
    [Export]
    private TextEdit _consoleLine;

    public override void _Ready()
    {
        RefreshConsole();
    }

    private void RefreshConsole()
    {
        if (DebugManager.IsDebugConsoleActive())
        {
            _consoleLine.Text = "";
            _consoleLine.GrabFocus();
        }
        _consoleUi.Visible = DebugManager.IsDebugConsoleActive();
    }

    public override void _Process(double delta)
    {
        if (!DataSaver.IsDebugBuild()) return;

        if (Input.IsActionJustPressed(GameConstants.Controls.debug_console.ToString()))
        {
            DebugManager.ToggleDebugConsole();
            RefreshConsole();
        }
        else if (DebugManager.IsDebugConsoleActive() && Input.IsActionJustPressed(GameConstants.Controls.debug_console_enter.ToString()))
        {
            DebugManager.ProcessCommand(_consoleLine.Text);
            DebugManager.ToggleDebugConsole();
            RefreshConsole();
        }
    }
}

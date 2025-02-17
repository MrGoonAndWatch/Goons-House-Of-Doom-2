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
        else if (DebugManager.IsDebugConsoleActive() && Input.IsActionJustPressed(GameConstants.Controls.debug_console_backspace.ToString()))
        {
            BackspaceConsole();
        }
    }

    private void BackspaceConsole()
    {
        if (_consoleLine.Text.Length <= 0) return;
        var isSelecting = _consoleLine.HasSelection();
        var selectionOriginColumn = _consoleLine.GetSelectionOriginColumn();
        var caretColumn = isSelecting ? selectionOriginColumn + _consoleLine.GetSelectedText().Length : _consoleLine.GetCaretColumn();

        var newCaretColumn = isSelecting ? selectionOriginColumn : caretColumn - 1;
        var newText = _consoleLine.Text.Substring(0, newCaretColumn);

        //GD.Print($"caretColumn={caretColumn} newCaretColumn={newCaretColumn} newText={newText} isSelecting={isSelecting} selectionOriginColumn={selectionOriginColumn}");
        if (caretColumn < _consoleLine.Text.Length)
        {
            var newTextSecondHalf = _consoleLine.Text.Substring(caretColumn, _consoleLine.Text.Length - caretColumn);
            newText = newText + newTextSecondHalf;
        }

        _consoleLine.Text = newText;
        _consoleLine.SetCaretColumn(newCaretColumn);
    }
}

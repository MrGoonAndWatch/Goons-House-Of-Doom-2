using Godot;

public partial class DebugConsole : Node
{
    [Export]
    private Control _consoleUi;
    [Export]
    private TextEdit _consoleLine;
    [Export]
    private Label _consoleOutput;

    private int _previousCommandRetrievalIndex = -1;

    public override void _Ready()
    {
        RefreshConsole();
    }

    private void RefreshConsole()
    {
        if (DebugManager.IsDebugConsoleActive())
        {
            _consoleLine.Text = _consoleLine.Text.Replace("`", "");
            _consoleLine.GrabFocus();
            _consoleLine.SetCaretColumn(_consoleLine.Text.Length);
        }
        _consoleUi.Visible = DebugManager.IsDebugConsoleActive();
        _previousCommandRetrievalIndex = -1;
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
            ProcessCommand();
        else if (DebugManager.IsDebugConsoleActive() && Input.IsActionJustPressed(GameConstants.Controls.debug_console_backspace.ToString()))
            BackspaceConsole();
        else if (DebugManager.IsDebugConsoleActive() && Input.IsActionJustPressed(GameConstants.Controls.up.ToString()))
        {
            _previousCommandRetrievalIndex++;
            var endOfList = SetCommandFromHistory();
            if (endOfList)
                _previousCommandRetrievalIndex--;
        }
        else if (DebugManager.IsDebugConsoleActive() && Input.IsActionJustPressed(GameConstants.Controls.down.ToString()))
        {
            if (_previousCommandRetrievalIndex > -1)
                _previousCommandRetrievalIndex--;
            SetCommandFromHistory();
        }
    }

    private void ProcessCommand()
    {
        (var success, var consoleOutput) = DebugManager.ProcessCommand(_consoleLine.Text);
        _consoleLine.Text = "";
        if (!string.IsNullOrEmpty(consoleOutput))
        {
            _consoleOutput.Modulate = success ? GameConstants.Colors.White : GameConstants.Colors.Red;
            _consoleOutput.Text = consoleOutput;
        }
        RefreshConsole();
    }

    private bool SetCommandFromHistory()
    {
        (var previousCommand, var endOfList) = DebugManager.GetPreviousCommand(_previousCommandRetrievalIndex);
        _consoleLine.Text = previousCommand;
        return endOfList;
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

    // Refocus back to the console input if the focus moves.
    public void _OnConsoleFocusExit()
    {
        if (!DebugManager.IsDebugConsoleActive()) return;

        _consoleLine.GrabFocus();
    }
}

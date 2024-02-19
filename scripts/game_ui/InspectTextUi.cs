using Godot;
using System;
using System.Linq;
using System.Text;

public partial class InspectTextUi : Node
{
    private PlayerStatus _playerStatus;
    [Export]
    private Control DescriptiveText;
    [Export]
    private Label TextBox;

    private int _currentLineIndex;
    private string[] _currentLines;
    private string[] _currentChoices;
    private int _currentChoiceSelection;
    private bool _makingChoice;
    private Action _onChoiceConfirmed;
    private bool _justMovedChoice;

    [Export]
    private float AdvanceTextCooldown;
    private double _advanceTextCooldownRemaining = 0.0;

    private bool _queuedText = false;
    private string[] _queuedLines = null;
    private string[] _queuedChoices = null;
    private Action _queuedOnChoiceConfirmed = null;

    public override void _Ready()
    {
        DescriptiveText.Visible = false;
        _playerStatus = PlayerStatus.GetInstance();
    }

    public override void _Process(double delta)
    {
        if (_advanceTextCooldownRemaining > 0)
            _advanceTextCooldownRemaining -= delta;

        if (!_playerStatus.Reading || _advanceTextCooldownRemaining > 0)
            return;

        if (_makingChoice)
        {
            if (Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
                ConfirmChoice();
            else if (Input.IsActionJustPressed(GameConstants.Controls.aim.ToString()))
                CloseTextbox();
            else
                ProcessChoiceMovement();
        }
        else if (Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
            AdvanceText();
    }

    public void QueueReadText(string[] lines, string[] choices = null, Action onChoiceConfirmed = null)
    {
        if (_queuedText)
            return;

        _queuedText = true;
        _queuedLines = lines;
        _queuedChoices = choices;
        _queuedOnChoiceConfirmed = onChoiceConfirmed;
    }

    public void ReadText(string[] lines, string[] choices = null, Action onChoiceConfirmed = null)
    {
        if (_playerStatus.LockMovement || _playerStatus.Reading || _advanceTextCooldownRemaining > 0)
            return;

        _advanceTextCooldownRemaining = AdvanceTextCooldown;
        _currentLineIndex = 0;
        _currentLines = lines;
        _currentChoices = choices;
        _currentChoiceSelection = 0;
        _makingChoice = false;
        _onChoiceConfirmed = onChoiceConfirmed;
        _playerStatus.Reading = true;
        AdvanceText();
        DescriptiveText.Visible = true;
    }

    private void AdvanceText()
    {
        GD.Print("AdvanceText called");

        _advanceTextCooldownRemaining = AdvanceTextCooldown;
        if (_currentLineIndex >= _currentLines.Length)
        {
            if (_currentChoices == null || !_currentChoices.Any())
                CloseTextbox();
            else
            {
                _makingChoice = true;
                _currentChoiceSelection = 0;
                DisplayChoices();
            }

            return;
        }

        TextBox.Text = _currentLines[_currentLineIndex];
        _currentLineIndex++;
    }

    private void DisplayChoices()
    {
        var choicesWithSelection = new StringBuilder();
        
        for (int i = 0; i < _currentChoices.Length; i++)
        {
            if (_currentChoiceSelection == i)
                choicesWithSelection.Append(">");
            choicesWithSelection.Append(_currentChoices[i]);
            if (i + 1 < _currentChoices.Length)
                choicesWithSelection.Append("   ");
        }
        TextBox.Text = choicesWithSelection.ToString();
    }

    public void ForceCloseTextbox()
    {
        if (_queuedText)
            CloseTextbox();
        CloseTextbox();
    }

    private void CloseTextbox()
    {
        DescriptiveText.Visible = false;
        _currentLineIndex = 0;
        _currentLines = null;
        _currentChoices = null;
        _currentChoiceSelection = 0;
        _makingChoice = false;
        _onChoiceConfirmed = null;
        _playerStatus.Reading = false;

        if (_queuedText)
        {
            ReadText(_queuedLines, _queuedChoices, _queuedOnChoiceConfirmed);
            _queuedLines = null;
            _queuedChoices = null;
            _queuedOnChoiceConfirmed = null;
            _queuedText = false;
        }
    }

    private void ProcessChoiceMovement()
    {
        var horizontalMovement = Input.GetVector(GameConstants.Controls.left.ToString(), GameConstants.Controls.right.ToString(), GameConstants.Controls.up.ToString(), GameConstants.Controls.down.ToString()).X;

        if (!_justMovedChoice && horizontalMovement < 0)
        {
            _justMovedChoice = true;
            _advanceTextCooldownRemaining = AdvanceTextCooldown;
            _currentChoiceSelection--;
            if (_currentChoiceSelection < 0)
                _currentChoiceSelection = _currentChoices.Length - 1;
            DisplayChoices();
        }
        else if (!_justMovedChoice && horizontalMovement > 0)
        {
            _justMovedChoice = true;
            _advanceTextCooldownRemaining = AdvanceTextCooldown;
            _currentChoiceSelection = (_currentChoiceSelection + 1) % _currentChoices.Length;
            DisplayChoices();
        }
        else if (_justMovedChoice && horizontalMovement == 0)
            _justMovedChoice = false;
    }

    private void ConfirmChoice()
    {
        // TODO: Can do something more intricate with this system if we pass back which selection was made and let it handle things from there.
        //       For now it just assumes that the last choice is no and everything else is yes.
        if (_currentChoiceSelection == _currentChoices.Length - 1)
        {
            CloseTextbox();
        }
        else
        {
            if (_onChoiceConfirmed != null)
                _onChoiceConfirmed();
            CloseTextbox();
        }
    }
}

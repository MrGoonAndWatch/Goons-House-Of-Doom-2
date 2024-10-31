using Godot;
using System;
using System.Collections.Generic;
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
    private Action<List<string>> _onChoiceConfirmed;
    private bool _justMovedChoice;
    private int _repeatChoices;
    private int _currentChoiceRepeatIndex;
    private List<string> _selectedChoices;

    [Export]
    private float AdvanceTextCooldown;
    private double _advanceTextCooldownRemaining = 0.0;

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
                ForceCloseTextbox();
            else
                ProcessChoiceMovement();
        }
        else if (Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
            AdvanceText();
    }

    public void ReadText(string[] lines, string[] choices = null, Action<List<string>> onChoiceConfirmed = null, int repeatChoices = 1, bool overrideRead = false)
    {
        if ((_playerStatus.Reading && !overrideRead) || _advanceTextCooldownRemaining > 0)
            return;

        GD.Print("ReadText started!");
        _advanceTextCooldownRemaining = AdvanceTextCooldown;
        _currentLineIndex = 0;
        _currentLines = lines;
        _currentChoices = choices;
        _currentChoiceSelection = 0;
        _makingChoice = false;
        _onChoiceConfirmed = onChoiceConfirmed;
        _currentChoiceRepeatIndex = 0;
        _repeatChoices = repeatChoices;
        _playerStatus.Reading = true;
        _selectedChoices = new List<string>();
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
            {
                if (_onChoiceConfirmed != null)
                    _onChoiceConfirmed(null);
                ForceCloseTextbox();
            }
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
        CloseTextbox();
        _onChoiceConfirmed = null;
    }

    private void CloseTextbox()
    {
        DescriptiveText.Visible = false;
        _currentLineIndex = 0;
        _currentLines = null;
        _currentChoices = null;
        _currentChoiceSelection = 0;
        _makingChoice = false;
        _playerStatus.Reading = false;
    }

    private void ProcessChoiceMovement()
    {
        var horizontalMovement = GameConstants.GetMovementVectorRaw().X;

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
        _selectedChoices.Add(_currentChoices[_currentChoiceSelection]);
        _currentChoiceRepeatIndex++;

        if (_currentChoiceRepeatIndex >= _repeatChoices)
        {
            CloseTextbox();
            if (_onChoiceConfirmed != null)
            {
                _onChoiceConfirmed(_selectedChoices);
                _onChoiceConfirmed = null;
            }
        }
    }
}

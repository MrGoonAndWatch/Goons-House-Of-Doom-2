using Godot;
using System;
using System.Collections.Generic;

public partial class PassCode : Node3D
{
	[Export]
	private GameConstants.PassCodeType PassCodeType;
	[Export]
	private string[] InspectText;
	[Export]
	private string[] DigitOptions = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
	[Export]
	private int NumberOfDigits = 1;
	[Export]
	private GameConstants.GlobalEvent SetEventOnUnlock;

	[Export]
	private string[] OnUnlockSuccessText;
	[Export]
	private string[] OnUnlockFailText;

	private bool _interactable = true;

	private InspectTextUi _textReader;

    public override void _Ready()
	{
        _textReader = GetNode<InspectTextUi>(GameConstants.NodePaths.FromSceneRoot.InspectTextUi);
    }

	public void Inspect()
	{
		if (_interactable == false) return;
		_textReader.ReadText(InspectText, DigitOptions, OnCodeEntered, NumberOfDigits);
    }

    public void OnCodeEntered(List<string> code)
	{
		var combinedCode = string.Join("", code);
		var playerStatus = PlayerStatus.GetInstance();
		var gameSettings = playerStatus.GameSettings;
        var correctCode = GameConstants.PassCodeLookup[gameSettings.GameDifficulty][PassCodeType];
        if (gameSettings.IsRandomized && gameSettings.RandomizerSeed.PassCodeLookup.ContainsKey(PassCodeType))
            correctCode = gameSettings.RandomizerSeed.PassCodeLookup[PassCodeType];
		if (correctCode == combinedCode)
		{
			playerStatus.TriggeredEvent(SetEventOnUnlock);
            if (OnUnlockSuccessText != null && OnUnlockSuccessText.Length > 0)
                _textReader.ReadText(OnUnlockSuccessText, overrideRead: true);
			_interactable = false;
        }
        else {
            if (OnUnlockFailText != null && OnUnlockFailText.Length > 0)
				_textReader.ReadText(OnUnlockFailText, overrideRead: true);
		}
    }

	public void OnEvent(GameConstants.GlobalEvent globalEvent)
	{
		if (globalEvent == SetEventOnUnlock)
			_interactable = false;
	}
}

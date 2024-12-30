using Godot;
using System;
using System.Collections.Generic;

public partial class TitleScreenUi : Node
{
    [Export]
    private Control MainMenu;
    [Export]
    private Control NewGameMenu;
    [Export]
    private SaveGame LoadGameMenu;
    [Export]
    private Control OptionsMenu;
    [Export]
    private Control ControlsMenu;
    [Export]
    private Control Credits;
    [Export]
    private Control InitFocusMainMenu;
    [Export]
    private Control InitFocusNewGame;
    [Export]
    private Control InitFocusOptions;
    [Export]
    private Control InitFocusControls;
    [Export]
    private Control InitFocusCredits;
    [Export]
    private CheckBox ItemRandomizerCheckbox;
    [Export]
    private CheckBox EnemyRandomizerCheckbox;
    [Export]
    private CheckBox CodeRandomizerCheckbox;

    [Export]
    private Button[] NewGameDifficultyButtons;
    private int _currentDifficultyIndex;

    private Control _currentMenu;

    public override void _Ready()
    {
        // TODO: Maybe put a song/ambience for the title screen at some point.
        GhodAudioManager.StopMusic();

        NewGameMenu.Visible = false;
        OptionsMenu.Visible = false;
        LoadGameMenu.Visible = false;
        ControlsMenu.Visible = false;
        Credits.Visible = false;
        SwapToMenu(MainMenu, InitFocusMainMenu);
    }

    private void SwapToMenu(Control targetMenu, Control initFocus)
    {
        if(_currentMenu != null)
            _currentMenu.Visible = false;
        targetMenu.Visible = true;
        initFocus.GrabFocus();
        _currentMenu = targetMenu;
    }

    private void RefreshDifficultyButtons()
    {
        for(var i = 0; i < NewGameDifficultyButtons.Length; i++)
        {
            if (i == _currentDifficultyIndex)
                NewGameDifficultyButtons[i].SelfModulate = new Color(1f, 1f, 0.39f);
            else
                NewGameDifficultyButtons[i].SelfModulate = new Color(1, 1, 1);
        }
    }

    public void _OnBackToMainMenu()
    {
        SwapToMenu(MainMenu, InitFocusMainMenu);
    }

    #region MainMenuEvents
    private void _OnNewGamePressed()
    {
        _currentDifficultyIndex = 1;
        RefreshDifficultyButtons();
        SwapToMenu(NewGameMenu, InitFocusNewGame);
    }

    private void _OnLoadGamePressed()
    {
        MainMenu.Visible = false;
        _currentMenu = LoadGameMenu;
        LoadGameMenu.Visible = true;
        LoadGameMenu.ShowLoadUi();
    }

    private void _OnOptionsPressed()
    {
        SwapToMenu(OptionsMenu, InitFocusOptions);
    }

    private void _OnControlsPressed()
    {
        SwapToMenu(ControlsMenu, InitFocusControls);
    }

    private void _OnCreditsPressed()
    {
        SwapToMenu(Credits, InitFocusCredits);
    }

    private void _OnExitPressed()
    {
        GetTree().Quit();
    }
    #endregion

    #region NewGameEvents
    private void _OnDifficultySelectedEasy()
    {
        _currentDifficultyIndex = 0;
        RefreshDifficultyButtons();
    }
    private void _OnDifficultySelectedNormal()
    {
        _currentDifficultyIndex = 1;
        RefreshDifficultyButtons();
    }
    private void _OnDifficultySelectedHard()
    {
        _currentDifficultyIndex = 2;
        RefreshDifficultyButtons();
    }
    private void _OnDifficultySelectedImpossible()
    {
        _currentDifficultyIndex = 3;
        RefreshDifficultyButtons();
    }


    private void _OnStartNewGamePressed()
    {
        var isRandomized = ItemRandomizerCheckbox.ButtonPressed || EnemyRandomizerCheckbox.ButtonPressed || CodeRandomizerCheckbox.ButtonPressed;
        GD.Print($"ItemRandomizerCheckbox.ButtonPressed || EnemyRandomizerCheckbox.ButtonPressed || CodeRandomizerCheckbox.ButtonPressed = {ItemRandomizerCheckbox.ButtonPressed} || {EnemyRandomizerCheckbox.ButtonPressed} || {CodeRandomizerCheckbox.ButtonPressed}");

        var gameSettings = new GameSettings()
        {
            GameDifficulty = (GameConstants.GameDifficulty) _currentDifficultyIndex,
            FunnyMode = false,
            IsRandomized = isRandomized,
        };
        if (gameSettings.IsRandomized)
        {
            SetupRandomizer(gameSettings);
        }

        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.SetupNewGame(gameSettings);
        GetTree().ChangeSceneToFile(GameConstants.NewGameStartingScenePath);
    }

    private void SetupRandomizer(GameSettings gameSettings)
    {
        // TODO: This is a hard coded randomizer setup, need UI for starting game in randomizer mode.
        var randomizerSettings = new RandomizerSettings
        {
            RandomizeItems = ItemRandomizerCheckbox.ButtonPressed,
            RandomizeEnemies = EnemyRandomizerCheckbox.ButtonPressed,
            RandomizePuzzleCodes = CodeRandomizerCheckbox.ButtonPressed,
            AllowSpawnsOnEmptyEnemySlotsForDifficulty = true,
            AllowSpawnsOnEmptyItemSlotsForDifficulty = true,
            EnemySpawnProbabilities = new List<Tuple<GameConstants.EnemySpawnType, float>>
            {
                new Tuple<GameConstants.EnemySpawnType, float>(GameConstants.EnemySpawnType.None, 0.10f),
                new Tuple<GameConstants.EnemySpawnType, float>(GameConstants.EnemySpawnType.Shambler, 0.25f),
                new Tuple<GameConstants.EnemySpawnType, float>(GameConstants.EnemySpawnType.Chaser, 0.65f),
            },
            ItemSpawnProbabilities = new List<Tuple<GameConstants.ItemSpawnType, float>>
            {
                new Tuple<GameConstants.ItemSpawnType, float>(GameConstants.ItemSpawnType.None, 0.34f),
                new Tuple<GameConstants.ItemSpawnType, float>(GameConstants.ItemSpawnType.GreenJuice, 0.33f),
                new Tuple<GameConstants.ItemSpawnType, float>(GameConstants.ItemSpawnType.PistolAmmo, 0.33f),
            },
            //Seed = 1234
        };

        gameSettings.RandomizerSeed = RandomizerSeed.GenerateRandomizer(randomizerSettings);
    }
    #endregion

    #region ControlsEvents

    #endregion
}

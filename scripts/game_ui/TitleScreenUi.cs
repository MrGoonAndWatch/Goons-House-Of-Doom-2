using Godot;

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
    private Control Credits;
    [Export]
    private Control InitFocusMainMenu;
    [Export]
    private Control InitFocusNewGame;
    [Export]
    private Control InitFocusLoadGame;
    [Export]
    private Control InitFocusOptions;
    [Export]
    private Control InitFocusCredits;

    [Export]
    private Button[] NewGameDifficultyButtons;
    private int _currentDifficultyIndex;

    private Control _currentMenu;

    public override void _Ready()
    {
        NewGameMenu.Visible = false;
        OptionsMenu.Visible = false;
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
        _currentMenu = null;
        LoadGameMenu.ShowLoadUi();
    }

    private void _OnOptionsPressed()
    {
        SwapToMenu(OptionsMenu, InitFocusOptions);
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
        var gameSetttings = new GameSettings()
        {
            GameDifficulty = (GameConstants.GameDifficulty) _currentDifficultyIndex,
            FunnyMode = false,
            IsRandomized = false,
            //RandomizerSeed = RandomizerSeed.GenerateRandomizer(settings)
        };
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.SetupNewGame(gameSetttings);
        GetTree().ChangeSceneToFile(GameConstants.NewGameStartingScenePath);
    }
    #endregion

    #region ControlsEvents
    #endregion
}

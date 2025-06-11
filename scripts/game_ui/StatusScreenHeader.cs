using Godot;

public partial class StatusScreenHeader : Control
{
    [Export]
    private StatusScreenTab[] Tabs;
    [Export]
    private Control[] TabHighlights;
    [Export]
    private Control StatusUiPrefab;

    private PlayerStatus _playerStatus;
    private int _currentTabIndex;
    private bool _hasFocus;

    private bool _pressingRight;
    private bool _pressingLeft;

    private bool _returnFocus;

    public override void _Ready()
    {
        _playerStatus = PlayerStatus.GetInstance();
        StatusUiPrefab.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_playerStatus.CanOpenMenu() &&
            Input.IsActionJustPressed(GameConstants.Controls.inventory.ToString()))
            ToggleMenu();

        if (!_playerStatus.MenuOpened) return;

        HandleHeaderInput();
    }

    public void ToggleMenu()
    {
        _playerStatus.MenuOpened = !_playerStatus.MenuOpened;

        StatusUiPrefab.Visible = _playerStatus.MenuOpened;

        if (_playerStatus.MenuOpened)
        {
            for (int i = 0; i < Tabs.Length; i++)
            {
                Tabs[i].OnOpenMenu();
                Tabs[i].SwitchOffTab();
                Tabs[i].Visible = false;
                TabHighlights[i].Visible = false;
            }
            _currentTabIndex = 0;
            Tabs[_currentTabIndex].Visible = true;
            EnterCurrentTab();
        }
    }

    private void HandleHeaderInput()
    {
        // HACK: Need to wait a frame before processing anything otherwise the back button will get double-processed when leaving a tab, causing the menu to instantly close.
        //       For some reason this only happens on the notes and map tabs.
        if (_returnFocus)
        {
            _returnFocus = false;
            _hasFocus = true;
            return;
        }
        if (!_hasFocus) return;

        HandleTabSwapping();
        if (Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
            EnterCurrentTab();
        if (Input.IsActionJustPressed(GameConstants.Controls.aim.ToString()))
            ToggleMenu();
    }

    private void HandleTabSwapping()
    {
        (var inputDir, var _) = GameConstants.GetMovementVectorWithDeadzone();
        var horizontalInput = inputDir.X;

        if (horizontalInput > 0 && !_pressingRight)
        {
            MoveCursorRight();
            _pressingRight = true;
        }
        else if (horizontalInput <= 0)
        {
            _pressingRight = false;
        }

        if (horizontalInput < 0 && !_pressingLeft)
        {
            MoveCursorLeft();
            _pressingLeft = true;
        }
        else if (horizontalInput >= 0)
        {
            _pressingLeft = false;
        }
    }

    private void MoveCursorRight()
    {
        var oldIndex = _currentTabIndex;
        var newIndex = (_currentTabIndex + 1) % Tabs.Length;
        UpdateActiveTab(oldIndex, newIndex);
    }

    private void MoveCursorLeft()
    {
        var oldIndex = _currentTabIndex;
        var newIndex = _currentTabIndex - 1;
        if(newIndex < 0) newIndex = Tabs.Length - 1;
        UpdateActiveTab(oldIndex, newIndex);
    }

    private void UpdateActiveTab(int previousTabIndex, int newTabIndex)
    {
        Tabs[previousTabIndex].Visible = false;
        TabHighlights[previousTabIndex].Visible = false;
        Tabs[newTabIndex].Visible = true;
        TabHighlights[newTabIndex].Visible = true;
        _currentTabIndex = newTabIndex;
    }

    private void EnterCurrentTab()
    {
        _hasFocus = false;
        Tabs[_currentTabIndex].SwitchOnTab();
        TabHighlights[_currentTabIndex].Visible = false;
    }

    public void ReturnFocus()
    {
        _returnFocus = true;
        Tabs[_currentTabIndex].SwitchOffTab();
        TabHighlights[_currentTabIndex].Visible = true;
    }
}

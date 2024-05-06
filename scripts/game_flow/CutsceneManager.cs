using Godot;

public partial class CutsceneManager : Node
{
	[Export]
	private Control TopBar;
    [Export]
    private Control BottomBar;
	[Export]
	private float CinematicBarSpeed;

	private float _targetBottomPosition;
	private float _targetTopPosition;

	private bool _cutsceneStarting;
	private bool _cutsceneEnding;

    private float StartTopBarYPos;
    private float EndTopBarYPos;
	private float StartBottomBarYPos;
	private float EndBottomBarYPos;

	private PlayerStatus _playerStatus;

	private Cutscene _currentCutscene;

    public override void _Ready()
	{
		_playerStatus = PlayerStatus.GetInstance();
		SetSizings();
        ForceCutsceneEnd();
    }

	private void SetSizings()
	{
        var viewportSize = BottomBar.GetViewportRect().End;
        GD.Print($"windowSize = ({viewportSize.X},{viewportSize.Y})");

        var barSize = new Vector2(viewportSize.X, 100);
        TopBar.SetSize(barSize);
        BottomBar.SetSize(barSize);

        var yEdgeOfScreen = viewportSize.Y;
        StartBottomBarYPos = yEdgeOfScreen - barSize.Y;
        EndBottomBarYPos = yEdgeOfScreen;
        StartTopBarYPos = 0;
        EndTopBarYPos = -(barSize.Y);

        GD.Print($"Init set bottom bar to ({BottomBar.GlobalPosition.X},{BottomBar.GlobalPosition.Y})");
        GD.Print($"Target bottom bar start = {StartBottomBarYPos}");
    }

    public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed(GameConstants.Controls.pause.ToString()))
		{
			SkipCutscene();
			return;
		}

		if (_currentCutscene != null && _currentCutscene.HasCutsceneEnded() && !_cutsceneEnding)
		{
			GD.Print("Cutscene has ended!");
			EndCutscene();
		}

        if (_cutsceneStarting && _cutsceneEnding) _cutsceneEnding = false;

        if (_cutsceneStarting)
		{
			var topFinishedStarting = MoveBar(TopBar, 1.0f, delta, StartTopBarYPos);
			var bottomFinishedStarting = MoveBar(BottomBar, -1.0f, delta, StartBottomBarYPos);
			if (topFinishedStarting && bottomFinishedStarting) { 
				_cutsceneStarting = false;
				_currentCutscene.StartCutscene();
			}
		}
		else if (_cutsceneEnding)
		{
            var topFinishedEnd = MoveBar(TopBar, -1.0f, delta, EndTopBarYPos);
            var bottomFinishedEnd = MoveBar(BottomBar, 1.0f, delta, EndBottomBarYPos);
			if (topFinishedEnd && bottomFinishedEnd)
				FinishCutsceneEnd();
        }
    }

	private bool MoveBar(Control bar, float direction, double delta, float targetPos)
	{
        var currentPosition = bar.GlobalPosition;
		var newPos = currentPosition.Y + (float)(direction * CinematicBarSpeed * delta);
		if (direction > 0) newPos = Mathf.Min(newPos, targetPos);
		else newPos = Mathf.Max(newPos, targetPos);
        currentPosition.Y = newPos;
        bar.SetGlobalPosition(currentPosition);

        return newPos == targetPos;
    }

	public void SkipCutscene()
	{
		if (_currentCutscene != null)
			_currentCutscene.SkipCutscene();
		ForceCutsceneEnd();
	}

	private void ForceCutsceneEnd()
	{
        TopBar.SetGlobalPosition(new Vector2(TopBar.GlobalPosition.X, EndTopBarYPos));
        BottomBar.SetGlobalPosition(new Vector2(BottomBar.GlobalPosition.X, EndBottomBarYPos));
		_cutsceneStarting = false;
        FinishCutsceneEnd();
    }

	private void FinishCutsceneEnd()
	{
		if (_currentCutscene != null)
			_playerStatus.SetWatchedCutscene(_currentCutscene.CutsceneId);
		_cutsceneEnding = false;
		_playerStatus.IsInCutscene = false;
		_currentCutscene = null;
	}

	public void StartCutscene(Cutscene cutscene)
	{
		if (_playerStatus.HasWatchedCutscene(cutscene.CutsceneId))
			return;

		_cutsceneStarting = true;
		_playerStatus.IsInCutscene = true;
		_currentCutscene = cutscene;
    }

	public void EndCutscene()
	{
		_cutsceneEnding = true;
    }
}

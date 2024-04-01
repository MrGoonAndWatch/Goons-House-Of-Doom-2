using Godot;
using System;

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
        //if (Input.IsActionJustPressed("DEBUG_Save"))
        //	StartCutscene();
        //if (Input.IsActionJustPressed("DEBUG_Load"))
        //	EndCutscene();
        if (Input.IsActionJustPressed(GameConstants.Controls.pause.ToString()))
            SkipCutscene();

        if (_cutsceneStarting && _cutsceneEnding) _cutsceneEnding = false;

        if (_cutsceneStarting)
		{
			var topFinishedStarting = MoveBar(TopBar, 1.0f, delta, StartTopBarYPos);
			var bottomFinishedStarting = MoveBar(BottomBar, -1.0f, delta, StartBottomBarYPos);
			if (topFinishedStarting && bottomFinishedStarting) _cutsceneStarting = false;
		}
		else if (_cutsceneEnding)
		{
            var topFinishedEnd = MoveBar(TopBar, -1.0f, delta, EndTopBarYPos);
            var bottomFinishedEnd = MoveBar(BottomBar, 1.0f, delta, EndBottomBarYPos);
			if (topFinishedEnd && bottomFinishedEnd)
			{
				_cutsceneEnding = false;
				_playerStatus.IsInCutscene = false;
            }
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
		ForceCutsceneEnd();
	}

	private void ForceCutsceneEnd()
	{
        TopBar.SetGlobalPosition(new Vector2(TopBar.GlobalPosition.X, EndTopBarYPos));
        BottomBar.SetGlobalPosition(new Vector2(BottomBar.GlobalPosition.X, EndBottomBarYPos));
		_cutsceneStarting = false;
		_cutsceneEnding = false;
		_playerStatus.IsInCutscene = false;
    }

	public void StartCutscene()
	{
		_cutsceneStarting = true;
		_playerStatus.IsInCutscene = true;
    }

	public void EndCutscene()
	{
		_cutsceneEnding = true;
	}
}

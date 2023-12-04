using Godot;

public partial class AnimationTest : Node
{
	[Export]
	private AnimationTree _tree;

    public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed(GameConstants.Controls.aim.ToString()))
        {
            _tree.Set(GameConstants.Animation.Player.Walking, false);
            _tree.Set(GameConstants.Animation.Player.Idle, true);
        }
        if (Input.IsActionJustPressed(GameConstants.Controls.up.ToString()))
        {
            _tree.Set(GameConstants.Animation.Player.Idle, false);
            _tree.Set(GameConstants.Animation.Player.Walking, true);
        }
    }
}

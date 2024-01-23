using Godot;

public partial class AnimationTest : Node
{
	[Export]
	private AnimationTree _tree;

    public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed(GameConstants.Controls.aim.ToString()))
            _tree.Set(GameConstants.Animation.Player.Walking, false);
        if (Input.IsActionJustPressed(GameConstants.Controls.up.ToString()))
            _tree.Set(GameConstants.Animation.Player.IdleLegs, false);
    }
}

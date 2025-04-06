using Godot;

public partial class PlayerAnimationEventHandler : AnimationTree
{
	private Player _player;

    public override void _Ready()
	{
        _player = GetNode<Player>(GameConstants.NodePaths.FromSceneRoot.Player);
    }

	public void _OnAnimationFinished(StringName animationName)
	{
		var animationStr = animationName.ToString();
        //GD.Print($"_OnAnimationFinished('{animationName}')");
        if (animationStr.StartsWith("Fire-"))
			_player.OnShootingEnded();
		if (animationStr.StartsWith("Aim-"))
			_player.OnShootingReady();
		if (animationStr.StartsWith("Death-"))
			_player.OnDeathAnimationFinished(animationStr);
	}
}

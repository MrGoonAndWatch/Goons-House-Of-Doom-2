using Godot;
using System;

public partial class PlayerAnimationEventHandler : AnimationTree
{
	private Player _player;

    public override void _Ready()
	{
        _player = GetNode<Player>(GameConstants.NodePaths.FromSceneRoot.Player);
    }

	public void _OnAnimationFinished(StringName animationName)
	{
		//GD.Print($"_OnAnimationFinished('{animationName}')");
		if (animationName.ToString().StartsWith("Fire-"))
			_player.OnShootingEnded();
		if (animationName.ToString().StartsWith("Aim-"))
			_player.OnShootingReady();
	}
}

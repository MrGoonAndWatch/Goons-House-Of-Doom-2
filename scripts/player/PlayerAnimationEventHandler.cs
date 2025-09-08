using Godot;

public partial class PlayerAnimationEventHandler : AnimationTree
{
	private Player _player;
	private PlayerInteract _playerInteract;

    public override void _Ready()
	{
        _player = GetNode<Player>(GameConstants.NodePaths.FromSceneRoot.Player);
        _playerInteract = GetNode<PlayerInteract>(GameConstants.NodePaths.FromSceneRoot.PlayerInteract);
        PlayerAnimationControl.GetInstance().Init(this);
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
		if (animationStr.StartsWith("pickup_") && animationStr.EndsWith("_start"))
			_playerInteract.OnPickupAnimationFinished();
		if (animationStr.StartsWith("pickup_") && animationStr.EndsWith("_end"))
			PlayerStatus.GetInstance().SetIsPickingUpItem(false);
	}
}

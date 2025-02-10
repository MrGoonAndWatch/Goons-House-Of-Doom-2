using Godot;

public partial class PlayerAnimationControl : Node3D
{
    [Export]
    private AnimationTree _animationTree;

    public void SetAnimationVariable(string flagName, Variant newValue)
    {
        _animationTree.Set(flagName, newValue);
    }
}

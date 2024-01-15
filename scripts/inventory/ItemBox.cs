using Godot;

public partial class ItemBox : StaticBody3D
{
    public void OpenBox()
    {
        GetNode<PlayerItemBoxControl>(GameConstants.NodePaths.FromSceneRoot.ItemBoxControl).OpenMenu();
    }
}

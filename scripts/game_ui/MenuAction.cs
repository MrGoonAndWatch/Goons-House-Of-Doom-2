using Godot;

public partial class MenuAction : Control
{
    [Export]
    public GameConstants.MenuActionType ActionType;
    [Export]
    public Label Textbox;
}

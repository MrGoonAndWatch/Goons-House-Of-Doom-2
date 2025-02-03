using Godot;

public partial class DebugUi : Control
{
    [Export]
    private Label PlayerPositionLabel;
    [Export]
    private Label PlayerRotationLabel;

    private bool _debugging;

    public override void _Ready()
    {
        _debugging = Visible;
    }

    public override void _Process(double delta)
    {
        if (!_debugging) return;

        var playerPos = PlayerStatus.GetInstance().GetPlayerPosition();
        PlayerPositionLabel.Text = playerPos.ToString("0.00");

        var playerRot = PlayerStatus.GetInstance().GetPlayerAngle();
        PlayerRotationLabel.Text = playerRot.ToString("0.00");
    }
}

using Godot;

public abstract partial class StatusScreenTab : Control
{
    [Export]
    protected StatusScreenHeader StatusScreenHeader;

    protected bool IsActiveTab { get; private set; }

    public virtual void SwitchOnTab() {
        IsActiveTab = true;
    }

    public virtual void SwitchOffTab() {
        IsActiveTab = false;
    }

    protected void CheckForReturnToHeader()
    {
        if (!IsActiveTab) return;

        if (Input.IsActionJustPressed(GameConstants.Controls.aim.ToString()))
            StatusScreenHeader.ReturnFocus();
    }

    public bool IsCurrentlyActiveTab() { return IsActiveTab; }

    public abstract void OnOpenMenu();
}

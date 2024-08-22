using Godot;

public abstract partial class StatusScreenTab : Control
{
    protected bool IsActiveTab { get; private set; }

    public void SwitchOnTab() {
        IsActiveTab = true;
    }

    public void SwitchOffTab() {
        IsActiveTab = false;
    }

    public bool IsCurrentlyActiveTab() { return IsActiveTab; }

    public abstract void OnOpenMenu();
}

using Godot;

public partial class InventoryStatusUi : StatusScreenTab
{
    [Export]
    private PlayerInventory playerInventory;

    public override void OnOpenMenu()
    {
        playerInventory.OnOpenMenu();
    }

    public void ToggleMenu()
    {
        StatusScreenHeader.ToggleMenu();
    }

    public void ReturnFocus()
    {
        StatusScreenHeader.ReturnFocus();
    }
}

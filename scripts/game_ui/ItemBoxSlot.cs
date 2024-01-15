public partial class ItemBoxSlot : ItemSlot
{
    public override string GetQtyDisplay()
    {
        if (Item == null)
            return GameConstants.UiLabels.EmptyItemBoxSlotText;

        var qtyStr = base.GetQtyDisplay();
        if (!string.IsNullOrEmpty(qtyStr))
            qtyStr = $" ({qtyStr})";

        return $"{Item.GetItemName()}{qtyStr}";
    }
}

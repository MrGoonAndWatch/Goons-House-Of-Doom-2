using Godot;
using System;

public partial class ItemSlot : Control
{
    [Export]
    public TextureRect ItemSprite;

    public int Qty;
    public Item Item;

    public void DiscardItem()
    {
        Item = null;
    }

    public void Combine(ItemSlot itemB)
    {
        var maxStackSize = itemB.Item.GetMaxStackSize();
        if (Item.GetType() == itemB.Item.GetType() && Item.IsStackable() && maxStackSize.HasValue)
        {
            var availableQty = maxStackSize.Value - itemB.Qty;
            var qtyTransferred = Math.Min(Qty, availableQty);
            itemB.Qty += qtyTransferred;

            Qty -= qtyTransferred;
            if (Qty <= 0)
                DiscardItem();

            return;
        }

        var comboResult = Item.Combine(itemB.Item);
        if (comboResult.ItemA == null)
            DiscardItem();
        else
            Item = comboResult.ItemA;

        if (comboResult.ItemB == null)
            itemB.DiscardItem();
        else
            itemB.Item = comboResult.ItemB;
    }

    public string GetQtyDisplay()
    {
        if (Item == null)
            return "";

        var weapon = Item as Weapon;
        if (weapon?.ShowQty() ?? false)
            return $"{weapon.Ammo}";
        if (Item.IsStackable())
            return $"{Qty}";

        return "";
    }
}

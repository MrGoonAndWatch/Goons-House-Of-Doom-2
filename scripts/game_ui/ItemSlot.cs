using Godot;
using System;

public partial class ItemSlot : Control
{
    [Export]
    public TextureRect ItemSprite;
    [Export]
    protected Label ItemQtyLabel;

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

    public void InitUi(Item item, int qty)
    {
        Item = item;
        if (Item != null)
        {
            ItemSprite.Texture = Item.MenuIcon;
            if (Item is Weapon)
            {
                (Item as Weapon).Ammo = qty;
                Qty = 1;
            }
            else
                Qty = qty;
        }

        UpdateUi();
    }

    public void SwapItemSlots(ItemSlot other)
    {
        // TODO: Merge ammo stacks if they're the same ammo type.
        var otherItem = other.Item;
        var otherQty = other.Qty;
        other.Item = Item;
        other.Qty = Qty;
        Item = otherItem;
        Qty = otherQty;

        other.UpdateUi();
        UpdateUi();
    }

    public void CopyItemSlot(ItemSlot other)
    {
        Item = other.Item;
        Qty = other.Qty;
        UpdateUi();
    }

    public void UpdateUi()
    {
        if (Item == null)
            ItemSprite.Modulate = GameConstants.Colors.Clear;
        else
        {
            ItemSprite.Texture = Item.MenuIcon;
            ItemSprite.Modulate = GameConstants.Colors.White;
        }

        ItemQtyLabel.Text = GetQtyDisplay();
    }

    public virtual string GetQtyDisplay()
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

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

    public static void StackItemSlots(ItemSlot itemA, ItemSlot itemB)
    {
        var maxStackSize = itemB.Item.GetMaxStackSize() ?? int.MaxValue;
        var availableQty = maxStackSize - itemB.Qty;
        var qtyTransferred = Math.Min(itemA.Qty, availableQty);
        itemB.Qty += qtyTransferred;

        itemA.Qty -= qtyTransferred;
        if (itemA.Qty <= 0)
            itemA.DiscardItem();
    }

    public void Combine(ItemSlot itemB)
    {
        if (Item.IsStackableWith(itemB?.Item))
        {
            StackItemSlots(this, itemB);
            return;
        }

        var mergedAmmoWithWeapon = false;
        if (Item is Weapon)
            mergedAmmoWithWeapon = MergeAmmoInToWeapon(this, itemB);
        else if (itemB.Item is Weapon)
            mergedAmmoWithWeapon = MergeAmmoInToWeapon(itemB, this);
        if(mergedAmmoWithWeapon)
            return;

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

    private static bool MergeAmmoInToWeapon(ItemSlot weapon, ItemSlot other)
    {
        var weaponItem = weapon.Item as Weapon;
        if (other.Item.GetType() != weaponItem.GetAmmoType()) return false;

        if (weaponItem.GetMaxStackSize().HasValue)
        {
            var maxAmmoToAdd = weaponItem.GetMaxStackSize().Value - weaponItem.GetAmmo();
            var actualAmmoAdded = Math.Min(other.Qty, maxAmmoToAdd);

            weaponItem.AddAmmo(actualAmmoAdded);
            other.Qty -= actualAmmoAdded;
        }
        else
        {
            weaponItem.AddAmmo(other.Qty);
            other.Qty = 0;
        }

        if (other.Qty <= 0) other.DiscardItem();

        return true;
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

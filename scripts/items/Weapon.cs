using System;

public abstract partial class Weapon : Item
{
    public int Ammo;

    protected virtual bool IsUnlimited()
    {
        return false;
    }

    public int GetAmmo()
    {
        return IsUnlimited() ? 1 : Ammo;
    }

    public void AddAmmo(int amount = -1)
    {
        if (IsUnlimited()) return;
        Ammo += amount;
    }

    public abstract void PlaySfx();
    public abstract bool IsHitscan();
    public abstract float GetDamagePerHit();
    public abstract string GetEquipAnimationName();
    public abstract Type GetAmmoType();

    public override bool IsStackable()
    {
        return false;
    }

    public override bool UseItem()
    {
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.EquipWeapon(this);
        playerStatus.SetInventoryEquipDirty();
        return false;
    }

    public override ComboResult Combine(Item otherItem)
    {
        return new ComboResult
        {
            ItemA = this,
            ItemB = otherItem,
        };
    }

    public virtual bool ShowQty()
    {
        return true;
    }
}

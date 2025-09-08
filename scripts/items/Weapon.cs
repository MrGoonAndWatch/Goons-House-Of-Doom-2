using Godot;
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

    public void AddAmmo(int amount)
    {
        if (IsUnlimited()) return;
        Ammo += amount;
        if (Ammo < 0) Ammo = 0;
    }

    public abstract void PlaySfx();
    public abstract bool IsHitscan();
    public abstract float GetDamagePerHit();
    public abstract string GetEquipAnimationName();
    public abstract string GetFireAnimationName();
    public abstract Type GetAmmoType();

    public override bool IsStackable()
    {
        return false;
    }

    public override bool UseItem()
    {
        var playerStatus = PlayerStatus.GetInstance();
        playerStatus.EquipWeapon(this);

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

    public float ShootWeapon(PlayerInventory playerInventory, RayCast3D hitscanRay, PlayerAnimationControl playerAnimationControl)
    {
        var playerStatus = PlayerStatus.GetInstance();
        if (GetAmmo() > 0)
        {
            playerStatus.ReadyToShoot = false;
            PlayerAnimationControl.FireWeapon();
            PlaySfx();
            AddAmmo(-1);
            playerInventory.SetAllDirty();
            if (IsHitscan())
            {
                var collider = hitscanRay.GetCollider();
                if (collider is Enemy)
                    (collider as Enemy).TakeDamage(GetDamagePerHit());
            }
            else
            {
                // TODO: Handle knives and other non-hitscan weapons!
            }
        }
        else if (playerStatus.HasAmmoInInventory(playerInventory))
        {
            // TODO: Play reload animation.
            playerStatus.AddAmmoToCurrentWeaponFromInventory(playerInventory);
        }
        else
        {
            // TODO: Play no ammo click.
        }

        return PlayerAnimationControl.GetShootAnimationDuration(this);
    }
}

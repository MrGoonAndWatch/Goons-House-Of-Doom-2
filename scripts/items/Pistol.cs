using System;

public partial class Pistol : Weapon
{
    public override float GetDamagePerHit()
    {
        return 2.0f;
    }

    public override string GetDescription()
    {
        return "A basic handgun";
    }

    public override string GetEquipAnimationName()
    {
        return GameConstants.Animation.Player.EquipPistol;
    }

    public override string GetFireAnimationName()
    {
        return GameConstants.Animation.Player.Fire;
    }

    public override string GetItemName()
    {
        return "Pistol";
    }

    public override string GetPrefabPath()
    {
        return GameConstants.ItemPrefabLookup[GameConstants.ItemSpawnType.Pistol];
    }

    public override bool IsHitscan()
    {
        return true;
    }

    public override void PlaySfx()
    {
        GhodAudioManager.PlayPistolFired();
    }

    public override int? GetMaxStackSize()
    {
        return 10;
    }

    public override Type GetAmmoType()
    {
        return typeof(PistolAmmo);
    }
}

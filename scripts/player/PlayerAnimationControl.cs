using Godot;

using PickupType = GameConstants.PickupType;
using Animation = GameConstants.Animation;

public partial class PlayerAnimationControl : Node3D
{
    private static PlayerAnimationControl Instance;
    
    private AnimationTree _animationTree;

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr($"Found more than one instance of PlayerAnimationControl, this should be a singleton! Ignoring instance '{GetInstanceId()}' ({Name})");
            return;
        }
        
        Instance = this;
    }

    public static PlayerAnimationControl GetInstance()
    {
        return Instance;
    }

    public void Init(AnimationTree animationTree)
    {
        _animationTree = animationTree;
    }

    private static bool ValidateInstance()
    {
        var valid = true;
        if (Instance == null)
        {
            GD.PrintErr("Unable to trigger animation, no instance of PlayerAnimationControl was found!");
            valid = false;
        }
        else if (Instance._animationTree == null)
        {
            GD.PrintErr("Unable to trigger animation, PlayerAnimationControl has null AnimationTree!");
            valid = false;
        }
        return valid;
    }

    public void StopMoving()
    {
        UpdateMovementFlags(false, false);
    }

    public void StartWalking()
    {
        UpdateMovementFlags(true, false);
    }

    public void StartRunning()
    {
        UpdateMovementFlags(true, true);
    }

    public void EquipWeapon(Weapon weapon)
    {
        SetAnimationVariable(weapon.GetEquipAnimationName(), true);
    }

    public void UnequipWeapon(Weapon weapon)
    {
        StopAiming();
        SetAnimationVariable(weapon.GetEquipAnimationName(), false);
    }

    public void StartAiming()
    {
        SetAnimationVariable(Animation.Player.Aiming, true);
    }

    public void StopAiming()
    {
        SetAnimationVariable(Animation.Player.Aiming, false);
    }

    public static void FireWeapon()
    {
        if (!ValidateInstance()) return;
        Instance.PlayFireWeapon();
    }
    
    public void OnShootingEnded()
    {
        SetAnimationVariable(Animation.Player.Fire, false);
    }

    public void BeginPickup(PickupType pickupType)
    {
        string itemPickupAnimationFlag;
        switch (pickupType)
        {
            case PickupType.AtTableLevel:
                itemPickupAnimationFlag = Animation.Player.PickupOnTable;
                break;
            case PickupType.OnTheGround:
            default:
                itemPickupAnimationFlag = Animation.Player.PickupOnGround;
                break;
        }
        
        SetAnimationVariable(itemPickupAnimationFlag, true);
    }

    public void EndPickup()
    {
        SetAnimationVariable(Animation.Player.PickupOnGround, false);
        SetAnimationVariable(Animation.Player.PickupOnTable, false);
    }

    public void GenericDeath()
    {
        SetAnimationVariable(Animation.Player.DeathBlendAmount, 1.0);
        SetAnimationVariable(Animation.Player.DeathGeneric, true);   
    }

    private void PlayFireWeapon()
    {
        // TODO: Get currently equipped weapon from player status and decide which weapon animation to trigger here!
        SetAnimationVariable(Animation.Player.Fire, true);
    }
    
    private void UpdateMovementFlags(bool walking, bool running)
    {
        SetAnimationVariable(Animation.Player.Walking, walking);
        SetAnimationVariable(Animation.Player.Running, running);
    }

    public void SetAnimationVariable(string flagName, Variant newValue)
    {
        _animationTree.Set(flagName, newValue);
    }
}

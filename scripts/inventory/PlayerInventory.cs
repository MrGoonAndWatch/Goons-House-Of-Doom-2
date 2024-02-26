using Godot;
using System;

public partial class PlayerInventory : Node3D
{
    [Export]
    private Label ExamineText;
    [Export]
    private TextureRect ExamineTexture;

    [Export]
    public ItemSlot[] Items;
    [Export]
    private Control ItemCursor;
    [Export]
    private Control MenuActionRoot;
    [Export]
    private MenuAction[] MenuActions;
    [Export]
    private Control ActionCursor;
    [Export]
    private Control MenuPrefab;
    [Export]
    private TextureRect EquipSlot;

    private int _currentItemIndex;
    private bool _actionMenuOpen;
    private bool _combiningItems;

    private bool _pressingLeft;
    private bool _pressingRight;
    private bool _pressingUp;
    private bool _pressingDown;

    private int _currentActionIndex;
    private int _comboSelectionIndex;

    private bool firstActionMenuOpen = true;

    public bool EquipDirty;
    public bool[] ItemDirty;
    public bool SyncedWithItemBox;

    private PlayerStatus _playerStatus;
    private PlayerItemBoxControl _itemBoxControl;

    private bool _menuEnabled;

    public override void _Ready()
    {
        _playerStatus = PlayerStatus.GetInstance();
        _itemBoxControl = GetNode<PlayerItemBoxControl>(GameConstants.NodePaths.FromSceneRoot.ItemBoxControl);
        MenuPrefab.Visible = false;

        ItemDirty = new bool[6];
        for (var i = 0; i < ItemDirty.Length; i++)
            ItemDirty[i] = true;

        EquipDirty = true;
    }

    public void OnOpenMenu()
    {
        ExamineTexture.Texture = null;
        ExamineTexture.Modulate = GameConstants.Colors.Clear;
        ExamineText.Text = "";
        _combiningItems = false;
        _actionMenuOpen = false;
        CloseActionMenu();
        _currentItemIndex = 0;
        UpdateItemCursorPosition();
    }

    public override void _Process(double delta)
    {
        if (_playerStatus.CanOpenMenu() &&
            Input.IsActionJustPressed(GameConstants.Controls.inventory.ToString()))
            ToggleMenu();

        if (!SyncedWithItemBox)
        {
            _itemBoxControl.SyncInventory(Items);
            SyncedWithItemBox = true;
        }

        if (!_playerStatus.MenuOpened)
            return;

        for (var i = 0; i < ItemDirty.Length; i++)
        {
            if (ItemDirty[i])
                UpdateItemUi(i);
        }
        if (EquipDirty)
            UpdateEquipUi();

        var inputDir = Input.GetVector(GameConstants.Controls.left.ToString(), GameConstants.Controls.right.ToString(), GameConstants.Controls.up.ToString(), GameConstants.Controls.down.ToString());

        if (_actionMenuOpen && !_combiningItems)
            HandleActionCursorMovement(inputDir.Y);
        else
            HandleItemCursorMovement(inputDir.X, inputDir.Y);

        if (Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
            HandleConfirmPressed();

        if (Input.IsActionJustPressed(GameConstants.Controls.aim.ToString()))
            HandleBackPressed();
    }

    public void SetAllDirty()
    {
        for (var i = 0; i < ItemDirty.Length; i++)
            ItemDirty[i] = true;
    }

    public void SyncInventory(ItemSlot[] items)
    {
        SyncedWithItemBox = true;
        for (var i = 0; i < items.Length && i < Items.Length; i++)
            Items[i].CopyItemSlot(items[i]);
        UpdateEquipUi();
    }

    void UpdateItemUi(int i)
    {
        var targetItem = Items[i];
        targetItem.UpdateUi();
        if (targetItem.Item == null)
        {
            ExamineText.Text = "";
            ExamineTexture.Modulate = GameConstants.Colors.Clear;
        }
        ItemDirty[i] = false;
    }

    void UpdateEquipUi()
    {
        if (_playerStatus.EquipedWeapon == null)
            EquipSlot.Modulate = GameConstants.Colors.Clear;
        else
        {
            EquipSlot.Texture = _playerStatus.EquipedWeapon.MenuIcon;
            EquipSlot.Modulate = GameConstants.Colors.White;
        }
    }

    void HandleConfirmPressed()
    {
        if (Items[_currentItemIndex].Item == null)
            return;

        //SoundManager.PlayMenuSelectSfx();

        if (_combiningItems && _comboSelectionIndex != _currentItemIndex)
            CombineItems(_comboSelectionIndex, _currentItemIndex);
        else if (_actionMenuOpen)
            DoAction();
        else if (!_combiningItems && !_actionMenuOpen)
            OpenActionMenu();
    }

    void HandleBackPressed()
    {
        if (_combiningItems)
            _combiningItems = false;
        else if (_actionMenuOpen)
            CloseActionMenu();
        else
            ToggleMenu();
    }

    void HandleActionCursorMovement(float vertical)
    {
        if (vertical < 0 && !_pressingUp)
        {
            if (_currentActionIndex == 0)
                _currentActionIndex = MenuActions.Length - 1;
            else
                _currentActionIndex--;
            UpdateActionCursorPosition();
            _pressingUp = true;
        }
        else if (vertical >= 0)
        {
            _pressingUp = false;
        }

        if (vertical > 0 && !_pressingDown)
        {
            _currentActionIndex = (_currentActionIndex + 1) % MenuActions.Length;
            UpdateActionCursorPosition();
            _pressingDown = true;
        }
        else if (vertical <= 0)
        {
            _pressingDown = false;
        }
    }

    void HandleItemCursorMovement(float horizontal, float vertical)
    {
        // Note: For some reason the Deadzone property in the project's InputMap wasn't being respected, leading to weird menu movement some of the time.
        if ((horizontal < 0 && horizontal > -GameConstants.ControllerMenuDeadzone) ||
            (horizontal > 0 && horizontal < GameConstants.ControllerMenuDeadzone))
            horizontal = 0;
        if ((vertical < 0 && vertical > -GameConstants.ControllerMenuDeadzone) ||
            (vertical > 0 && vertical < GameConstants.ControllerMenuDeadzone))
            vertical = 0;

        if (horizontal > 0 && !_pressingRight)
        {
            MoveItemCursorRight();
            _pressingRight = true;
        }
        else if (horizontal <= 0)
        {
            _pressingRight = false;
        }

        if (horizontal < 0 && !_pressingLeft)
        {
            MoveItemCursorLeft();
            _pressingLeft = true;
        }
        else if (horizontal >= 0)
        {
            _pressingLeft = false;
        }

        if (vertical < 0 && !_pressingUp)
        {
            MoveItemCursorUp();
            _pressingUp = true;
        }
        else if (vertical >= 0)
        {
            _pressingUp = false;
        }

        if (vertical > 0 && !_pressingDown)
        {
            MoveItemCursorDown();
            _pressingDown = true;
        }
        else if (vertical <= 0)
        {
            _pressingDown = false;
        }
    }

    void MoveItemCursorRight()
    {
        _currentItemIndex = (_currentItemIndex + 1) % Items.Length;
        UpdateItemCursorPosition();
    }

    void MoveItemCursorLeft()
    {
        if (_currentItemIndex == 0)
            _currentItemIndex = Items.Length - 1;
        else
            _currentItemIndex--;
        UpdateItemCursorPosition();
    }

    void MoveItemCursorUp()
    {
        if (_currentItemIndex < 2)
            _currentItemIndex = Items.Length - (2 - _currentItemIndex);
        else
            _currentItemIndex -= 2;
        UpdateItemCursorPosition();
    }

    void MoveItemCursorDown()
    {
        _currentItemIndex = (_currentItemIndex + 2) % Items.Length;
        UpdateItemCursorPosition();
    }

    void UpdateItemCursorPosition()
    {
        //SoundManager.PlayMenuMoveSfx();
        var targetSlot = Items[_currentItemIndex].Position;
        ItemCursor.Position = targetSlot;
    }

    void UpdateActionCursorPosition()
    {
        //SoundManager.PlayMenuMoveSfx();
        var targetSlot = MenuActions[_currentActionIndex].Position;
        ActionCursor.Position = targetSlot;
    }

    void DoAction()
    {
        var action = MenuActions[_currentActionIndex].ActionType;
        switch (action)
        {
            case GameConstants.MenuActionType.Use:
                var usedItem = Items[_currentItemIndex].Item.UseItem();
                if (usedItem)
                    UsedItem();
                CloseActionMenu();
                ToggleMenu();
                break;
            case GameConstants.MenuActionType.Combine:
                _combiningItems = true;
                _comboSelectionIndex = _currentItemIndex;
                break;
            case GameConstants.MenuActionType.Examine:
                ExamineCurrentItem();
                break;
            case GameConstants.MenuActionType.Discard:
                Items[_currentItemIndex].DiscardItem();
                ItemDirty[_currentItemIndex] = true;
                SyncedWithItemBox = false;
                CloseActionMenu();
                break;
        }
    }

    void CloseActionMenu()
    {
        MenuActionRoot.Visible = false;
        _actionMenuOpen = false;
    }

    void ExamineCurrentItem()
    {
        var currentItem = Items[_currentItemIndex].Item;
        if (currentItem == null)
            return;

        ExamineTexture.Texture = currentItem.MenuIcon;
        ExamineTexture.Modulate = GameConstants.Colors.White;
        ExamineText.Text = currentItem.GetDescription();
    }

    void CombineItems(int itemA, int itemB)
    {
        Items[itemA].Combine(Items[itemB]);
        ItemDirty[itemA] = true;
        ItemDirty[itemB] = true;
        SyncedWithItemBox = false;
        _combiningItems = false;
        CloseActionMenu();
    }

    void OpenActionMenu()
    {
        _currentActionIndex = 0;
        UpdateActionMenuText();
        MenuActionRoot.Visible = true;
        // HACK: For some reason the first time this menu opens it has the wrong position for the action panels, so just skip that step once.
        if (firstActionMenuOpen)
            firstActionMenuOpen = false;
        else
            UpdateActionCursorPosition();
        _actionMenuOpen = true;
    }

    private void UpdateActionMenuText()
    {
        foreach (var menuAction in MenuActions)
        {
            if (menuAction.ActionType == GameConstants.MenuActionType.Use)
            {
                if (Items[_currentItemIndex].Item is Weapon)
                {
                    if (_playerStatus.EquipedWeapon == null || _playerStatus.EquipedWeapon.ItemId != Items[_currentItemIndex].Item.ItemId)
                        menuAction.Textbox.Text = "EQUIP";
                    else
                        menuAction.Textbox.Text = "UNEQUIP";
                }
                else
                    menuAction.Textbox.Text = "USE";
            }
        }
    }

    public int AddItem(Item item)
    {
        var qty = item.QtyOnPickup;
        var i = 0;

        // Try to stack item.
        if (item.IsStackable())
        {
            foreach (var itemSlot in Items)
            {
                var maxStackSize = itemSlot.Item?.GetMaxStackSize();
                if (itemSlot.Item != null &&
                    itemSlot.Item.GetType() == item.GetType() &&
                    maxStackSize.HasValue)
                {
                    var remainingQtyInStack = maxStackSize.Value - itemSlot.Qty;
                    var qtyToAddToStack = Math.Min(qty, remainingQtyInStack);
                    qty -= qtyToAddToStack;
                    itemSlot.Qty += qtyToAddToStack;
                    ItemDirty[i] = true;
                    SyncedWithItemBox = false;
                }
                i++;
            }
        }

        // Put remaining qty in the first open slot.
        if (qty > 0)
        {
            i = 0;
            foreach (var itemSlot in Items)
            {
                if (itemSlot.Item == null)
                {
                    itemSlot.Item = item;
                    itemSlot.Qty = qty;
                    ItemDirty[i] = true;
                    SyncedWithItemBox = false;
                    qty = 0;
                    break;
                }
                i++;
            }
        }

        return qty;
    }

    void UsedItem()
    {
        if (Items[_currentItemIndex].Item.IsStackable())
        {
            Items[_currentItemIndex].Qty--;
            if (Items[_currentItemIndex].Qty <= 0)
                Items[_currentItemIndex].DiscardItem();
        }
        else
            Items[_currentItemIndex].DiscardItem();
        ItemDirty[_currentItemIndex] = true;
        SyncedWithItemBox = false;
    }

    public void RefreshItemUi()
    {
        for (var i = 0; i < ItemDirty.Length; i++)
            ItemDirty[i] = true;
        SyncedWithItemBox = false;
    }

    public void ToggleMenu()
    {
        _menuEnabled = !_menuEnabled;
        _playerStatus.MenuOpened = _menuEnabled;

        MenuPrefab.Visible = _menuEnabled;

        if (_menuEnabled)
            OnOpenMenu();
    }
}

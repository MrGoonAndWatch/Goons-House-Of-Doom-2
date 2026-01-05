using Godot;

public partial class PlayerItemBoxControl : Node3D
{
    [Export]
    private Control ItemBoxUi;
    [Export]
    public ItemSlot[] PlayerItems;
    [Export]
    public ItemSlot[] ItemBoxItems;
    [Export]
    private ColorRect InventoryCursor;
    [Export]
    private ColorRect ItemBoxCursor;
    [Export]
    private ScrollContainer ItemBoxScroll;

    private PlayerStatus _playerStatus;
    private PlayerInventory _playerInventory;
    private bool _inItemBox;

    private int _currentInventorySlot;
    private int _currentItemBoxSlot;

    private int _lastYPress;
    private int _lastXPress;

    private bool _itemBoxCursorDirty;

    public override void _Ready()
    {
        ItemBoxCursor.Visible = false;
        _playerStatus = PlayerStatus.GetInstance();
        _playerInventory = GetNode<PlayerInventory>(GameConstants.NodePaths.FromSceneRoot.PlayerInventory);
        ItemBoxScroll.GetVScrollBar().Modulate = GameConstants.Colors.Clear;
    }

    public override void _Process(double delta)
    {
        if (!_playerStatus.ItemBoxOpened)
            return;

        if (Input.IsActionJustPressed(GameConstants.Controls.aim.ToString()))
            HandleBackPressed();
        if (Input.IsActionJustPressed(GameConstants.Controls.confirm.ToString()))
            HandleConfirmPressed();

        HandleCursorMovement();
    }

    public void SyncInventory(ItemSlot[] items)
    {
        for(var i = 0; i < items.Length && i < PlayerItems.Length; i++)
            PlayerItems[i].CopyItemSlot(items[i]);
    }

    private void BackToPlayerInventory()
    {
        _inItemBox = false;
        ItemBoxCursor.Visible = false;
    }

    private void HandleBackPressed()
    {
        if (_inItemBox)
            BackToPlayerInventory();
        else
            CloseMenu();
    }

    private void HandleConfirmPressed()
    {
        if (_inItemBox)
        {
            var selectedInvSlot = PlayerItems[_currentInventorySlot];
            var selectedBoxSlot = ItemBoxItems[_currentItemBoxSlot];
            
            // If we just swapped the currently equipped weapon in to the item box, unequip it.
            if(_playerStatus.EquipedWeapon != null && selectedInvSlot.Item != null &&
               selectedInvSlot.Item.ItemId == _playerStatus.EquipedWeapon.ItemId)
            {
                _playerStatus.EquipWeapon(_playerStatus.EquipedWeapon);
                _playerInventory.EquipDirty = true;
            }

            // If the swapped items are the same type _and_ stackable, combine them, otherwise swap them.
            if (selectedInvSlot?.Item?.IsStackableWith(selectedBoxSlot?.Item) ?? false)
                ItemSlot.StackItemSlots(selectedInvSlot, selectedBoxSlot);
            else
                PlayerItems[_currentInventorySlot].SwapItemSlots(ItemBoxItems[_currentItemBoxSlot]);

            selectedInvSlot?.UpdateUi();
            selectedBoxSlot?.UpdateUi();
            BackToPlayerInventory();
            _playerInventory.SyncInventory(PlayerItems);
        }
        else
        {
            _inItemBox = true;
            ItemBoxCursor.Visible = true;
        }
    }

    private void HandleCursorMovement()
    {
        if (_itemBoxCursorDirty)
        {
            UpdateItemBoxCursor();
            _itemBoxCursorDirty = false;
        }

        (var inputDir, var _) = GameConstants.GetMovementVectorWithDeadzone();

        if (_inItemBox)
            HandleItemBoxMovement(inputDir.Y);
        else
            HandleInventoryMovement(inputDir.X, inputDir.Y);
    }

    private void HandleItemBoxMovement(float inputVal)
    {
        var oldItemBoxSlot = _currentItemBoxSlot;

        if (inputVal < 0 && _lastYPress >= 0)
        {
            _currentItemBoxSlot--;
            if (_currentItemBoxSlot < 0)
                _currentItemBoxSlot = ItemBoxItems.Length - 1;
            _lastYPress = -1;
        }
        else if (inputVal > 0 && _lastYPress <= 0)
        {
            _currentItemBoxSlot = (_currentItemBoxSlot + 1) % ItemBoxItems.Length;
            _lastYPress = 1;
        }
        
        if (inputVal == 0)
            _lastYPress = 0;

        if (oldItemBoxSlot != _currentItemBoxSlot)
        {
            UpdateItemBoxCursor();
            _itemBoxCursorDirty = true;
        }
    }

    private void HandleInventoryMovement(float horizontal, float vertical)
    {
        // Note: For some reason the Deadzone property in the project's InputMap wasn't being respected, leading to weird menu movement some of the time.
        if ((horizontal < 0 && horizontal > -GameConstants.ControllerMenuDeadzone) ||
            (horizontal > 0 && horizontal < GameConstants.ControllerMenuDeadzone))
            horizontal = 0;
        if ((vertical < 0 && vertical > -GameConstants.ControllerMenuDeadzone) ||
            (vertical > 0 && vertical < GameConstants.ControllerMenuDeadzone))
            vertical = 0;

        if (horizontal < 0 && _lastXPress >= 0)
        {
            _currentInventorySlot--;
            if (_currentInventorySlot < 0)
                _currentInventorySlot = PlayerItems.Length - 1;
            _lastXPress = -1;
        }
        else if (horizontal > 0 && _lastXPress <= 0)
        {
            _currentInventorySlot = (_currentInventorySlot + 1) % PlayerItems.Length;
            _lastXPress = 1;
        }
        else if (vertical < 0 && _lastYPress >= 0)
        {
            _currentInventorySlot -= 2;
            if (_currentInventorySlot < 0)
                _currentInventorySlot = PlayerItems.Length + _currentInventorySlot;
            _lastYPress = -1;
        }
        else if (vertical > 0 && _lastYPress <= 0)
        {
            _currentInventorySlot = (_currentInventorySlot + 2) % PlayerItems.Length;
            _lastYPress = 1;
        }

        if (horizontal == 0)
            _lastXPress = 0;
        if (vertical == 0)
            _lastYPress = 0;

        UpdateInventoryCursor();
    }

    private void UpdateItemBoxCursor()
    {
        // HACK: To get scrolling to be smoother towards the top, center on the lesser of 5 slots earlier or pos 0 if that's negative.
        var targetScrollIndex = Mathf.Max(_currentItemBoxSlot - 5, 0);
        ItemBoxScroll.ScrollVertical = targetScrollIndex * 100;

        ItemBoxCursor.GlobalPosition = ItemBoxItems[_currentItemBoxSlot].GlobalPosition;
    }

    private void UpdateInventoryCursor()
    {
        InventoryCursor.Position = PlayerItems[_currentInventorySlot].Position;
    }

    public void OpenMenu()
    {
        _currentInventorySlot = 0;
        UpdateInventoryCursor();
        ItemBoxUi.Visible = true;
        _playerStatus.ItemBoxOpened = true;
    }

    private void CloseMenu()
    {
        ItemBoxUi.Visible = false;
        _playerStatus.ItemBoxOpened = false;
    }
}

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

    public override void _Ready()
    {
        ItemBoxCursor.Visible = false;
        _playerStatus = PlayerStatus.GetInstance();
        _playerInventory = GetNode<PlayerInventory>(GameConstants.NodePaths.FromSceneRoot.PlayerInventory);
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
            // If we just swapped the currently equipped weapon in to the item box, unequip it.
            if(_playerStatus.EquipedWeapon != null && ItemBoxItems[_currentItemBoxSlot].Item != null && 
                ItemBoxItems[_currentItemBoxSlot].Item.GetInstanceId() == _playerStatus.EquipedWeapon.GetInstanceId())
            {
                _playerStatus.EquipWeapon(_playerStatus.EquipedWeapon);
                _playerInventory.EquipDirty = true;
            }

            PlayerItems[_currentInventorySlot].SwapItemSlots(ItemBoxItems[_currentItemBoxSlot]);
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
        var inputDir = Input.GetVector(GameConstants.Controls.left.ToString(), GameConstants.Controls.right.ToString(), GameConstants.Controls.up.ToString(), GameConstants.Controls.down.ToString());

        if (_inItemBox)
            HandleItemBoxMovement(inputDir.Y);
        else
            HandleInventoryMovement(inputDir.X, inputDir.Y);
    }

    private void HandleItemBoxMovement(float inputVal)
    {
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

        UpdateItemBoxCursor();
    }

    private void HandleInventoryMovement(float xVal, float yVal)
    {
        if (xVal < 0 && _lastXPress >= 0)
        {
            _currentInventorySlot--;
            if (_currentInventorySlot < 0)
                _currentInventorySlot = PlayerItems.Length - 1;
            _lastXPress = -1;
        }
        else if (xVal > 0 && _lastXPress <= 0)
        {
            _currentInventorySlot = (_currentInventorySlot + 1) % PlayerItems.Length;
            _lastXPress = 1;
        }
        else if (yVal < 0 && _lastYPress >= 0)
        {
            _currentInventorySlot -= 2;
            if (_currentInventorySlot < 0)
                _currentInventorySlot = PlayerItems.Length + _currentInventorySlot;
            _lastYPress = -1;
        }
        else if (yVal > 0 && _lastYPress <= 0)
        {
            _currentInventorySlot = (_currentInventorySlot + 2) % PlayerItems.Length;
            _lastYPress = 1;
        }

        if (xVal == 0)
            _lastXPress = 0;
        if (yVal == 0)
            _lastYPress = 0;

        UpdateInventoryCursor();
    }

    private void UpdateItemBoxCursor()
    {
        ItemBoxCursor.GlobalPosition = ItemBoxItems[_currentItemBoxSlot].GlobalPosition;
        // TODO: THIS IS BUSTED!!! Need to focus on currently selected slot!
        //ItemBoxItems[_currentItemBoxSlot].GrabFocus();
        //ItemBoxScroll.ScrollVertical = (int)(ItemBoxItems[_currentItemBoxSlot].GlobalPosition.Y);
    }

    private void UpdateInventoryCursor()
    {
        InventoryCursor.Position = PlayerItems[_currentInventorySlot].Position;
    }

    public void OpenMenu()
    {
        ItemBoxUi.Visible = true;
        _playerStatus.ItemBoxOpened = true;
    }

    private void CloseMenu()
    {
        ItemBoxUi.Visible = false;
        _playerStatus.ItemBoxOpened = false;
    }
}

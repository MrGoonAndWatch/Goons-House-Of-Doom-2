using Godot;

public partial class InventoryStatusUi : StatusScreenTab
{
    [Export]
    private PlayerInventory _playerInventory;
    
    [Export]
    private SubViewportContainer _subViewportContainer;

    [Export]
    private Camera3D _examineCamera;

    [Export]
    private Node _examineObjectContainer;

    [Export]
    private Light3D _examineObjectLight;

    [Export]
    private float _runSpeedMod = 3.0f;

    [Export]
    private float _rotationSpeed = 1.5f;

    private Node3D _generatedExamineObject;
    private bool _examiningItem;
    
    public override void _Ready()
    {
        _subViewportContainer.Visible = false;
        _examineObjectLight.Visible = false;
        _examineCamera.Visible = false;
        _examineCamera.Position = new  Vector3(0, 0, 2);
        _examiningItem = false;
    }

    public void ToggleOn(string targetItemScene)
    {
        _generatedExamineObject = (Node3D) GD.Load<PackedScene>(targetItemScene).Instantiate();
        _examineObjectContainer.AddChild(_generatedExamineObject);
        _examineObjectLight.Visible = true;
        _examineCamera.Visible = true;
        _subViewportContainer.Visible = true;
        _examiningItem = true;
    }

    public void ToggleOff()
    {
        _examineObjectContainer.RemoveChild(_generatedExamineObject);
        _generatedExamineObject.QueueFree();
        _generatedExamineObject = null;
        _subViewportContainer.Visible = false;
        _examineCamera.Visible = false;
        _examineObjectLight.Visible = false;
        _examiningItem = false;
    }

    public override void _Process(double delta)
    {
        ProcessRotation(delta);
    }

    private void ProcessRotation(double delta)
    {
        if (!_examiningItem) return;

        var (movement, _) = GameConstants.GetMovementVectorWithDeadzone();
        var running = Input.IsActionPressed(GameConstants.Controls.run.ToString());
        var runMod = running ? _runSpeedMod : 1.0f;
        
        if (movement.X != 0)
            _generatedExamineObject.RotateX((float) (movement.X * delta * _rotationSpeed * runMod));
        if (movement.Y != 0)
            _generatedExamineObject.RotateY((float) (movement.Y * delta * _rotationSpeed * runMod));
    }

    public override void OnOpenMenu()
    {
        _playerInventory.OnOpenMenu();
    }

    public void ToggleMenu()
    {
        StatusScreenHeader.ToggleMenu();
    }

    public void ReturnFocus()
    {
        StatusScreenHeader.ReturnFocus();
    }
}

using Godot;
using System.Collections.Generic;

public partial class ActorLookAt : Area3D
{
    [Export]
    private LookAtModifier3D _headLookAtModifier;

    private int _targetCount;

    [Export]
    private Vector2 _maxLookAtDistance = new Vector2(5, 4);
    [Export]
    private CollisionShape3D _collisionShape;

    private List<Node3D> _targetNodes;

    [Export]
    private float _lookSpeed = 2.5f;
    [Export]
    private double _timeBetweenLookAtUpdates = 1.0;
    private double _timeSinceLastLookAtUpdate;

    private Vector3 _initialTargetLocalPosition;
    private Node3D _lookAtTarget;
    private Tween _currentLookAtTween;

    public override void _Ready()
    {
        _currentLookAtTween = null;
        _targetNodes = new List<Node3D>();

        UpdateLookAtColliderFromParams();

        _lookAtTarget = _headLookAtModifier.GetNode<Node3D>(_headLookAtModifier.TargetNode);
        _initialTargetLocalPosition = _lookAtTarget.Transform.Origin;
    }

    private void UpdateLookAtColliderFromParams()
    {
        if (_collisionShape?.Shape is CylinderShape3D)
        {
            (_collisionShape.Shape as CylinderShape3D).Radius = _maxLookAtDistance.X;
            (_collisionShape.Shape as CylinderShape3D).Height = _maxLookAtDistance.Y;
        }
        else
            GD.Print($"[color=yellow]{nameof(ActorLookAt)} script did not have a collision shape parameter specified or it was not a CyclinderShape3D. Using default shape values for look at range calculations.");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (TimeToUpdateLookAt(delta))
            UpdateLookAt();
    }

    private bool TimeToUpdateLookAt(double delta)
    {
        _timeSinceLastLookAtUpdate += delta;
        if (_timeSinceLastLookAtUpdate > _timeBetweenLookAtUpdates)
        {
            _timeSinceLastLookAtUpdate = 0;
            return true;
        }
        return false;
    }

    private void UpdateLookAt()
    {
        // TODO: Additional consideration for resetting to default position when target is out of player's cone of rotation!
        if (_targetCount > 0 && _lookAtTarget.GlobalTransform.Origin != _targetNodes[0].GlobalTransform.Origin)
        {
            CreateNewLookAtTween(_targetNodes[0].GlobalTransform.Origin, true);
            //GD.Print($"Set player look at to {_targetNodes[0].GlobalTransform.Origin}!");
        }
        // TODO: even with equal approx this never actually reaches the position, we'll need to figure out some other snap to logic at some point!
        else //if (!_lookAtTarget.Transform.Origin.IsEqualApprox(_initialTargetLocalPosition))
        {
            CreateNewLookAtTween(_initialTargetLocalPosition, false);
            //GD.Print($"Reset player look at to {_initialTargetLocalPosition}! _lookAtTarget.Transform.Origin={_lookAtTarget.Transform.Origin} _initialTargetLocalPosition={_initialTargetLocalPosition}");
        }
    }

    private void CreateNewLookAtTween(Vector3 endPosition, bool isEndInGlobalCoords)
    {
        if (_currentLookAtTween != null)
            _currentLookAtTween.Kill();

        _currentLookAtTween = CreateTween();
        var positionPropertyName = isEndInGlobalCoords ? "global_position" : "position";
        _currentLookAtTween.TweenProperty(_lookAtTarget, positionPropertyName, endPosition, _lookSpeed);
        _currentLookAtTween.SetTrans(Tween.TransitionType.Linear);
    }

    public void _OnAreaEntered(Area3D other)
    {
        Node3D target = null;
        bool foundValidTarget = false;

        if (other is Item)
        {
            target = (other as Item).LookAtTargetPoint;
            foundValidTarget = true;
        }
        else if (other is MapPickup)
        {
            target = (other as MapPickup).LookAtTargetPoint;
            foundValidTarget = true;
        }
        else if (other is NotePickup)
        {
            target = (other as NotePickup).LookAtTargetPoint;
            foundValidTarget = true;
        }

        if (target != null)
        {
            _targetNodes.Add(target);
            _targetCount++;
            //GD.Print($"Added lookAt target {target.GetParent().Name}");
        }
        else if (foundValidTarget)
            GD.PrintErr($"Found LookAt object '{other.Name}' but it had a null LookAtTargetPoint. Check this prefab ({other.GetParent().Name})!!!");
    }

    public void _OnAreaExited(Area3D other)
    {
        Node3D target = null;
        if (other is Item)
            target = (other as Item).LookAtTargetPoint;
        else if (other is MapPickup)
            target = (other as MapPickup).LookAtTargetPoint;
        else if (other is NotePickup)
            target = (other as NotePickup).LookAtTargetPoint;

        if (target == null)
            return;

        for (var i = 0; i < _targetNodes.Count; i++)
        {
            if (_targetNodes[i].GetInstanceId() == target?.GetInstanceId())
            {
                //GD.Print($"Removed lookAt target {_targetNodes[i].GetParent().Name}");
                _targetCount--;
                _targetNodes.RemoveAt(i);
                break;
            }
        }
    }
}

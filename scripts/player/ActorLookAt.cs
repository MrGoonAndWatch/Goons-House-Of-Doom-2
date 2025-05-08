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
    private Vector3? _targetRotation = null;

    [Export]
    private float _lookSpeed = 100.0f;
    [Export]
    private double _timeBetweenLookAtUpdates = 1.0;
    private float _minLookSpeed = 0.001f;
    private double _timeSinceLastLookAtUpdate;

    private Transform3D _headBoneTransform;

    public override void _Ready()
    {
        _targetNodes = new List<Node3D>();
        _targetRotation = Vector3.Zero;

        if (_collisionShape?.Shape is CylinderShape3D)
        {
            (_collisionShape.Shape as CylinderShape3D).Radius = _maxLookAtDistance.X;
            (_collisionShape.Shape as CylinderShape3D).Height = _maxLookAtDistance.Y;
        }
        else
            GD.Print("[color=yellow]ActorLookAt script did not have a collision shape parameter specified or it was not a CyclinderShape3D. Using default shape values for look at range calculations.");

        var playerSkeleton = _headLookAtModifier.GetNode<Skeleton3D>(_headLookAtModifier.TargetNode);
        _headBoneTransform = playerSkeleton.GetBonePose(_headLookAtModifier.Bone);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (TimeToUpdateLookAt(delta))
            UpdateLookAt();
        LookAtTarget(delta);
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
        if (_targetCount > 0)
        {
            // TODO: Calculate angle between player head bone's Up vector and vector from head bone to object.
            var lookFromPos = _headLookAtModifier.Transform.Origin;
            var lookToPos = _targetNodes[0].Transform.Origin;

            var x = Vector3.Up;
        }
        else
        {
            _targetRotation = Vector3.Zero;
        }
    }

    private void LookAtTarget(double delta)
    {
        if (_targetCount == 0 && _headLookAtModifier.OriginOffset == Vector3.Zero) return;

        var currentRotation = _headLookAtModifier.OriginOffset;
        var lookSpeed = (float)Mathf.Max(_lookSpeed * delta, _minLookSpeed);
        
        var rotateAmount = currentRotation.Slerp(_targetRotation.Value, lookSpeed);
        if (rotateAmount.IsEqualApprox(_targetRotation.Value))
            _headLookAtModifier.OriginOffset = _targetRotation.Value;
        else
            _headLookAtModifier.OriginOffset = rotateAmount;

        // Uncomment for big head mode :)
        //_characterSkeleton.SetBonePoseScale(_boneIndex, new Vector3(2, 2, 2));
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

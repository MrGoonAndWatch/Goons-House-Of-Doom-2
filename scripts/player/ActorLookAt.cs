using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ActorLookAt : Node3D
{
    [Export]
    private Skeleton3D _characterSkeleton;
    [Export]
    private float _maxLookAtDistance = 5;

    private Transform3D _bone;
    private bool _initialized;
    private int _boneIndex;
    private List<Transform3D> _targetNodes;
    private Transform3D? _currentTarget = null;
    private Quaternion? _targetRotation = null;
    private Quaternion _originalRotation;

    [Export]
    private float _lookSpeed = 100.0f;
    private float _minLookSpeed = 0.001f;
    private const double _timeBetweenLookAtUpdates = 1.0;
    private double _timeSinceLastLookAtUpdate;

    private bool _isHeadMoving;

    public override void _PhysicsProcess(double delta)
    {
        if (_initialized)
        {
            if (TimeToUpdateLookAt(delta))
                UpdateLookAt();
            LookAtTarget(delta);
        }
        else
            Initialize();
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
        float? closestDistance = null;
        Transform3D? closestNode = null;
        foreach (var node in _targetNodes)
        {
            var targetNodePos = node.Origin;
            var currentPos = GlobalTransform.Origin;
            var currentDistance = currentPos.DistanceTo(targetNodePos);
            if (currentDistance <= _maxLookAtDistance && (closestDistance == null || closestDistance > currentDistance))
            {
                //GD.Print($"Found thing to look at in UpdateLookAt! ({currentDistance}) {node}");
                closestDistance = currentDistance;
                closestNode = node;
            }
        }

        //if ((closestNode.HasValue && closestNode.Value != _currentTarget) || (closestNode == null && _currentTarget != null))
        {
            _isHeadMoving = true;
            _currentTarget = closestNode;
            Vector3? vecToTarget = null;
            if (closestNode != null)
            {
                vecToTarget = (closestNode.Value.Origin - GlobalTransform.Origin).Normalized();
                _targetRotation = new Quaternion(GlobalTransform.Basis.Z.Normalized(), vecToTarget.Value).Normalized();
            }
            //GD.Print($"Set new target look at: _currentTarget={_currentTarget}\r\n\r\n vecToTarget={vecToTarget}\r\n\r\n _targetRotation={_targetRotation}");
        }
    }

    private void LookAtTarget(double delta)
    {
        if (!_isHeadMoving) return;

        var currentRotation = _characterSkeleton.GetBonePoseRotation(_boneIndex).Normalized();
        var lookSpeed = (float) Mathf.Max(_lookSpeed * delta, _minLookSpeed);
        if (_targetRotation == null)
        {
            var rotateAmount = currentRotation.Slerp(_originalRotation, lookSpeed);
            if (rotateAmount.IsEqualApprox(_originalRotation))
            {
                _characterSkeleton.SetBonePoseRotation(_boneIndex, _originalRotation);
                _isHeadMoving = false;
            }
            else
                _characterSkeleton.SetBonePoseRotation(_boneIndex, rotateAmount);
        }
        else
        {
            var rotateAmount = currentRotation.Slerp(_targetRotation.Value, lookSpeed);
            if (rotateAmount.IsEqualApprox(_targetRotation.Value))
            {
                _characterSkeleton.SetBonePoseRotation(_boneIndex, _targetRotation.Value);
                _isHeadMoving = false;
            }
            else
                _characterSkeleton.SetBonePoseRotation(_boneIndex, rotateAmount);
        }

        //var t = _characterSkeleton.GetBonePoseRotation(_boneIndex);
        //if (Input.IsActionPressed("ui_up"))
        //{
        //    t = t.Slerp(new Quaternion(new Vector3(1, 0, 0), 200), 0.1f);
        //    _characterSkeleton.SetBonePoseRotation(_boneIndex, t);
        //}
        //else if (Input.IsActionPressed("ui_down"))
        //{
        //    t = t.Slerp(new Quaternion(new Vector3(1, 0, 0), -200), 0.1f);
        //    _characterSkeleton.SetBonePoseRotation(_boneIndex, t);
        //}

        // Uncomment for big head mode :)
        //_characterSkeleton.SetBonePoseScale(_boneIndex, new Vector3(2, 2, 2));
    }

    private void Initialize()
    {
        _boneIndex = _characterSkeleton.FindBone("Head");
        _bone = _characterSkeleton.GetBoneGlobalPose(_boneIndex);
        var currentRotation = _characterSkeleton.GetBonePoseRotation(_boneIndex);
        _originalRotation = new Quaternion(currentRotation.X, currentRotation.Y, currentRotation.Z, currentRotation.W).Normalized();

        GD.Print($"Initializing ActorLookAt. _boneIndex={_boneIndex}, _bone={_bone}");
        
        RefreshTargetNodes();

        _initialized = true;
    }

    public void RefreshTargetNodes()
    {
        var targetNodes = new List<Transform3D>();
        var root = GetNode(GameConstants.NodePaths.FromSceneRoot.SceneRoot);
        PopulateTargetNodes(root, GetInstanceId(), targetNodes);
        _targetNodes = targetNodes;
    }

    private static void PopulateTargetNodes(Node node, ulong? ignoreNodeId, List<Transform3D> targetNodes)
    {
        // TODO: With this code other actors will never be able to find the player to look at because their root is ignored.
        //          ALSO REMOVING THIS WILL CAUSE JANK WITH LOOKING AT ITEMS THAT ARE IN THE INVENTORY UI.
        if (GameConstants.RoomClearCheckBlacklist.Contains(node.GetPath().GetConcatenatedNames()))
            return;
        if ((ignoreNodeId == null || node.GetInstanceId() != ignoreNodeId.Value) && IsLookAtTarget(node))
            targetNodes.Add((node as Node3D).GlobalTransform);
        var children = node.GetChildren();
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            PopulateTargetNodes(child, ignoreNodeId, targetNodes);
        }
    }

    private static bool IsLookAtTarget(Node node)
    {
        if (!(node is Node3D))
            return false;

        // TODO: Eventually determine which types to check here dynamically!
        return node is Enemy || node is Item || node is MapPickup || node is NotePickup;
    }
}

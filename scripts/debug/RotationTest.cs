using Godot;
using System;
using static GameConstants;

public partial class RotationTest : Node
{
    [Export]
    private Player _player;

    [Export]
    private Node3D _targetA;
    [Export]
    private Node3D _targetB;

    private bool _isRotating;

    public override void _Process(double delta)
    {
        if (!_isRotating && Input.IsActionJustPressed(Controls.aim.ToString()))
            StartRotatingTowardsTargetA();
        else if (!_isRotating && Input.IsActionJustPressed(Controls.confirm.ToString()))
            InstantlyRotateToTargetB();
    }

    private void StartRotatingTowardsTargetA()
    {
        GD.Print("StartRotatingTowardsTargetA");
        _isRotating = true;

        var targetFull = _targetA.Position - _player.Position;
        var target = new Vector2(targetFull.X, targetFull.Z).Normalized();
        var forward = new Vector2(_player.Basis.Z.X, _player.Basis.Z.Z);

        GD.Print($"playerBasis={_player.Basis}");
        var targetAngle = _player.Rotation.Y - (forward.AngleTo(target) + (Mathf.Pi / 2));
        GD.Print($"forward={forward}, target={target}, _player.Rotation.Y {_player.Rotation.Y}, targetAngle={targetAngle}");
        _player.DebugTweenToAngle(targetAngle, EndRotateToTargetPosition);
    }

    private void EndRotateToTargetPosition()
    {
        _isRotating = false;
    }

    private void InstantlyRotateToTargetB()
    {
        _player.LookAt(_targetB.Position);
        _player.Rotate(Vector3.Up, Mathf.Pi / 2);
    }
}

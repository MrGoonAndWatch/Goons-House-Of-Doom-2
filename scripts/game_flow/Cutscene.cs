using Godot;
using Godot.Collections;
using System;

public partial class Cutscene : Node
{
    // Note: if defining ICutsceneActor references per-instruction is bad for performance we could change those params to ints and use a single array of actors which we reference the indexes of here.
    //[Export]
    //private ICutsceneActor[] Actors;
    [Export]
    private int CutsceneEventId;
    [Export]
    private CutsceneInstruction[] Instructions;

    private bool _initialized;
    private int _currentInstructionIndex;
    private float _currentInstructionTimeRemaining;
    private bool _isCurrentActorMoving;
    private bool _skipped;

    private Dictionary<int, AudioStream> _voiceLines;

    public void StartCutscene()
    {
        if(Instructions == null || Instructions.Length == 0)
        {
            GD.PrintErr($"Failed to play cutscene '{Name}', no Instructions were set!");
            _initialized = true;
            _currentInstructionIndex = 1;
            return;
        }

        _voiceLines = LoadVoiceLines();

        _currentInstructionIndex = -1;
        NextInstruction();
        _initialized = true;
    }

    private Dictionary<int, AudioStream> LoadVoiceLines()
    {
        var voiceLines = new Dictionary<int, AudioStream>();

        for (var i = 0; i < Instructions.Length; i++)
        {
            var instruction = Instructions[i];
            if (!string.IsNullOrEmpty(instruction.VoiceLineFile))
            {
                var voiceLine = GhodAudioManager.LoadVoiceClip(instruction.VoiceLineFile);
                if (voiceLine != null)
                    voiceLines.Add(i, voiceLine);
            }
        }

        return voiceLines;
    }

    public void NextInstruction()
    {
        if (_currentInstructionIndex >= 0)
        {
            var currentInstruciton = Instructions[_currentInstructionIndex];
            if (!string.IsNullOrEmpty(currentInstruciton.AnimationFlag))
                currentInstruciton.TargetActor.SetAnimationFlag(currentInstruciton.AnimationFlag, false);
        }

        _currentInstructionIndex++;
        if (_currentInstructionIndex >= Instructions.Length)
        {
            _currentInstructionTimeRemaining = 0;
            _isCurrentActorMoving = false;
            return;
        }

        var nextInstruction = Instructions[_currentInstructionIndex];
        if (nextInstruction.EndType == GameConstants.CutsceneInstructionEndType.EndAfterTime)
            _currentInstructionTimeRemaining = nextInstruction.EndTimer;
        else
            _currentInstructionTimeRemaining = 0;
        if (!nextInstruction.MoveToPosition.Equals(Vector3.Zero))
        {
            nextInstruction.TargetActor.SetMoveToPosition(nextInstruction.MoveToPosition, nextInstruction.MoveSpeed);
            _isCurrentActorMoving = true;
        }
        else
            _isCurrentActorMoving = false;
        if (!string.IsNullOrEmpty(nextInstruction.AnimationFlag))
            nextInstruction.TargetActor.SetAnimationFlag(nextInstruction.AnimationFlag, true);
        if (_voiceLines.ContainsKey(_currentInstructionIndex))
            GhodAudioManager.PlayVoiceClip(_voiceLines[_currentInstructionIndex]);
    }

    public override void _Process(double delta)
    {
        if (!_initialized || _skipped)
            return;

        if (_currentInstructionTimeRemaining > 0)
        {
            _currentInstructionTimeRemaining -= (float) delta;
            if (_currentInstructionTimeRemaining <= 0 && Instructions[_currentInstructionIndex].EndType == GameConstants.CutsceneInstructionEndType.EndAfterTime)
                NextInstruction();
        }

        if (_isCurrentActorMoving)
        {
            var reachedDestination = Instructions[_currentInstructionIndex].TargetActor.MoveTowardsPosition(delta);
            if (reachedDestination && Instructions[_currentInstructionIndex].EndType == GameConstants.CutsceneInstructionEndType.EndWhenMovementEnds)
                NextInstruction();
            else if (reachedDestination)
                _isCurrentActorMoving = false;
        }
    }

    public bool HasCutsceneEnded()
    {
        return _initialized && _currentInstructionIndex >= (Instructions?.Length ?? 0);
    }

    public void SkipCutscene()
    {
        if (_skipped)
            return;

        _skipped = true;

        for (; _currentInstructionIndex < Instructions.Length; _currentInstructionIndex++)
        {
            var currentInstruction = Instructions[_currentInstructionIndex];
            if (currentInstruction.TargetActor != null && !currentInstruction.MoveToPosition.Equals(Vector3.Zero))
                currentInstruction.TargetActor.MoveToPositionInstantly(currentInstruction.MoveToPosition);
        }

        GD.Print("Cutscene skipped!");
    }
}

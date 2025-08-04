using System.Collections.Generic;
using Godot;

public partial class Cutscene : Node
{
    // Note: if defining ICutsceneActor references per-instruction is bad for performance we could change those params to ints and use a single array of actors which we reference the indexes of here.
    //[Export]
    //private ICutsceneActor[] Actors;
    [Export]
    public int CutsceneId;
    [Export]
    private CutsceneInstruction[] Instructions;
    [Export]
    private CutsceneSubtitleInstruction[] SubtitleLines;
    [Export(hintString: "When the cutscene ends, set the camera to whatever camera the player most recently passed through during the cutscene if this is checked, otherwise leave the camera at the ending angle when the cutscene ends.")]
    public bool ResetCameraOnCutsceneEnd = true;
    /// <summary>
    /// Unique identifier for this cutscene.
    /// </summary>

    private bool _initialized;
    private int _currentInstructionIndex;
    private float _currentInstructionTimeRemaining;
    private bool _isCurrentActorMoving;
    private bool _skipped;
    private bool _isCutscenePaused;
    private bool _moveToNextInstruction;

    private Godot.Collections.Dictionary<int, AudioStream> _voiceLines;
    private Queue<SubtitleLine> _currentSubtitleLines;
    private float _timeUntilNextSubtitleLine;

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

    private Godot.Collections.Dictionary<int, AudioStream> LoadVoiceLines()
    {
        var voiceLines = new Godot.Collections.Dictionary<int, AudioStream>();

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
        FlipActiveAnimationFlagsOff();

        if (IncrementToNextCutsceneInstruction())
            return;

        ProcessNextSubtitleInstruction();
        ProcessNextInstruction();
    }

    private void FlipActiveAnimationFlagsOff()
    {
        if (_currentInstructionIndex >= 0)
        {
            var currentInstruction = Instructions[_currentInstructionIndex];
            if (!string.IsNullOrEmpty(currentInstruction.AnimationFlag))
                currentInstruction.TargetActor.SetAnimationFlag(currentInstruction.AnimationFlag, false);
        }
    }

    private bool IncrementToNextCutsceneInstruction()
    {
        var isEndOfCutscene = false;
        _currentInstructionIndex++;
        if (_currentInstructionIndex >= Instructions.Length)
        {
            _currentInstructionTimeRemaining = 0;
            _isCurrentActorMoving = false;
            isEndOfCutscene = true;
        }
        return isEndOfCutscene;
    }

    private void ProcessNextSubtitleInstruction()
    {
        if (_currentInstructionIndex < 0 || 
            _currentInstructionIndex >= (SubtitleLines?.Length ?? 0) || 
            (SubtitleLines?[_currentInstructionIndex]?.SubtitleLines?.Length ?? 0) <= 0)
            return;
        
        var subtitleInstruction = SubtitleLines[_currentInstructionIndex];
        _currentSubtitleLines = new Queue<SubtitleLine>();
        for (var i = 0; i < subtitleInstruction.SubtitleLines.Length; i++)
            _currentSubtitleLines.Enqueue(subtitleInstruction.SubtitleLines[i]);
        ShowNextSubtitleLine();
    }

    private void ShowNextSubtitleLine()
    {
        if (_currentSubtitleLines?.TryDequeue(out var nextSubtitleLine) ?? false)
        {
            SubtitleDisplay.DisplaySubtitles(nextSubtitleLine);
            _timeUntilNextSubtitleLine = nextSubtitleLine.SubtitleDisplayTimeInSeconds;
        }
        else
            SubtitleDisplay.HideSubtitles();
    }
    
    private void ProcessNextInstruction()
    {
        var nextInstruction = Instructions[_currentInstructionIndex];
        //GD.Print($"nextInstruction; nextInstruction.Name={nextInstruction.Name}, index={_currentInstructionIndex}, type={nextInstruction.InstructionType}");
        switch (nextInstruction.InstructionType)
        {
            case GameConstants.CutsceneInstructionType.InGameInstruction:
                HandleInGameCutsceneInstruction(nextInstruction);
                break;
            case GameConstants.CutsceneInstructionType.FmvCutscene:
                HandleFmvCutsceneInstruction(nextInstruction);
                break;
            case GameConstants.CutsceneInstructionType.ChangeCamera:
                HandleChangeCameraInstruction(nextInstruction);
                break;
        }
    }

    private void HandleInGameCutsceneInstruction(CutsceneInstruction nextInstruction)
    {
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
        if (_voiceLines?.ContainsKey(_currentInstructionIndex) ?? false)
            GhodAudioManager.PlayVoiceClip(_voiceLines[_currentInstructionIndex]);
    }

    private void HandleFmvCutsceneInstruction(CutsceneInstruction nextInstruction)
    {
        if(nextInstruction.FmvStream == null)
        {
            GD.PrintErr("Cutscene instruction was type 'FmvVideo' but no video parameter was provided in the 'FmvStream' field. Skipping video!");
            NextInstruction();
            return;
        }

        nextInstruction.EndType = GameConstants.CutsceneInstructionEndType.EndWhenVideoEnds;
        _currentInstructionTimeRemaining = 0;

        var fmvManager = GetNode<FmvManager>(GameConstants.NodePaths.FromSceneRoot.FmvPlayer);
        fmvManager.PlayVideo(nextInstruction.FmvStream, NextInstruction);
    }
    
    private void HandleChangeCameraInstruction(CutsceneInstruction nextInstruction)
    {
        if (nextInstruction.NewCameraTransform == null)
        {
            GD.PrintErr("ChangeCamera instruction found in Cutscene but no NewCameraTransform was specified! Ignoring instruction!");
            _moveToNextInstruction = true;
            return;
        }
        
        // TODO: Consider adding camera as param to the Cutscene object so we don't need to do this jank GetNode call!!!
        var camera = GetNode<Camera3D>(GameConstants.NodePaths.FromSceneRoot.Camera);
        camera.GlobalPosition = nextInstruction.NewCameraTransform.GlobalPosition;
        camera.GlobalRotation = nextInstruction.NewCameraTransform.GlobalRotation;

        _moveToNextInstruction = true;
    }

    public override void _Process(double delta)
    {
        if (!_initialized || _skipped || _isCutscenePaused)
            return;

        if (_timeUntilNextSubtitleLine > 0)
            ProcessSubtitleTiming(delta);
        
        if (CurrentlyWatchingFmv())
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
            if (reachedDestination)
                _isCurrentActorMoving = false;
            if (reachedDestination && Instructions[_currentInstructionIndex].EndType == GameConstants.CutsceneInstructionEndType.EndWhenMovementEnds)
                NextInstruction();
        }

        if (_moveToNextInstruction)
        {
            _moveToNextInstruction = false;
            NextInstruction();
        }
    }

    private void ProcessSubtitleTiming(double delta)
    {
        _timeUntilNextSubtitleLine -= (float) delta;

        if (_timeUntilNextSubtitleLine <= 0)
            ShowNextSubtitleLine();
    }

    public bool HasCutsceneEnded()
    {
        return _initialized && _currentInstructionIndex >= (Instructions?.Length ?? 0);
    }

    public void ToggleCutscenePause()
    {
        if (CurrentlyWatchingFmv())
        {
            var fmvPlayer = GetNode<FmvManager>(GameConstants.NodePaths.FromSceneRoot.FmvPlayer);
            _isCutscenePaused = fmvPlayer.PlayPause();
        }
        else
        {
            _isCutscenePaused = !_isCutscenePaused;
            if (Instructions[_currentInstructionIndex]?.TargetActor != null)
                Instructions[_currentInstructionIndex].TargetActor.SetCutscenePaused(_isCutscenePaused);
        }
    }

    public bool SkipCutscene()
    {
        _isCutscenePaused = false;

        if (_skipped)
            return false;

        if (CurrentlyWatchingFmv())
        {
            var fmvPlayer = GetNode<FmvManager>(GameConstants.NodePaths.FromSceneRoot.FmvPlayer);
            fmvPlayer.SkipVideo();
            return false;
        }

        for (; _currentInstructionIndex < Instructions.Length; _currentInstructionIndex++)
        {
            var currentInstruction = Instructions[_currentInstructionIndex];
            //GD.Print($"SkipCutscene {_currentInstructionIndex+1} of {Instructions.Length} ({currentInstruction.Name})");
            
            if (CurrentlyWatchingFmv())
            {
                _currentInstructionIndex--;
                NextInstruction();
                return false;
            }
            if (currentInstruction.TargetActor != null && !currentInstruction.MoveToPosition.Equals(Vector3.Zero))
                currentInstruction.TargetActor.MoveToPositionInstantly(currentInstruction.MoveToPosition);
            if (currentInstruction.NewCameraTransform != null)
                HandleChangeCameraInstruction(currentInstruction);
        }

        GD.Print("Cutscene skipped!");
        _moveToNextInstruction = false;
        _currentSubtitleLines = null;
        SubtitleDisplay.HideSubtitles();
        _skipped = true;
        return true;
    }

    private bool CurrentlyWatchingFmv()
    {
        if (Instructions == null || _currentInstructionIndex >= Instructions.Length) return false;

        return Instructions[_currentInstructionIndex].InstructionType == GameConstants.CutsceneInstructionType.FmvCutscene;
    }
}

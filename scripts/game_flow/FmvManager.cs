using Godot;
using System;

public partial class FmvManager : Control
{
    [Export]
    private VideoStreamPlayer _fmvPlayer;

    private Action _callbackOnVideoEnd;

    public override void _Ready()
    {
        Visible = false;
    }

    public void PlayVideo(VideoStream video, Action callbackOnVideoEnd)
    {
        if (video == null || callbackOnVideoEnd == null) return;

        _callbackOnVideoEnd = callbackOnVideoEnd;
        _fmvPlayer.Stream = video;
        Visible = true;
        _fmvPlayer.Play();
    }

    // TODO: Implement Pause/Unpause? Currently the skip button is the same as the in game pause button so need to remap that!

    public void SkipVideo()
    {
        if (_fmvPlayer?.IsPlaying() ?? false)
            _fmvPlayer.Stop();

        FinishVideo();
    }

    private void FinishVideo()
    {
        _fmvPlayer.Stream = null;
        Visible = false;

        if (_callbackOnVideoEnd == null)
            GD.PrintErr("No callback provided for current FMV playing, cannot signal to CutsceneManager that video is over!");
        else
            _callbackOnVideoEnd();
    }

    public void _OnVideoFinished()
    {
        FinishVideo();
    }
}

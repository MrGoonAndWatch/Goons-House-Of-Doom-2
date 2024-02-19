using Godot;
using System;

public partial class OptionsMenuUi : Control
{
    [Export]
    private Slider TotalVolumeSlider;
    [Export]
    private Slider MusicVolumeSlider;
    [Export]
    private Slider SfxVolumeSlider;
    [Export]
    private Slider VoiceVolumeSlider;

    private float _originalTotalVolume;
    private float _originalMusicVolume;
    private float _originalSfxVolume;
    private float _originalVoiceVolume;

    public override void _Ready()
    {
        SaveCurrentValues();
        //GhodAudioManager.ChangeTotalVolume(_originalTotalVolume);
        //GhodAudioManager.ChangeMusicVolume(_originalMusicVolume);
        //GhodAudioManager.ChangeSfxVolume(_originalSfxVolume);
        //GhodAudioManager.ChangeVoiceVolume(_originalVoiceVolume);
    }

    public void _OnTotalVolumeChanged(float newVolume)
    {
        GhodAudioManager.ChangeTotalVolume(newVolume);
    }

    public void _OnMusicVolumeChanged(float newVolume)
    {
        GhodAudioManager.ChangeMusicVolume(newVolume);
    }

    public void _OnSfxVolumeChanged(float newVolume)
    {
        GhodAudioManager.ChangeSfxVolume(newVolume);
    }

    public void _OnVoiceVolumeChanged(float newVolume)
    {
        GhodAudioManager.ChangeVoiceVolume(newVolume);
    }

    public void _OnConfirmPressed()
    {
        SaveCurrentValues();
        CloseOptions();
    }

    public void _OnCancelPressed()
    {
        RevertValues();
        CloseOptions();
    }

    private void CloseOptions()
    {
        var parent = GetParent();
        if (parent is PauseScreenUi)
            (parent as PauseScreenUi).OnOptionsMenuClosed();
        // TODO: Handle title screen UI logic!
        //else if(parent is TitleScreenUi)
        //{
        //}
    }

    private void SaveCurrentValues()
    {
        _originalTotalVolume = (float) TotalVolumeSlider.Value;
        _originalMusicVolume = (float) MusicVolumeSlider.Value;
        _originalSfxVolume = (float) SfxVolumeSlider.Value;
        _originalVoiceVolume = (float) VoiceVolumeSlider.Value;
    }

    private void RevertValues()
    {
        TotalVolumeSlider.Value = _originalTotalVolume;
        MusicVolumeSlider.Value = _originalMusicVolume;
        SfxVolumeSlider.Value = _originalSfxVolume;
        VoiceVolumeSlider.Value = _originalVoiceVolume;

        GhodAudioManager.ChangeTotalVolume(_originalTotalVolume);
        GhodAudioManager.ChangeMusicVolume(_originalMusicVolume);
        GhodAudioManager.ChangeSfxVolume(_originalSfxVolume);
        GhodAudioManager.ChangeVoiceVolume(_originalVoiceVolume);
    }
}

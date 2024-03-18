using Godot;
using System.Linq;

public partial class OptionsMenuUi : Control
{
    [Export]
    private OptionButton ResolutionPicker;
    [Export]
    private Slider TotalVolumeSlider;
    [Export]
    private Slider MusicVolumeSlider;
    [Export]
    private Slider SfxVolumeSlider;
    [Export]
    private Slider VoiceVolumeSlider;
    [Export]
    private CheckBox FullscreenCheckbox;

    private GlobalSettings _originalGlobalSettings;
    private GlobalSettings _globalSettings;

    private const int DefaultResolutionChoice = 6;

    private int _currentResolutionIndex;
    private bool _initialized;

    public override void _Ready()
    {
        _originalGlobalSettings = DataSaver.GetGlobalSettings();
        _globalSettings = new GlobalSettings(_originalGlobalSettings);
        InitResolution();
    }

    private void InitResolution()
    {
        _currentResolutionIndex = GetResolutionIndex(_originalGlobalSettings.Resolution);
        ResolutionPicker.Select(_currentResolutionIndex);
        SyncResolution();
        FullscreenCheckbox.SetPressedNoSignal(_globalSettings.Fullscreen);
        SetFullscreen();
    }

    private int GetResolutionIndex(string resolution)
    {
        for (var i = 0; i < ResolutionPicker.ItemCount; i++)
        {
            if (ResolutionPicker.GetItemText(i).Equals(resolution, System.StringComparison.InvariantCultureIgnoreCase))
            {
                GD.Print($"Found resolution option at slot {i}");
                return i;
            }
        }

        return DefaultResolutionChoice;
    }

    public override void _Process(double delta)
    {
        if(!_initialized && GhodAudioManager.IsInitialized())
        {
            GD.Print("audio manager is initialized, initializing volumes!");
            SyncAllVolumes();
            _initialized = true;
        }
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
        _globalSettings.TotalVolume = (float) TotalVolumeSlider.Value;
        _globalSettings.MusicVolume = (float) MusicVolumeSlider.Value;
        _globalSettings.SfxVolume = (float) SfxVolumeSlider.Value;
        _globalSettings.VoiceVolume = (float) VoiceVolumeSlider.Value;
        _globalSettings.Resolution = ResolutionPicker.GetItemText(_currentResolutionIndex);
        _originalGlobalSettings = _globalSettings;
        DataSaver.GetInstance().SaveGlobalSettings(_globalSettings);
    }

    private void RevertValues()
    {
        _globalSettings = _originalGlobalSettings;
        SyncAllVolumes();
        InitResolution();
    }

    private void SyncAllVolumes()
    {
        TotalVolumeSlider.Value = _globalSettings.TotalVolume;
        MusicVolumeSlider.Value = _globalSettings.MusicVolume;
        SfxVolumeSlider.Value = _globalSettings.SfxVolume;
        VoiceVolumeSlider.Value = _globalSettings.VoiceVolume;

        GhodAudioManager.ChangeTotalVolume((float)TotalVolumeSlider.Value);
        GhodAudioManager.ChangeMusicVolume((float)MusicVolumeSlider.Value);
        GhodAudioManager.ChangeSfxVolume((float)SfxVolumeSlider.Value);
        GhodAudioManager.ChangeVoiceVolume((float)VoiceVolumeSlider.Value);
    }

    public void _OnResolutionChanged(int newResolutionIndex)
    {
        _currentResolutionIndex = newResolutionIndex;
        SyncResolution();
    }

    private void SyncResolution()
    {
        var resolutionText = ResolutionPicker.GetItemText(_currentResolutionIndex);
        if (!resolutionText.Contains("x")) return;

        var resolution = resolutionText.Split("x").Select(r => int.Parse(r)).ToArray();

        DisplayServer.WindowSetSize(new Vector2I(resolution[0], resolution[1]));
    }

    public void _OnSetFullscreen(bool isFullscreen)
    {
        _globalSettings.Fullscreen = isFullscreen;
        SetFullscreen();
    }

    private void SetFullscreen()
    {
        if (_globalSettings.Fullscreen)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
    }
}

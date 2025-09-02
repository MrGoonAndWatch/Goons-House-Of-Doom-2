using Godot;
using System.Linq;

public partial class OptionsMenuUi : Control
{
    [Export]
    private ScrollContainer _scrollContainer;
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
    [Export]
    private Slider GammaSlider;
    [Export]
    private CheckBox UseAnalogueMovement;
    [Export]
    private CheckBox ForceAnalogueMovement;
    [Export]
    private CheckBox SubtitlesEnabled;
    [Export]
    private CheckBox SubtitlesShowSpeaker;
    [Export]
    private Slider SubtitleBackgroundAlpha;
    [Export]
    private Label _exampleSubtitleLabel;
    [Export]
    private ColorRect _exampleSubtitleBackground;
    private const string ExampleSubtitleMessage = "Example Subtitle Text";

    private GlobalSettings _originalGlobalSettings;
    private GlobalSettings _globalSettings;

    private const int DefaultResolutionChoice = 6;

    private int _currentResolutionIndex;
    private bool _initialized;

    public override void _Ready()
    {
        _originalGlobalSettings = DataSaver.GetGlobalSettings();
        _globalSettings = new GlobalSettings(_originalGlobalSettings);
        InitGraphics();
    }

    private void InitGraphics()
    {
        _currentResolutionIndex = GetResolutionIndex(_originalGlobalSettings.Resolution);
        ResolutionPicker.Select(_currentResolutionIndex);
        SyncResolution();
        FullscreenCheckbox.SetPressedNoSignal(_globalSettings.Fullscreen);
        SetFullscreen();
        UpdateGamma(_globalSettings.Gamma);
        UseAnalogueMovement.SetPressedNoSignal(_globalSettings.UseAnalogueMovement);
        ForceAnalogueMovement.SetPressedNoSignal(_globalSettings.ForceAnalogueMovement);
        SubtitlesEnabled.SetPressedNoSignal(_globalSettings.SubtitlesEnabled);
        SubtitlesShowSpeaker.SetPressedNoSignal(_globalSettings.SubtitlesShowSpeaker);
        
    }

    private int GetResolutionIndex(string resolution)
    {
        for (var i = 0; i < ResolutionPicker.ItemCount; i++)
        {
            if (ResolutionPicker.GetItemText(i).Equals(resolution, System.StringComparison.InvariantCultureIgnoreCase))
                return i;
        }

        return DefaultResolutionChoice;
    }

    public override void _Process(double delta)
    {
        if(!_initialized && GhodAudioManager.IsInitialized())
        {
            SyncAllSliders();
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

    public void _OnGammaChanged(float newGamma)
    {
        UpdateGamma(newGamma);
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
        if (parent is TitleScreenUi)
            (parent as TitleScreenUi)._OnBackToMainMenu();
    }

    private void SaveCurrentValues()
    {
        _globalSettings.TotalVolume = (float) TotalVolumeSlider.Value;
        _globalSettings.MusicVolume = (float) MusicVolumeSlider.Value;
        _globalSettings.SfxVolume = (float) SfxVolumeSlider.Value;
        _globalSettings.VoiceVolume = (float) VoiceVolumeSlider.Value;
        _globalSettings.Resolution = ResolutionPicker.GetItemText(_currentResolutionIndex);
        _globalSettings.Gamma = (float)GammaSlider.Value;
        _globalSettings.SubtitleBackgroundAlpha = (float)SubtitleBackgroundAlpha.Value;
        _originalGlobalSettings.CopyFrom(_globalSettings);
        DataSaver.GetInstance().SaveGlobalSettings(_globalSettings);
    }

    private void RevertValues()
    {
        _globalSettings.CopyFrom(_originalGlobalSettings);
        SyncAllSliders();
        InitGraphics();
    }

    private void SyncAllSliders()
    {
        TotalVolumeSlider.Value = _globalSettings.TotalVolume;
        MusicVolumeSlider.Value = _globalSettings.MusicVolume;
        SfxVolumeSlider.Value = _globalSettings.SfxVolume;
        VoiceVolumeSlider.Value = _globalSettings.VoiceVolume;
        GammaSlider.Value = _globalSettings.Gamma;

        GhodAudioManager.ChangeTotalVolume((float)TotalVolumeSlider.Value);
        GhodAudioManager.ChangeMusicVolume((float)MusicVolumeSlider.Value);
        GhodAudioManager.ChangeSfxVolume((float)SfxVolumeSlider.Value);
        GhodAudioManager.ChangeVoiceVolume((float)VoiceVolumeSlider.Value);
        
        SubtitleBackgroundAlpha.Value = _globalSettings.SubtitleBackgroundAlpha;
        RefreshSubtitleExample();
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

    public void _OnUseAnalogueMovementSet(bool useAnalogueMovement)
    {
        _globalSettings.UseAnalogueMovement = useAnalogueMovement;
    }

    public void _OnUseForceAnalogueMovementSet(bool forceAnalogueMovement)
    {
        _globalSettings.ForceAnalogueMovement = forceAnalogueMovement;
    }

    public void _OnSetSubtitles(bool subtitlesEnabled)
    {
        _globalSettings.SubtitlesEnabled = subtitlesEnabled;
    }

    public void _OnSubtitleOpacityChanged(float newOpacity)
    {
        _globalSettings.SubtitleBackgroundAlpha = newOpacity;
        RefreshSubtitleExample();
    }

    public void _OnSetSubtitlesSpeaker(bool showSpeaker)
    {
        _globalSettings.SubtitlesShowSpeaker = showSpeaker;
        RefreshSubtitleExample();
    }

    private void RefreshSubtitleExample()
    {
        _exampleSubtitleBackground.Color = new Color(0, 0, 0, _globalSettings.SubtitleBackgroundAlpha);
        _exampleSubtitleLabel.Text = $"{(_globalSettings.SubtitlesShowSpeaker ? "Goon: " : "")}{ExampleSubtitleMessage}";
    }

    public void _onTopMenuItemFocused()
    {
        _scrollContainer.SetVScroll(0);
    }

    private void SetFullscreen()
    {
        if (_globalSettings.Fullscreen)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
    }

    private void UpdateGamma(float gamma)
    {
        if (!HasNode(GameConstants.NodePaths.FromSceneRoot.GammaCorrectionPlayer)) return;
        var gammaRect = GetNode<CanvasItem>(GameConstants.NodePaths.FromSceneRoot.GammaCorrectionPlayer);
        var gammaShader = gammaRect.Material as ShaderMaterial;
        gammaShader.SetShaderParameter(GameConstants.ShaderParameters.Gamma, gamma);
    }
}

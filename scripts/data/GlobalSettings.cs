public partial class GlobalSettings
{
    public float TotalVolume;
    public float MusicVolume;
    public float SfxVolume;
    public float VoiceVolume;

    public bool Fullscreen;
    public string Resolution;
    public float Gamma = 1.0f;

    public bool UseAnalogueMovement;
    public bool ForceAnalogueMovement;
    public bool SubtitlesEnabled;
    public bool SubtitlesShowSpeaker;
    public float SubtitleBackgroundAlpha;

    public GlobalSettings() { }

    public GlobalSettings(GlobalSettings other)
    {
        CopyFrom(other);
    }
    
    public void CopyFrom(GlobalSettings other)
    {
        TotalVolume = other.TotalVolume;
        MusicVolume = other.MusicVolume;
        SfxVolume = other.SfxVolume;
        VoiceVolume = other.VoiceVolume;
        Fullscreen = other.Fullscreen;
        Resolution = other.Resolution;
        Gamma = other.Gamma;
        UseAnalogueMovement = other.UseAnalogueMovement;
        ForceAnalogueMovement = other.ForceAnalogueMovement;
        SubtitlesEnabled = other.SubtitlesEnabled;
        SubtitlesShowSpeaker = other.SubtitlesShowSpeaker;
        SubtitleBackgroundAlpha = other.SubtitleBackgroundAlpha;
    }
}

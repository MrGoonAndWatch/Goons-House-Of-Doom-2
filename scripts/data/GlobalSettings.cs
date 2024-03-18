using System;

public partial class GlobalSettings
{
    public float TotalVolume;
    public float MusicVolume;
    public float SfxVolume;
    public float VoiceVolume;

    public bool Fullscreen;
    public string Resolution;

    public GlobalSettings() { }

    public GlobalSettings(GlobalSettings other)
    {
        TotalVolume = other.TotalVolume;
        MusicVolume = other.MusicVolume;
        SfxVolume = other.SfxVolume;
        VoiceVolume = other.VoiceVolume;
        Fullscreen = other.Fullscreen;
        Resolution = other.Resolution;
    }
}

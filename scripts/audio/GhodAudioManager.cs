using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class GhodAudioManager : Node
{
	[Export]
	private AudioStreamPlayer _musicPlayer;
    [Export]
    private AudioStreamPlayer _playerSfxPlayer;
    [Export]
    private AudioStreamPlayer _voicePlayer;

    private AudioStream _painSound;
    private AudioStream _pistolShotSound;

    private AudioStreamWav _clownSong;
    private AudioStreamWav _countdownSong;
    private AudioStreamWav _demoSong;

    private static GhodAudioManager _instance;

	private List<Task> _audioLoadingTasks;
	private bool _initializedAudio;

    private int _masterBusIndex;
    private int _musicBusIndex;
    private int _sfxBusIndex;
    private int _voiceBusIndex;

	public override void _Ready()
	{
		_audioLoadingTasks = new List<Task>();

        if (_instance != null)
		{
			GD.PrintErr($"Failed to load audio manager '{Name}', one already exists under '{_instance.Name}'!");
			return;
		}

        _instance = this;

        _audioLoadingTasks.Add(Task.Run(() => { var source = LoadSong(GameConstants.AudioAssetPaths.ClownSongPath); _clownSong = source; }));
        _audioLoadingTasks.Add(Task.Run(() => { var source = LoadSong(GameConstants.AudioAssetPaths.CountdownSongPath); _countdownSong = source; }));
        _audioLoadingTasks.Add(Task.Run(() => { var source = LoadSong(GameConstants.AudioAssetPaths.DemoSongPath); _demoSong = source; }));
        _audioLoadingTasks.Add(Task.Run(() => { var source = LoadSound(GameConstants.AudioAssetPaths.PainSfxPath); _painSound = source; }));
        _audioLoadingTasks.Add(Task.Run(() => { var source = LoadSound(GameConstants.AudioAssetPaths.GunshotSfxPath); _pistolShotSound = source; }));

        _masterBusIndex = AudioServer.GetBusIndex(GameConstants.AudioBusNames.MasterAudioBusName);
        _musicBusIndex = AudioServer.GetBusIndex(GameConstants.AudioBusNames.MusicAudioBusName);
        _sfxBusIndex = AudioServer.GetBusIndex(GameConstants.AudioBusNames.SfxAudioBusName);
        _voiceBusIndex = AudioServer.GetBusIndex(GameConstants.AudioBusNames.VoiceAudioBusName);
    }

	public override void _Process(double delta)
	{
        if (!_initializedAudio && _audioLoadingTasks.Count > 0 && _audioLoadingTasks.All(t => t.IsCompleted))
        {
            //GD.Print("All audio loaded, playing clown song...");
            _musicPlayer.Stream = _countdownSong;
			//_musicPlayer.Play();
            _initializedAudio = true;
        }
	}

    public static void PlayDemoSong()
    {
        _instance._musicPlayer.Stream = _instance._demoSong;
        _instance._musicPlayer.Play();
    }

    public static AudioStream LoadVoiceClip(string soundPath)
    {
        if (_instance == null) return null;

        var stream = ResourceLoader.Load<AudioStream>(soundPath);
        return stream;
    }

    public static void PlayVoiceClip(AudioStream stream)
    {
        if (_instance == null) return;

        _instance._voicePlayer.Stream = stream;
        _instance._voicePlayer.Play();
    }

    public static bool IsInitialized()
    {
        if (_instance == null) return false;

        return _instance._initializedAudio;
    }

    private AudioStream LoadSound(string soundPath)
    {
        var stream = ResourceLoader.Load<AudioStream>(soundPath);
        return stream;
    }

    private AudioStreamWav LoadSong(string soundPath)
    {
        //GD.Print($"Loading Song '{soundPath}'...");
        var stream = ResourceLoader.Load<AudioStreamWav>(soundPath);
        return stream;
    }

    public void _OnSongFinished()
    {
        _musicPlayer.Play();
    }

    public static void PlayPainSound()
    {
        PlayOneShotOnPlayer(_instance?._painSound);
    }

    public static void PlayPistolFired()
    {
        PlayOneShotOnPlayer(_instance?._pistolShotSound);
    }

    public static void PlayDeathSfx()
    {
        // TODO: Add a death sound and uncomment this!
        //PlayOneShotOnPlayer(_instance?._playerDeathSound);
    }

    private static void PlayOneShotOnPlayer(AudioStream sound)
    {
        PlayOneShotSound(_instance._playerSfxPlayer, sound);
    }

    private static void PlayOneShotSound(AudioStreamPlayer player, AudioStream sound)
    {
        if (!(_instance?._initializedAudio ?? false) || player == null || sound == null) return;

        player.Stream = sound;
        player.Play();
    }

    public static void ChangeTotalVolume(float newVolume)
    {
        if(_instance == null) return;

        var newDbVolume = ConvertToDb(newVolume);
        MuteOrUnmuteBus(_instance._masterBusIndex, newVolume);
        AudioServer.SetBusVolumeDb(_instance._masterBusIndex, newDbVolume);
    }

    public static void ChangeMusicVolume(float newVolume)
    {
        if (_instance == null) return;

        var newDbVolume = ConvertToDb(newVolume);
        MuteOrUnmuteBus(_instance._musicBusIndex, newVolume);
        AudioServer.SetBusVolumeDb(_instance._musicBusIndex, newDbVolume);
    }

    public static void ChangeSfxVolume(float newVolume)
    {
        if (_instance == null) return;

        var newDbVolume = ConvertToDb(newVolume);
        MuteOrUnmuteBus(_instance._sfxBusIndex, newVolume);
        AudioServer.SetBusVolumeDb(_instance._sfxBusIndex, newDbVolume);
    }

    public static void ChangeVoiceVolume(float newVolume)
    {
        if (_instance == null) return;

        var newDbVolume = ConvertToDb(newVolume);
        MuteOrUnmuteBus(_instance._voiceBusIndex, newVolume);
        AudioServer.SetBusVolumeDb(_instance._voiceBusIndex, newDbVolume);
    }

    private static void MuteOrUnmuteBus(int busIndex, float volume)
    {
        var disabled = volume <= 0;
        AudioServer.SetBusMute(busIndex, disabled);
    }

    private static float ConvertToDb(float sliderValue)
    {
        var dbVolume = ((sliderValue / 100) - 1) * 24;
        //GD.Print($"Converted sliderValue '{sliderValue}' to '{dbVolume}'db");
        return dbVolume;
    }

    public static void StopMusic()
    {
        if (_instance == null) return;

        _instance._musicPlayer.Stop();
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class GhodAudioManager : Node
{
	[Export]
	private AudioStreamPlayer _musicPlayer;
    [Export]
    private AudioStreamPlayer _playerSfxPlayer;

    private AudioStream _painSound;

	private AudioStreamWav _clownSong;
    private AudioStreamWav _countdownSong;

    private static GhodAudioManager _instance;

	private List<Task> _audioLoadingTasks;
	private bool _initializedAudio;

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
        _audioLoadingTasks.Add(Task.Run(() => { var source = LoadSound(GameConstants.AudioAssetPaths.PainSfxPath); _painSound = source; }));
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
        if (_instance == null || !_instance._initializedAudio)
            return;

        _instance._playerSfxPlayer.Stream = _instance._painSound;
        _instance._playerSfxPlayer.Play();
    }
}

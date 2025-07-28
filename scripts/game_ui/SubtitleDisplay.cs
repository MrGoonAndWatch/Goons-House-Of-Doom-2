using Godot;

public partial class SubtitleDisplay : Control
{
    private static SubtitleDisplay Instance;

    [Export]
    private Control _subtitleContainer;
    [Export]
    private ColorRect _subtitleBackground;
    [Export]
    private Label _subtitleTextDisplay;
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr($"Multiple SubtitleDisplay objects found! Ignoring instance named '{Name}'!");
            return;
        }
        
        ZIndex = (int)RenderingServer.CanvasItemZMax;
        Instance = this;
        if (_subtitleContainer == null)
            _subtitleContainer = this;
        HideSubtitles();
        RefreshSubtitleDisplaySettings();
    }

    public static void HideSubtitles()
    {
        if (Instance == null) return;
        Instance._subtitleContainer.Hide();
        Instance._subtitleTextDisplay.Text = "";
    }

    public static void DisplaySubtitles(SubtitleLine line)
    {
        if (Instance == null) return;
        if (line == null) {
            HideSubtitles();
            return;
        }
        Instance.UpdateSubtitleDisplay(line);
    }

    private void UpdateSubtitleDisplay(SubtitleLine line)
    {
        var formattedSubtitleText = line.SubtitleContent;//.Replace("\\r\\n", "\r\n").Replace("\\r", "\r").Replace("\\n", "\n");
        // TODO: Consider a smooth reveal of subtitles somehow.
        _subtitleTextDisplay.Hide();
        
        _subtitleTextDisplay.Text = formattedSubtitleText;
        _subtitleTextDisplay.Position = new Vector2(-(_subtitleTextDisplay.Size.X / 2.0f), -250); 
        // TODO: Prefix with speaker (if settings say so).
        // TODO: Change font color per settings.
        // TODO: Special formatting rules? Like special BOLD text, italics, etc.
        _subtitleContainer.Show();
        _subtitleTextDisplay.Show();
    }

    public static void RefreshSubtitleDisplaySettings()
    {
        // TODO: IMPLEMENT!!!
    }
}

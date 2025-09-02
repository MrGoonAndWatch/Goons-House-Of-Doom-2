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

    [Export]
    private float SubtitleYSpacePerLine = 75.0f;
    [Export]
    private float SubtitleExtraYSpacing = 150.0f;
    
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
    }

    public static void HideSubtitles()
    {
        if (Instance == null) return;
        Instance._subtitleContainer.Hide();
        Instance._subtitleTextDisplay.Text = "";
    }

    public static void DisplaySubtitles(SubtitleLine line)
    {
        if (Instance == null || !(DataSaver.GetGlobalSettings()?.SubtitlesEnabled ?? true))
            return;
        
        if (line == null) {
            HideSubtitles();
            return;
        }
        Instance.UpdateSubtitleDisplay(line);
    }

    private void UpdateSubtitleDisplay(SubtitleLine line)
    {
        var formattedSubtitleText = line.SubtitleContent;//.Replace("\\r\\n", "\r\n").Replace("\\r", "\r").Replace("\\n", "\n");
        
        _subtitleTextDisplay.Hide();

        if (DataSaver.GetGlobalSettings().SubtitlesShowSpeaker && !string.IsNullOrEmpty(line.CharacterSpeaking))
            formattedSubtitleText = $"{line.CharacterSpeaking}: {formattedSubtitleText}";
        
        _subtitleTextDisplay.Text = formattedSubtitleText;
        var lineCount = line.SubtitleContent.Contains('\n') ? 2 : 1;
        _subtitleTextDisplay.Position = new Vector2(-(_subtitleTextDisplay.Size.X / 2.0f), -(lineCount * SubtitleYSpacePerLine)-SubtitleExtraYSpacing);
        _subtitleBackground.Color = new Color(0, 0, 0, DataSaver.GetGlobalSettings().SubtitleBackgroundAlpha);
        // TODO: Change font color per settings.
        // TODO: Special formatting rules? Like special BOLD text, italics, etc.
        
        // TODO: Consider a smooth reveal of subtitles somehow.
        _subtitleContainer.Show();
        _subtitleTextDisplay.Show();
    }
}

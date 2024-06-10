using Godot;
using Godot.Collections;
using System;

public partial class InputMapper : Control
{
	private const string DefaultControlsFilePath = "res://data/default_controls.cfg";
	private const string ControlsFileDir = "user://settings";
    private const string ControlsFilePath = $"{ControlsFileDir}/controls.cfg";

    private Dictionary<string, Dictionary<GameConstants.ControlBinding, InputEvent>> _controlsMap;
	private bool _awaitingInput;

	[Export]
	private Control InitialFocus;

	public override void _Ready()
	{
		LoadInputMapKeys();
		LoadControlsConfig();
	}

    public void OnVisible()
    {
        InitialFocus.GrabFocus();
    }


    private void LoadInputMapKeys()
	{
        _controlsMap = new Dictionary<string, Dictionary<GameConstants.ControlBinding, InputEvent>>();
        foreach (var action in InputMap.GetActions())
        {
            if (InputMap.ActionGetEvents(action).Count != 0)
            {
                _controlsMap[action] = new Dictionary<GameConstants.ControlBinding, InputEvent>();
                foreach (var controlBinding in InputMap.ActionGetEvents(action))
                {
                    var bindingType = GetControlBindingType(controlBinding);
                    _controlsMap[action][bindingType] = controlBinding;
                }
            }
        }

		SaveControlsForDefault();
    }

	private void InitRemappers()
	{
        InitRemappers(GetChildren());
    }

	private static void InitRemappers(Array<Node> nodes)
	{
		foreach(var node in nodes)
		{
			if(node is ControlRemapButton)
                (node as ControlRemapButton).Init();
			InitRemappers(node.GetChildren());
        }
	}

	public void StartBinding()
	{
		_awaitingInput = true;
    }

	public bool IsLocked()
	{
		return _awaitingInput;
	}

    public void UpdateControlsBinding(string action, GameConstants.ControlBinding binding, InputEvent inputEvent)
    {
        if (!_controlsMap.ContainsKey(action) || _controlsMap[action] == null)
            _controlsMap[action] = new Dictionary<GameConstants.ControlBinding, InputEvent>();
        _controlsMap[action][binding] = inputEvent;
        SaveControlsConfig();
		_awaitingInput = false;
    }

	public void CancelBinding()
	{
		_awaitingInput = false;
	}

    public InputEvent GetInputEventForBinding(string action, GameConstants.ControlBinding binding)
    {
        if (!_controlsMap.ContainsKey(action))
            return null;
        if (!_controlsMap[action].ContainsKey(binding))
            return null;
        return _controlsMap[action][binding];
    }

	public void ResetToDefaultControls()
	{
		if (!FileAccess.FileExists(DefaultControlsFilePath))
		{
			GD.PrintErr($"Failed to reset to default controls. File '{DefaultControlsFilePath}' not found!");
			return;
		}

		LoadControlsFromFile(DefaultControlsFilePath);
        InitRemappers();
		SaveControlsConfig();
		_awaitingInput = false;
    }

    public static GameConstants.ControlBinding GetControlBindingType(InputEvent controlBinding)
    {
		var bindingText = controlBinding.AsText();
		if (bindingText.ToLower().Contains("d-pad"))
			return GameConstants.ControlBinding.Tertiary;
		else if (bindingText.ToLower().Contains("joypad"))
			return GameConstants.ControlBinding.Secondary;
		else
			return GameConstants.ControlBinding.Primary;
    }

    private void LoadControlsConfig()
	{
		if (FileAccess.FileExists(ControlsFilePath))
		{
			LoadControlsFromFile(ControlsFilePath);
            InitRemappers();
        }
		else
			ResetToDefaultControls();
        
	}

	private void LoadControlsFromFile(string controlsFilePath)
	{
        var file = FileAccess.Open(controlsFilePath, FileAccess.ModeFlags.Read);
        var tempControlsMap = (Dictionary<string, Dictionary<GameConstants.ControlBinding, InputEvent>>)file.GetVar(true);
        file.Close();
        foreach (var action in _controlsMap.Keys)
        {
            if (tempControlsMap.ContainsKey(action))
            {
                if (tempControlsMap[action].Keys.Count > 0)
                {
                    InputMap.ActionEraseEvents(action);
                    _controlsMap[action] = new Dictionary<GameConstants.ControlBinding, InputEvent>();
                }
                foreach (var controlType in tempControlsMap[action].Keys)
                {
                    _controlsMap[action][controlType] = tempControlsMap[action][controlType];
                    InputMap.ActionAddEvent(action, _controlsMap[action][controlType]);
                }
            }
        }
    }

	private void SaveControlsConfig()
	{
		try
		{
			DirAccess.MakeDirRecursiveAbsolute(ControlsFileDir);
			var file = FileAccess.Open(ControlsFilePath, FileAccess.ModeFlags.Write);
			file.StoreVar(_controlsMap, true);
			file.Close();
		}
		catch (Exception e)
		{
			GD.PrintErr($"Failed to save controls, got exception '{e.Message}'");
		}
	}

    private void SaveControlsForDefault()
    {
        try
        {
            DirAccess.MakeDirRecursiveAbsolute(ControlsFileDir);
            var file = FileAccess.Open($"{ControlsFileDir}/default_controls.cfg", FileAccess.ModeFlags.Write);
            file.StoreVar(_controlsMap, true);
            file.Close();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save controls, got exception '{e.Message}'");
        }
    }
}

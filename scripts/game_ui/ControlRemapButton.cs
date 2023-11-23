using Godot;

public partial class ControlRemapButton : Button
{
	[Export]
	private GameConstants.Controls _action;
	[Export]
	private InputMapper _inputMapper;
	[Export]
	private GameConstants.ControlBinding _bindingType;

	private string _actionStr;

	public ControlRemapButton()
	{
		ToggleMode = true;
	}

    public override void _Ready()
    {
        _actionStr = _action.ToString();
        SetProcessUnhandledInput(false);
    }

    public void Init()
	{
		SetTextFromMap();
	}

    public override void _Toggled(bool buttonPressed)
    {
		if (buttonPressed && _inputMapper.IsLocked())
		{
			ButtonPressed = false;
			return;
		}

        SetProcessUnhandledInput(buttonPressed);
		if (buttonPressed) {
            _inputMapper.StartBinding();
            if (_bindingType == GameConstants.ControlBinding.Primary)
				Text = "Press a Key";
			else
                Text = "Press a Button";
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
		if (e.IsPressed())
		{
			if (IsCancelButton(e))
			{
				_inputMapper.CancelBinding();
				ButtonPressed = false;
                SetTextFromMap();
                return;
			}

			var inputType = InputMapper.GetControlBindingType(e);
			// Ignore keyboard inputs when this is for a controller binding and vice-versa.
			if((_bindingType == GameConstants.ControlBinding.Primary && inputType != _bindingType) ||
               (_bindingType != GameConstants.ControlBinding.Primary && inputType == GameConstants.ControlBinding.Primary))
			{
				GD.Print($"Refusing to bind input because it was from type {inputType} but this button is for type {_bindingType} (input='{e.AsText()}')");
				return;
			}

            GD.Print($"Setting action {_actionStr} ({_bindingType}) to {e.AsText()}");
			var currentAction = _inputMapper.GetInputEventForBinding(_actionStr, _bindingType);
			if (currentAction == null)
				GD.PrintErr($"Failed to remove existing binding for {_actionStr} ({_bindingType}), binding not found in map!");
			else
				InputMap.ActionEraseEvent(_actionStr, currentAction);
			InputMap.ActionAddEvent(_actionStr, e);
			ButtonPressed = false;
			ReleaseFocus();
			_inputMapper.UpdateControlsBinding(_actionStr, _bindingType, e);
            SetTextFromMap();
        }
    }

    private void SetTextFromMap()
	{
		var currentBinding = _inputMapper.GetInputEventForBinding(_actionStr, _bindingType);
		if (currentBinding != null)
			Text = CleanInputText(currentBinding.AsText());
		else
		{
			Text = "";
			GD.PrintErr($"Failed to get action '{_actionStr}' from InputMap!");
		}
	}

	private static string CleanInputText(string inputText)
	{
		var lowerText = inputText.ToLower();
		if (lowerText.StartsWith("joypad"))
		{
			if (lowerText.Contains("-axis"))
			{
				var axisDirStr = inputText.Substring(inputText.LastIndexOf(" ") + 1);
				var foundAxisDir = float.TryParse(axisDirStr, out var axisDir);
                var axisDirText = foundAxisDir ? GetAxisDirText(lowerText, axisDir) : "";
				var startOfAxisText = inputText.IndexOf("(") + 1;
				var stickAndAxisText = inputText.Substring(startOfAxisText, inputText.IndexOf(",") - startOfAxisText);

				return $"{stickAndAxisText}{axisDirText}";
			}
			else
			{
				var startOfButtonText = inputText.IndexOf("(") + 1;
				return inputText.Substring(startOfButtonText, inputText.IndexOf(")") - startOfButtonText);
			}
		}
		else
		{
            return inputText.Replace(" (Physical)", "");
        }
    }

	private static string GetAxisDirText(string inputText, float axisDir)
	{
		if (inputText.Contains("x-axis"))
			return axisDir > 0 ? " Right" : " Left";
		else
			return axisDir > 0 ? " Down" : " Up";
	}

	private static bool IsCancelButton(InputEvent inputEvent)
	{
		var inputText = inputEvent.AsText().ToLower();
		return inputText.Equals("escape") || inputText.Contains("back");
	}
}

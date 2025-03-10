using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DebugManager : Node
{
    private static DebugManager _instance;

    private bool _debugConsoleActive;

    private bool _playerIsNoClipping;

    private List<string> _previousCommands;
    private const int MaxCommandHistory = 20;

    public override void _Ready()
    {
        if(_instance != null)
        {
            GD.PrintErr($"More than one instance of DebugManager found! Ignoring the second one (instance id {GetInstanceId()})");
            return;
        }

        _instance = this;
        _previousCommands = new List<string>();
    }

    public static bool IsDebugConsoleActive()
    {
        return _instance?._debugConsoleActive ?? false;
    }

    public static void ToggleDebugConsole()
    {
        if (_instance != null)
            _instance._debugConsoleActive = !_instance._debugConsoleActive;
    }

    public static bool IsPlayerNoClipping()
    {
        return _instance?._playerIsNoClipping ?? false;
    }

    public static (bool, string) ProcessCommand(string rawCommand)
    {
        if (_instance == null || !DataSaver.IsDebugBuild()) return (true, "");

        var tokenizedCommand = rawCommand.Split(' ')
            .Select(str => str.Trim())
            .Where(str => !string.IsNullOrEmpty(str))
            .ToArray();
        return _instance.ProcessCommand(tokenizedCommand, rawCommand);
    }

    public static (string, bool) GetPreviousCommand(int commandIndex)
    {
        if (_instance == null) return ("", false);

        return _instance.GetPreviousCommandFromCountdown(commandIndex);
    }

    private (string, bool) GetPreviousCommandFromCountdown(int countFromMostRecent)
    {
        if (countFromMostRecent < 0) return ("", false);

        var endOfList = false;
        if (countFromMostRecent >= _previousCommands.Count)
        {
            countFromMostRecent = _previousCommands.Count - 1;
            endOfList = true;
        }

        var index = _previousCommands.Count - 1 - countFromMostRecent;
        return (_previousCommands[index], endOfList);
    }

    private (bool, string) ProcessCommand(string[] tokenizedCommand, string rawCommand)
    {
        var success = true;
        var consoleOutput = "";

        if (tokenizedCommand.Length == 0) return (success, consoleOutput);

        var baseCommand = tokenizedCommand[0].ToLower();

        switch (baseCommand)
        {
            case "noclip":
                ToggleNoclip();
                consoleOutput = _playerIsNoClipping ? "noclip enabled" : "noclip disabled";
                break;
            case "save":
                OpenSaveUi();
                consoleOutput = "save screen opened";
                break;
            case "load":
                OpenLoadUi();
                consoleOutput = "load screen opened";
                break;
            case "go":
                (success, consoleOutput) = WarpToScene(tokenizedCommand);
                break;
            case "help":
                consoleOutput = GetHelpCommandPrintout();
                break;
            default:
                consoleOutput = $"unrecognized command '{baseCommand}'";
                success = false;
                break;
        }

        SaveCommandInHistory(rawCommand);

        return (success, consoleOutput);
    }

    private void SaveCommandInHistory(string rawCommand)
    {
        _previousCommands.Add(rawCommand);
        if (_previousCommands.Count > MaxCommandHistory)
            _previousCommands.RemoveAt(0);
    }

    private void ToggleNoclip()
    {
        _playerIsNoClipping = !_playerIsNoClipping;
        var player = GetNode<Player>(GameConstants.NodePaths.FromSceneRoot.Player);
        player.RefreshNoClip();
    }

    private void OpenSaveUi()
    {
        var saveGameUi = GetNode<SaveGame>(GameConstants.NodePaths.FromSceneRoot.SaveGameUi);
        saveGameUi.ShowSaveUi();
    }

    private void OpenLoadUi()
    {
        var saveGameUi = GetNode<SaveGame>(GameConstants.NodePaths.FromSceneRoot.SaveGameUi);
        saveGameUi.ShowLoadUi();
    }

    private static (bool, string) WarpToScene(string[] args)
    {
        if(args.Length < 2)
        {
            var errorMessage = "cannot run 'go' command without a target scene in the second param!";
            GD.PrintErr(errorMessage);
            return (false, errorMessage);
        }

        var firstArgSanitized = args[1]?.Replace("-", "").ToLower();
        if (firstArgSanitized == "help" || firstArgSanitized == "h")
        {
            return (true, "go {scene-filename-nopath} [{target-x} {target-y} {target-z}] [{target-rot-degrees}] [{door-load-type}]");
        }

        var sceneChanger = SceneChanger.GetInstance();
        if (sceneChanger == null)
        {
            var errorMessage = "failed to run 'go' command: failed to find a SceneChanger instance.";
            GD.PrintErr(errorMessage);
            return (false, errorMessage);
        }

        var targetRoom = args[1];

        float consoleX;
        float consoleY;
        float consoleZ;
        var targetPosition = Vector3.Zero;
        if (args.Length >= 5 && float.TryParse(args[2], out consoleX) && float.TryParse(args[3], out consoleY) && float.TryParse(args[4], out consoleZ))
        {
            targetPosition = new Vector3(consoleX, consoleY, consoleZ);
        }

        var targetRotation = Vector3.Zero;
        float consoleRotation;
        if (args.Length >= 6 && float.TryParse(args[5], out consoleRotation))
        {
            targetRotation = new Vector3(0, consoleRotation, 0);
        }

        GameConstants.DoorLoadType doorLoadType = GameConstants.DoorLoadType.None;
        if (args.Length >= 7)
        {
            GameConstants.DoorLoadType.TryParse(args[6], out doorLoadType);
        }

        var sceneLoadData = new SceneLoadData
        {
            TargetScene = targetRoom,
            LoadPosition = targetPosition,
            LoadRotation = targetRotation,
        };

        (var validSceneChange, var sceneChangeError) = SceneChanger.IsValidSceneChange(sceneLoadData, doorLoadType);
        if (!validSceneChange)
            return (validSceneChange, sceneChangeError);

        sceneChanger.ChangeScene(sceneLoadData, doorLoadType);
        return (true, $"successfully set warp to {targetRoom}");
    }

    private static string GetHelpCommandPrintout()
    {
        return "noclip - toggles noclip\r\nsave - open save screen\r\nload - open load screen\r\ngo - warp to room";
    }
}
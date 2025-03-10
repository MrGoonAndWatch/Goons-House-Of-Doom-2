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

    public static void ProcessCommand(string rawCommand)
    {
        if (_instance == null || !DataSaver.IsDebugBuild()) return;

        var tokenizedCommand = rawCommand.Split(' ')
            .Select(str => str.Trim())
            .Where(str => !string.IsNullOrEmpty(str))
            .ToArray();
        _instance.ProcessCommand(tokenizedCommand, rawCommand);
    }

    public static Tuple<string, bool> GetPreviousCommand(int commandIndex)
    {
        if (_instance == null) return new Tuple<string, bool>("", false);

        return _instance.GetPreviousCommandFromCountdown(commandIndex);
    }

    private Tuple<string, bool> GetPreviousCommandFromCountdown(int countFromMostRecent)
    {
        if (countFromMostRecent < 0) return new Tuple<string, bool>("", false);

        var endOfList = false;
        if (countFromMostRecent >= _previousCommands.Count)
        {
            countFromMostRecent = _previousCommands.Count - 1;
            endOfList = true;
        }

        var index = _previousCommands.Count - 1 - countFromMostRecent;
        return new Tuple<string, bool>(_previousCommands[index], endOfList);
    }

    private void ProcessCommand(string[] tokenizedCommand, string rawCommand)
    {
        if (tokenizedCommand.Length == 0) return;

        var baseCommand = tokenizedCommand[0].ToLower();

        switch (baseCommand)
        {
            case "noclip":
                ToggleNoclip();
                break;
            case "save":
                OpenSaveUi();
                break;
            case "load":
                OpenLoadUi();
                break;
            case "go":
                WarpToScene(tokenizedCommand);
                break;
        }

        SaveCommandInHistory(rawCommand);
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

    private static void WarpToScene(string[] args)
    {
        if(args.Length < 2)
        {
            GD.PrintErr("Cannot run 'go' command without a target scene in the second param!");
            return;
        }

        var sceneChanger = SceneChanger.GetInstance();
        if (sceneChanger == null)
        {
            GD.PrintErr("Failed to run 'go' command: failed to find a SceneChanger instance.");
            return;
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
        sceneChanger.ChangeScene(sceneLoadData, doorLoadType);
    }
}
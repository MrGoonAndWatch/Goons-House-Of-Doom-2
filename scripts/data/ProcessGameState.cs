using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ProcessGameState : Node3D
{
    // TODO: Nuke this class, we've pushed the responsibility of checking game state down to each respective object type.
	public override void _Ready()
	{
        LoadGameStateToScene();
    }

    private void LoadGameStateToScene()
    {
        //var dataSaver = DataSaver.GetInstance();
        //var gameState = dataSaver.GetGameState();

        //var doors = FindObjectsOfType<Door>();
        //var passCodes = FindObjectsOfType<PassCode>();

        //UnlockPreviouslyUnlockedDoors(gameState, doors);
        //ProcessPreviouslyTriggeredEvent(gameState.TriggeredEvents, doors, passCodes);
    }

    private static void UnlockPreviouslyUnlockedDoors(DataSaver.GameState gameState, List<Door> doors)
    {
        foreach (var door in doors)
        {
            if (door.DoorId != 0 && gameState.DoorsUnlocked.Contains(door.DoorId))
                door.ForceUnlock();
        }
    }

    // private static void ProcessPreviouslyTriggeredEvent(int[] triggeredEvents, List<Door> doors, List<PassCode> passCodes)
    // {
    //     foreach (var gameStateTriggeredEvent in triggeredEvents)
    //     {
    //         foreach (var passCode in passCodes)
    //         {
    //             passCode.OnEvent((GameConstants.GlobalEvent)gameStateTriggeredEvent);
    //         }
    // 
    //         // foreach (var inspectable in inspectables)
    //         // {
    //         //     if ((int)inspectable.EventToTrigger == gameStateTriggeredEvent)
    //         //         inspectable.SetTriggered();
    //         // }
    //     }
    // }

    private List<T> FindObjectsOfType<T>() where T: class
    {
        return FindObjectsOfType<T>(GetTree().Root);
    }

    public static List<T> FindObjectsOfType<T>(Node parent) where T: class
    {
        var matchingNodes = new List<T>();

        var children = parent.GetChildren();
        foreach (var node in children)
        {
            if (node is T) matchingNodes.Add(node as T);
            matchingNodes.AddRange(FindObjectsOfType<T>(node));
        }

        return matchingNodes;
    }
}

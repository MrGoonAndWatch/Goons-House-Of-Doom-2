using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ProcessGameState : Node3D
{
	public override void _Ready()
	{
        LoadGameStateToScene();
    }

    private void LoadGameStateToScene()
    {
        var dataSaver = DataSaver.GetInstance();
        var gameState = dataSaver.GetGameState();

        var enemies = FindObjectsOfType<Enemy>();
        var doors = FindObjectsOfType<Door>();
        var items = FindObjectsOfType<Item>();
        var passCodes = FindObjectsOfType<PassCode>();
        GD.Print($"Found {enemies.Count} enemies, {doors.Count} doors, {items.Count} items.");

        DestroyPreviouslyKilledEnemies(gameState, enemies);
        UnlockPreviouslyUnlockedDoors(gameState, doors);
        ProcessPreviouslyTriggeredEvent(gameState.TriggeredEvents, doors, passCodes);
        DeletePreviouslyPickedUpItems(gameState, items);
    }

    private static void DestroyPreviouslyKilledEnemies(DataSaver.GameState gameState, List<Enemy> enemies)
    {
        foreach (var enemy in enemies)
        {
            if (enemy.EnemyId != 0 && gameState.DeadEnemies.Contains(enemy.EnemyId))
            {
                GD.Print($"Killing enemy {enemy.EnemyId}");
                enemy.ForceDead();
            }
        }
    }

    private static void UnlockPreviouslyUnlockedDoors(DataSaver.GameState gameState, List<Door> doors)
    {
        foreach (var door in doors)
        {
            if (door.DoorId != 0 && gameState.DoorsUnlocked.Contains(door.DoorId))
                door.ForceUnlock();
        }
    }

    private static void DeletePreviouslyPickedUpItems(DataSaver.GameState gameState, List<Item> items)
    {
        foreach (var item in items)
        {
            if (item.ItemId != 0 && gameState.GrabbedItems.Contains(item.ItemId))
                item.ForceDestroy();
        }
    }

    private static void ProcessPreviouslyTriggeredEvent(int[] triggeredEvents, List<Door> doors, List<PassCode> passCodes)
    {
        foreach (var gameStateTriggeredEvent in triggeredEvents)
        {
            // TODO: Eventually probably need to propagate these through more than just doors...
            foreach (var door in doors)
            {
                door.OnEvent((GameConstants.GlobalEvent)gameStateTriggeredEvent);
            }

            foreach (var passCode in passCodes)
            {
                passCode.OnEvent((GameConstants.GlobalEvent)gameStateTriggeredEvent);
            }

            // foreach (var inspectable in inspectables)
            // {
            //     if ((int)inspectable.EventToTrigger == gameStateTriggeredEvent)
            //         inspectable.SetTriggered();
            // }
        }
    }

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

using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System;

public class RandomizerSeed
{
    private const int MaxEnemyId = 1;
    private const int MaxItemId = 1;

    public static RandomizerSeed GenerateRandomizer(RandomizerSettings settings)
    {
        var settingsJson = JsonConvert.SerializeObject(settings);
        GD.Print($"Generating randomizer with settings: {settingsJson}");

        var generatedRandomizer = new RandomizerSeed
        {
            AllowSpawnsOnEmptyEnemySlotsForDifficulty = settings.AllowSpawnsOnEmptyEnemySlotsForDifficulty,
            AllowSpawnsOnEmptyItemSlotsForDifficulty = settings.AllowSpawnsOnEmptyItemSlotsForDifficulty
        };

        var enemyTypesCount = Enum.GetValues(typeof(GameConstants.EnemySpawnType)).Length;
        var itemTypesCount = Enum.GetValues(typeof(GameConstants.ItemSpawnType)).Length;
        var randomizerSeedForEnemies = settings.Seed.HasValue ? settings.Seed.Value : 0;
        var randomizerSeedForItems = settings.Seed.HasValue ? settings.Seed.Value : 0;

        if (settings.RandomizeEnemies)
        {
            for (var i = 1; i <= MaxEnemyId; i++)
            {
                var foundValidRandomEnemyPick = false;
                while (!foundValidRandomEnemyPick) {
                    var randomEnemyPick = (settings.Seed.HasValue ? GD.RandFromSeed(ref randomizerSeedForEnemies) : GD.Randi()) % enemyTypesCount;
                    var pickedEnemyId = (ulong) Math.Pow(2, randomEnemyPick);
                    if ((settings.EnabledEnemySpawnTypes & pickedEnemyId) > 0)
                    {
                        foundValidRandomEnemyPick = true;
                        generatedRandomizer.RandomizedEnemies.Add(i, (GameConstants.EnemySpawnType)pickedEnemyId);
                    }
                }
            }
        }
        
        if (settings.RandomizeItems)
        {
            // TODO: Implement this. Definitely need to rework this to ensure we spawn keys and spawn them in the right spots!
        }

        return generatedRandomizer;
    }

    public RandomizerSeed()
    {
        RandomizedEnemies = new Dictionary<int, GameConstants.EnemySpawnType>();
        RandomizedItems = new Dictionary<int, GameConstants.ItemSpawnType>();
    }

    public Dictionary<int, GameConstants.EnemySpawnType> RandomizedEnemies;
    public Dictionary<int, GameConstants.ItemSpawnType> RandomizedItems;
    public bool AllowSpawnsOnEmptyEnemySlotsForDifficulty;
    public bool AllowSpawnsOnEmptyItemSlotsForDifficulty;
}

public class RandomizerSettings
{
    public ulong? Seed;

    public bool RandomizeEnemies;
    public bool RandomizeItems;

    public bool AllowSpawnsOnEmptyEnemySlotsForDifficulty;
    public bool AllowSpawnsOnEmptyItemSlotsForDifficulty;

    public ulong EnabledEnemySpawnTypes = ulong.MaxValue;
    public ulong EnabledItemSpawnTypes = ulong.MaxValue;
}
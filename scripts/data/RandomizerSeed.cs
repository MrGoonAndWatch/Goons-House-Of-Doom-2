using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

public class RandomizerSeed
{
    private const int MaxEnemyId = 1;
    private const int MaxItemId = 4;

    public static RandomizerSeed GenerateRandomizer(RandomizerSettings settings)
    {
        var settingsJson = JsonConvert.SerializeObject(settings);
        GD.Print($"Generating randomizer with settings: {settingsJson}");

        var generatedRandomizer = new RandomizerSeed
        {
            AllowSpawnsOnEmptyEnemySlotsForDifficulty = settings.AllowSpawnsOnEmptyEnemySlotsForDifficulty,
            AllowSpawnsOnEmptyItemSlotsForDifficulty = settings.AllowSpawnsOnEmptyItemSlotsForDifficulty
        };

        RandomizeEnemies(settings, generatedRandomizer);
        RandomizeItems(settings, generatedRandomizer);
        // TODO: randomize the puzzle codes!

        // TODO: Create spoiler log w/ as much detail as possible.

        return generatedRandomizer;
    }

    private static void RandomizeEnemies(RandomizerSettings settings, RandomizerSeed generatedRandomizer)
    {
        if (!settings.RandomizeEnemies)
            return;

        var randomizerSeedForEnemies = settings.Seed.HasValue ? settings.Seed.Value : 0;

        var enemySpawnProbabilities = GenerateProbabilityLookup(settings.EnemySpawnProbabilities);
        if (enemySpawnProbabilities.Count == 0)
            return;

        for (var i = 1; i <= MaxEnemyId; i++)
        {
            var selectedEnemy = RandomlySelectWithWeightedProbability(enemySpawnProbabilities, settings.Seed.HasValue, randomizerSeedForEnemies);
            generatedRandomizer.RandomizedEnemies.Add(i, selectedEnemy);
            GD.Print($"Randomized enemy {i} to a '{selectedEnemy}'");
        }
    }

    private static void RandomizeItems(RandomizerSettings settings, RandomizerSeed generatedRandomizer)
    {
        if (!settings.RandomizeItems)
            return;

        var randomizerSeedForItems = settings.Seed.HasValue ? settings.Seed.Value : 0;
        var itemIdsUsedUpByKeys = new Godot.Collections.Dictionary<int, bool>();

        var zoneCount = GameConstants.ZoneKeyMap.Keys.Count;
        for (var i = 0; i < zoneCount; i++)
        {
            var currentZoneId = GameConstants.ZoneKeyMap.Keys.Skip(i).First();
            var validItemIds = GameConstants.ZoneKeyMap[currentZoneId].ItemIdsInZone.ToList();
            validItemIds = validItemIds.Except(itemIdsUsedUpByKeys.Keys).ToList();
            for (var j = 0; j < GameConstants.ZoneKeyMap[currentZoneId].KeysRequiredToPassZone.Count; j++)
            {
                var randomIndex = (int)(settings.Seed.HasValue ? GD.RandFromSeed(ref randomizerSeedForItems) : GD.Randi()) % validItemIds.Count;
                GD.Print($"randomIndex={randomIndex},validItemIds.Count={validItemIds.Count}");
                var selectedItemId = validItemIds[randomIndex];
                itemIdsUsedUpByKeys.Add(selectedItemId, true);
                var currentKeyType = GameConstants.ZoneKeyMap[currentZoneId].KeysRequiredToPassZone[j];
                generatedRandomizer.RandomizedItems.Add(selectedItemId, currentKeyType);
                validItemIds.RemoveAt(randomIndex);
                GD.Print($"Randomized key '{currentKeyType}' to item id {selectedItemId}");
            }
        }

        var itemSpawnProbabilities = GenerateProbabilityLookup(settings.ItemSpawnProbabilities);
        if (itemSpawnProbabilities.Count == 0)
            return;

        for (var i = 1; i <= MaxItemId; i++)
        {
            if (itemIdsUsedUpByKeys.ContainsKey(i))
                continue;

            var selectedItem = RandomlySelectWithWeightedProbability(itemSpawnProbabilities, settings.Seed.HasValue, randomizerSeedForItems);
            generatedRandomizer.RandomizedItems.Add(i, selectedItem);
            GD.Print($"Randomized item {i} to a '{selectedItem}'");
        }
    }

    private static List<Tuple<T, float>> GenerateProbabilityLookup<T>(List<Tuple<T, float>> probabilities)
    {
        var enemySpawnProbabilities = new List<Tuple<T, float>>();
        var sum = 0.0f;
        var probabilityCount = probabilities.Count;
        for (var i = 0; i < probabilityCount; i++)
        {
            if (probabilities[i].Item2 <= 0)
                continue;
            sum += probabilities[i].Item2;
            enemySpawnProbabilities.Add(new Tuple<T, float>(probabilities[i].Item1, sum));
        }
        return enemySpawnProbabilities;
    }

    private static T RandomlySelectWithWeightedProbability<T>(List<Tuple<T, float>> probabilityLookup, bool usingSeed, ulong randomizerSeedForEnemies)
    {
        var randomNumberZeroToOne = ((usingSeed ? GD.RandFromSeed(ref randomizerSeedForEnemies) : GD.Randi()) % 10001) / 10000.0f;

        for (var i = 0; i < probabilityLookup.Count; i++)
        {
            if (probabilityLookup[i].Item2 >= randomNumberZeroToOne)
                return probabilityLookup[i].Item1;
        }
        // Note: this should never happen?
        GD.PrintErr($"GetSelectedProbabilityValue failed to find a record with probability >= {randomNumberZeroToOne}!");
        return default;
    }

    public RandomizerSeed()
    {
        RandomizedEnemies = new Godot.Collections.Dictionary<int, GameConstants.EnemySpawnType>();
        RandomizedItems = new Godot.Collections.Dictionary<int, GameConstants.ItemSpawnType>();
        PassCodeLookup = new Godot.Collections.Dictionary<GameConstants.PassCodeType, string>();
    }

    public Godot.Collections.Dictionary<int, GameConstants.EnemySpawnType> RandomizedEnemies;
    public Godot.Collections.Dictionary<int, GameConstants.ItemSpawnType> RandomizedItems;
    public Godot.Collections.Dictionary<GameConstants.PassCodeType, string> PassCodeLookup;
    public bool AllowSpawnsOnEmptyEnemySlotsForDifficulty;
    public bool AllowSpawnsOnEmptyItemSlotsForDifficulty;
}

public class RandomizerSettings
{
    public ulong? Seed;

    public bool RandomizeEnemies;
    public bool RandomizeItems;
    public bool RandomizePuzzleCodes;

    public bool AllowSpawnsOnEmptyEnemySlotsForDifficulty;
    public bool AllowSpawnsOnEmptyItemSlotsForDifficulty;

    public List<Tuple<GameConstants.EnemySpawnType, float>> EnemySpawnProbabilities;
    public List<Tuple<GameConstants.ItemSpawnType, float>> ItemSpawnProbabilities;
}
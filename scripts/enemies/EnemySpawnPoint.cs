using Godot;
using static GameConstants;

public partial class EnemySpawnPoint : Node3D
{
    [Export(hintString: "Unique identifier for the enemy, use this number ONCE per the entire game!")]
    private int EnemyId;

    [Export]
    private EnemySpawnType EnemySpawnOnEasy = EnemySpawnType.None;
    [Export]
    private EnemySpawnType EnemySpawnOnNormal = EnemySpawnType.None;
    [Export]
    private EnemySpawnType EnemySpawnOnHard = EnemySpawnType.None;
    [Export]
    private EnemySpawnType EnemySpawnOnImpossible = EnemySpawnType.None;

    public override void _Ready()
    {
        var playerStatus = PlayerStatus.GetInstance();
        if (playerStatus.KilledEnemies.Contains(EnemyId)) return;

        EnemySpawnType enemySpawnType;
        switch (playerStatus.GameDifficulty) {
            case GameDifficulty.Easy:
                enemySpawnType = EnemySpawnOnEasy;
                break;
            case GameDifficulty.Normal:
                enemySpawnType = EnemySpawnOnNormal;
                break;
            case GameDifficulty.Hard:
                enemySpawnType = EnemySpawnOnHard;
                break;
            case GameDifficulty.Impossible:
            default:
                enemySpawnType = EnemySpawnOnImpossible;
                break;
        }

        if (playerStatus.RandomizerEnabled && playerStatus.RandomizerSeed.RandomizedEnemies.ContainsKey(EnemyId))
        {
            if (enemySpawnType != EnemySpawnType.None || playerStatus.RandomizerSeed.AllowSpawnsOnEmptyItemSlotsForDifficulty)
                enemySpawnType = playerStatus.RandomizerSeed.RandomizedEnemies[EnemyId];
        }

        if (enemySpawnType != EnemySpawnType.None)
            SpawnEnemy(enemySpawnType);
    }

    private void SpawnEnemy(EnemySpawnType enemySpawnType)
    {
        if (!EnemyPrefabLookup.ContainsKey(enemySpawnType))
        {
            GD.PrintErr($"Failed to spawn enemy, no prefab path found for enemy type '{enemySpawnType}'.");
            return;
        }

        var enemySceneLoad = GD.Load<PackedScene>(EnemyPrefabLookup[enemySpawnType]);
        if(enemySceneLoad == null)
        {
            GD.PrintErr($"Failed to spawn enemy of type '{enemySpawnType}', scene not found '{EnemyPrefabLookup[enemySpawnType]}' .");
            return;
        }
        var enemyScene = enemySceneLoad.Instantiate();
        var enemy = enemyScene as Enemy;
        if(enemy == null)
        {
            GD.PrintErr($"Failed to spawn enemy of type '{enemySpawnType}'. Scene '{EnemyPrefabLookup[enemySpawnType]}' does not have an Enemy script at its root node!");
            return;
        }

        enemy.EnemyId = EnemyId;

        AddChild(enemy);
    }
}

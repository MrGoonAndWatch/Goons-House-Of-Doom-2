using Godot;

public partial class EnemySpawnPoint : Node3D
{
    [Export]
    private int EnemyId;

    [Export]
    private GameConstants.EnemySpawnType EnemySpawnOnEasy;
    [Export]
    private GameConstants.EnemySpawnType EnemySpawnOnNormal;
    [Export]
    private GameConstants.EnemySpawnType EnemySpawnOnHard;
    [Export]
    private GameConstants.EnemySpawnType EnemySpawnOnImpossible;

    public override void _Ready()
    {
        if (PlayerStatus.GetInstance().KilledEnemies.Contains(EnemyId)) return;

        GameConstants.EnemySpawnType enemySpawnType;
        switch (PlayerStatus.GetInstance().GameDifficulty) {
            case GameConstants.GameDifficulty.Easy:
                enemySpawnType = EnemySpawnOnEasy;
                break;
            case GameConstants.GameDifficulty.Normal:
                enemySpawnType = EnemySpawnOnNormal;
                break;
            case GameConstants.GameDifficulty.Hard:
                enemySpawnType = EnemySpawnOnHard;
                break;
            case GameConstants.GameDifficulty.Impossible:
            default:
                enemySpawnType = EnemySpawnOnImpossible;
                break;
        }

        // TODO: Get randomizer settings to check if this needs to be swapped.

        if (enemySpawnType != GameConstants.EnemySpawnType.None)
            SpawnEnemy(enemySpawnType);
    }

    private void SpawnEnemy(GameConstants.EnemySpawnType enemySpawnType)
    {
        if (!GameConstants.EnemyPrefabLookup.ContainsKey(enemySpawnType))
        {
            GD.PrintErr($"Failed to spawn enemy, no prefab path found for enemy type '{enemySpawnType}'.");
            return;
        }

        var enemySceneLoad = GD.Load<PackedScene>(GameConstants.EnemyPrefabLookup[enemySpawnType]);
        if(enemySceneLoad == null)
        {
            GD.PrintErr($"Failed to spawn enemy of type '{enemySpawnType}', scene not found '{GameConstants.EnemyPrefabLookup[enemySpawnType]}' .");
            return;
        }
        var enemyScene = enemySceneLoad.Instantiate();
        var enemy = enemyScene as Enemy;
        if(enemy == null)
        {
            GD.PrintErr($"Failed to spawn enemy of type '{enemySpawnType}'. Scene '{GameConstants.EnemyPrefabLookup[enemySpawnType]}' does not have an Enemy script at its root node!");
            return;
        }

        enemy.EnemyId = EnemyId;

        AddChild(enemy);
    }
}

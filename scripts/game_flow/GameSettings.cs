using System;

public partial class GameSettings
{
    public Guid PlaythroughId;

    public GameConstants.GameDifficulty GameDifficulty;

    public bool IsRandomized;
    public RandomizerSeed RandomizerSeed;

    public bool FunnyMode;
}

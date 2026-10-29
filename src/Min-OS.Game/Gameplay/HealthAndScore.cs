using MinOS.Core;

namespace MinOS.Gameplay;

public sealed class HealthSystem : IHealthSystem
{
    public int MaxHp(int hpUpgrade) => 1 + Math.Max(0, hpUpgrade);
    public int ApplyMistake(int currentHp) => Math.Max(0, currentHp - 1);
    public bool ShouldRegenerate(int completedSets, int setsInLevel, int regenUpgrade, bool regeneratedThisLevel)
    {
        // Version 0 permits no more than one regeneration event per level.
        if (regeneratedThisLevel || completedSets <= 0) return false;
        var requiredSets = Math.Max(1, setsInLevel - Math.Max(0, regenUpgrade));
        return completedSets >= requiredSets;
    }
    public int Regenerate(int currentHp, int maxHp) => Math.Min(maxHp, currentHp + 1);
}

public sealed class ScoreCalculator : IScoreCalculator
{
    public decimal LevelReward(int level) => (decimal)Math.Pow(2, level - 1);
    public double MultiplierAfterSet(double current) => current + GameBalance.MultiplierIncrement;
    public decimal AwardedBits(int level, double multiplier) => LevelReward(level) * (decimal)multiplier;
}

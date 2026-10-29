using MinOS.Core;

namespace MinOS.Gameplay;

public sealed class PatternGenerator : IPatternGenerator
{
    public IReadOnlyList<PatternSet> Generate(int level, int? seed = null)
    {
        if (level is < 1 or > GameBalance.MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
        var random = seed.HasValue ? new Random(seed.Value) : Random.Shared;
        return Enumerable.Range(0, level).Select(_ => new PatternSet(
            Next(random), Next(random), Next(random), Next(random))).ToArray();
    }

    private static ColorId Next(Random random) => (ColorId)random.Next(0, 9);
}

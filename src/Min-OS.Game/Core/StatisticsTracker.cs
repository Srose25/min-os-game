namespace MinOS.Core;

/// <summary>Owns cumulative slot statistics independently from the temporary RunState.</summary>
public sealed class StatisticsTracker
{
    public StatisticsData Data { get; private set; }
    public StatisticsTracker(StatisticsData? initial = null) => Data = initial ?? new StatisticsData();
    // QA sprint 3: these counters are cumulative save-slot data, never RunState fields.
    public void RecordInput() => Data = Data with { Inputs = Data.Inputs + 1 };
    public void RecordSetCompleted() => Data = Data with { CompletedSets = Data.CompletedSets + 1 };
    public void RecordGameOver() => Data = Data with { GameOvers = Data.GameOvers + 1 };
    public void RecordLevel(int level, decimal reward) => Data = Data with { HighestLevel = Math.Max(Data.HighestLevel, level), TotalBitsEarned = Data.TotalBitsEarned + reward };
}

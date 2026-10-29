namespace MinOS.Core;

public enum ColorId { Color0, Color1, Color2, Color3, Color4, Color5, Color6, Color7, Color8 }
public enum GamePhase { Preparing, ShowingPattern, WaitingForInput, Breakpoint, LevelComplete, GameOver }
public enum ValidationResult { Ignored, Correct, Wrong, SetCompleted, LevelCompleted }
public enum UpgradeId { InputTime, MemoryTime, Hp, Regen, CachedMemory, AutoComplete, AutoCooldown, Breakpoint, Show, StartingLevel }

public readonly record struct ColorInput(ColorId Color);
public readonly record struct PatternSet(ColorId A, ColorId B, ColorId C, ColorId D)
{
    public IReadOnlyList<ColorId> Colors => [A, B, C, D];
}

public sealed record RunState
{
    public required int CurrentLevel { get; init; }
    public required int CurrentSet { get; init; }
    public required IReadOnlyList<PatternSet> Sets { get; init; }
    public required int InputIndex { get; init; }
    public required int CurrentHp { get; init; }
    public required int MaxHp { get; init; }
    public required double InputSecondsRemaining { get; init; }
    public required double MemorySecondsRemaining { get; init; }
    public required double Multiplier { get; init; }
    public required int ConsecutiveSets { get; init; }
    public required decimal RunBits { get; init; }
    public required GamePhase Phase { get; init; }
    public required int ShowUsesRemaining { get; init; }
    public required int AutoCompleteCooldownLevels { get; init; }
    public int AutoCompleteCharges { get; init; }
    public bool PatternRevealed { get; init; }
    public bool RegeneratedThisLevel { get; init; }
}

public sealed record SettingsData
{
    public Dictionary<ColorId, string> Bindings { get; init; } = [];
    public float MusicVolume { get; init; } = 0f;
    public float SfxVolume { get; init; } = 1f;
}

public sealed record StatisticsData
{
    public long CompletedSets { get; init; }
    public long GameOvers { get; init; }
    public long Inputs { get; init; }
    public int HighestLevel { get; init; }
    public decimal TotalBitsEarned { get; init; }
}

public sealed record SaveSlotData
{
    public int SchemaVersion { get; init; } = 1;
    public Dictionary<UpgradeId, int> UpgradeLevels { get; init; } = [];
    public decimal BankedBits { get; init; }
    public SettingsData Settings { get; init; } = new();
    public StatisticsData Statistics { get; init; } = new();
    public RunState? ActiveRun { get; init; }
}

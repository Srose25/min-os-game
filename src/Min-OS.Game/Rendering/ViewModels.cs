using MinOS.Core;

namespace MinOS.Rendering;

public sealed record PatternSetViewModel(IReadOnlyList<ColorId> Colors, bool Visible, bool Cached, bool AutoCompleted);
public sealed record GameViewModel(int Level, double SecondsRemaining, int Hp, int MaxHp, double Multiplier, decimal Bits, IReadOnlyList<IReadOnlyList<PatternSetViewModel>> Rows, GamePhase Phase);
public sealed record StoreRowViewModel(UpgradeId Upgrade, int CurrentLevel, int MaxLevel, decimal Price, bool Affordable);
public sealed record LeaderboardViewModel(StatisticsData Statistics);

// Renderer rule: these models are snapshots. Draw code must never call gameplay mutation methods.

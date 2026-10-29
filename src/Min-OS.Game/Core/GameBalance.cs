namespace MinOS.Core;

public static class GameBalance
{
    public const string GameTitle = "Min-OS";
    public const int MaxLevel = 15;
    public const int ColorsPerSet = 4;
    public const int SetsPerPatternRow = 3;
    public const double InitialMultiplier = 1.0;
    public const double MultiplierIncrement = 0.1;
    public const double DefaultMemorySeconds = 3.0;
    public const double DefaultInputSeconds = 30.0;
    public const double DefaultBreakpointSeconds = 2.0;
    public const double DefaultBreakpointTimeRestoreSeconds = 1.0;
    public const int DefaultRegenSetInterval = 1;
    public const int DefaultAutoCooldownLevels = 5;
    // Architect assumption: Color8 is a neutral gray so white remains distinct on the blue background.
    public static readonly string[] PaletteHex = ["#AE00FF", "#FF00FF", "#0000FF", "#00FFFF", "#00FF00", "#FFFF00", "#FF7300", "#FFFFFF", "#808080"];

    // Engineer/QA: prices are intentionally linear in version 0. Adjust only these base prices during balancing.
    public static decimal PriceFor(UpgradeId id, int nextLevel) => id switch
    {
        UpgradeId.InputTime => 2m * nextLevel,
        UpgradeId.MemoryTime => 3m * nextLevel,
        UpgradeId.Hp => 8m * nextLevel,
        UpgradeId.Regen => 12m * nextLevel,
        UpgradeId.CachedMemory => 10m * nextLevel,
        UpgradeId.AutoComplete => 20m * nextLevel,
        UpgradeId.AutoCooldown => 30m * nextLevel,
        UpgradeId.Breakpoint => 10m * nextLevel,
        UpgradeId.Show => 25m * nextLevel,
        UpgradeId.StartingLevel => 100m * nextLevel,
        _ => throw new ArgumentOutOfRangeException(nameof(id))
    };

    public static double BreakpointTimeRestoreSeconds(int breakpointUpgrade) => DefaultBreakpointTimeRestoreSeconds + Math.Max(0, breakpointUpgrade);
}

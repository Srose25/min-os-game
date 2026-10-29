using MinOS.Core;
using MinOS.Gameplay;

namespace MinOS.Store;

public sealed class StoreCatalog : IStoreCatalog
{
    private static readonly IReadOnlyDictionary<UpgradeId, int> Limits = new Dictionary<UpgradeId, int>
    {
        [UpgradeId.InputTime] = 50, [UpgradeId.MemoryTime] = 20, [UpgradeId.Hp] = 25, [UpgradeId.Regen] = 10,
        [UpgradeId.CachedMemory] = 10, [UpgradeId.AutoComplete] = 5, [UpgradeId.AutoCooldown] = 5,
        [UpgradeId.Breakpoint] = 5, [UpgradeId.Show] = 3, [UpgradeId.StartingLevel] = 15
    };
    public int MaxPurchases(UpgradeId id) => Limits[id];
    public decimal Price(UpgradeId id, int nextLevel) => GameBalance.PriceFor(id, nextLevel);
    public bool CanPurchase(UpgradeId id, int currentLevel, decimal bankedBits) => currentLevel < MaxPurchases(id) && bankedBits >= Price(id, currentLevel + 1);
}

public sealed class UpgradeSystem(IStoreCatalog catalog, Dictionary<UpgradeId, int> levels) : IUpgradeSystem
{
    public IReadOnlyDictionary<UpgradeId, int> Levels => levels;
    public bool TryPurchase(UpgradeId id, ref decimal bits)
    {
        var current = levels.GetValueOrDefault(id);
        if (!catalog.CanPurchase(id, current, bits)) return false;
        bits -= catalog.Price(id, current + 1);
        levels[id] = current + 1;
        return true;
    }
}

using MinOS.Core;

namespace MinOS.Gameplay;

public interface IPatternGenerator { IReadOnlyList<PatternSet> Generate(int level, int? seed = null); }
public interface IPatternValidator { ValidationResult Submit(RunState state, ColorInput input); }
public interface IHealthSystem { int MaxHp(int hpUpgrade); int ApplyMistake(int currentHp); bool ShouldRegenerate(int completedSets, int setsInLevel, int regenUpgrade, bool regeneratedThisLevel); int Regenerate(int currentHp, int maxHp); }
public interface IScoreCalculator { decimal LevelReward(int level); double MultiplierAfterSet(double current); decimal AwardedBits(int level, double multiplier); }
public interface IStoreCatalog { int MaxPurchases(UpgradeId id); decimal Price(UpgradeId id, int nextLevel); bool CanPurchase(UpgradeId id, int currentLevel, decimal bankedBits); }
public interface IUpgradeSystem { IReadOnlyDictionary<UpgradeId, int> Levels { get; } bool TryPurchase(UpgradeId id, ref decimal bits); }
public interface IRunController { RunState State { get; } void Update(TimeSpan elapsed); ValidationResult Submit(ColorInput input); bool TryAutoComplete(); bool TryShow(); }
public interface IInputManager { IReadOnlyList<ColorInput> ReadColorEdges(); bool PausePressed { get; } }
public interface IScreenManager { string ActiveScreen { get; } void Show(string screenName); }
public interface IAudioManager { void PlayColor(ColorId color); void SetSfxVolume(float volume); }
public interface ISaveGameRepository { SaveSlotData? Load(int slot); void Save(int slot, SaveSlotData data); void Delete(int slot); }

using MinOS.Core;
using MinOS.Gameplay;
using MinOS.Input;
using MinOS.Persistence;
using MinOS.Store;
using MinOS.Rendering;
using MinOS.Audio;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace MinOS.Tests;

public sealed class CoreLogicTests
{
    [Fact]
    public void Generator_IsSeedReproducible_AndUsesLevelCount()
    {
        var generator = new PatternGenerator();
        var first = generator.Generate(15, 1234);
        var second = generator.Generate(15, 1234);
        Assert.Equal(15, first.Count);
        Assert.Equal(first, second);
        Assert.All(first, set => Assert.Equal(4, set.Colors.Count));
    }

    [Fact]
    public void Validator_IgnoresInvalidPhase_AndRestartsSetOnWrongInput()
    {
        var state = State(GamePhase.ShowingPattern);
        var validator = new PatternValidator();
        Assert.Equal(ValidationResult.Ignored, validator.Submit(state, new(ColorId.Color0)));

        var waiting = state with { Phase = GamePhase.WaitingForInput };
        var wrong = waiting.Sets[0].Colors[0] == ColorId.Color0 ? ColorId.Color1 : ColorId.Color0;
        Assert.Equal(ValidationResult.Wrong, validator.Submit(waiting, new(wrong)));
    }

    [Fact]
    public void HealthAndScore_RespectBoundaries()
    {
        var health = new HealthSystem();
        Assert.Equal(3, health.MaxHp(2));
        Assert.Equal(0, health.ApplyMistake(1));
        Assert.True(health.ShouldRegenerate(1, 15, 10, false));
        Assert.False(health.ShouldRegenerate(15, 15, 10, true));

        var score = new ScoreCalculator();
        Assert.Equal(16m, score.LevelReward(5));
        Assert.Equal(2.2m, score.AwardedBits(1, 2.2));
    }

    [Fact]
    public void Store_EnforcesLimitAffordabilityAndLinearPrice()
    {
        var catalog = new StoreCatalog();
        Assert.Equal(50, catalog.MaxPurchases(UpgradeId.InputTime));
        Assert.Equal(4m, catalog.Price(UpgradeId.InputTime, 2));
        Assert.True(catalog.CanPurchase(UpgradeId.InputTime, 0, 4m));
        Assert.False(catalog.CanPurchase(UpgradeId.InputTime, 50, 10000m));
    }

    [Fact]
    public void Bindings_DetectCaseInsensitiveConflicts()
    {
        Assert.True(BindingValidator.HasConflicts([new(ColorId.Color0, "D1"), new(ColorId.Color1, "d1")]));
        Assert.False(BindingValidator.HasConflicts([new(ColorId.Color0, "D1"), new(ColorId.Color1, "D2")]));
    }

    [Fact]
    public void SaveRepository_UsesIndependentSlots_AndDeletesOnlyCorruptSlot()
    {
        var directory = Path.Combine(Path.GetTempPath(), "min-os-tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            var repository = new SaveGameRepository(directory);
            repository.Save(1, new SaveSlotData { BankedBits = 1.5m });
            repository.Save(2, new SaveSlotData { BankedBits = 2.5m });
            Assert.Equal(1.5m, repository.Load(1)!.BankedBits);
            Assert.Equal(2.5m, repository.Load(2)!.BankedBits);
            File.WriteAllText(Path.Combine(directory, "save-1.json"), "not-json");
            Assert.Null(repository.Load(1));
            Assert.Equal(2.5m, repository.Load(2)!.BankedBits);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public void RunController_CompletesLevelAndCanAdvanceToNextLevel()
    {
        var levels = Enum.GetValues<UpgradeId>().ToDictionary(id => id, _ => 0);
        var controller = new RunController(new PatternGenerator(), new PatternValidator(), new HealthSystem(), new ScoreCalculator(), new UpgradeSystem(new StoreCatalog(), levels), debugSeed: 7);
        controller.Update(TimeSpan.FromSeconds(.1));
        controller.Update(TimeSpan.FromSeconds(4));
        Assert.Equal(GamePhase.WaitingForInput, controller.State.Phase);
        foreach (var color in controller.State.Sets[0].Colors) controller.Submit(new(color));
        Assert.Equal(GamePhase.LevelComplete, controller.State.Phase);
        Assert.Equal(1.1m, controller.State.RunBits);
        controller.StartNextLevel();
        Assert.Equal(2, controller.State.CurrentLevel);
        Assert.Equal(GamePhase.Preparing, controller.State.Phase);
    }

    [Fact]
    public void RunController_WrongInputWithBaseHpImmediatelyGameOvers()
    {
        var levels = Enum.GetValues<UpgradeId>().ToDictionary(id => id, _ => 0);
        var controller = new RunController(new PatternGenerator(), new PatternValidator(), new HealthSystem(), new ScoreCalculator(), new UpgradeSystem(new StoreCatalog(), levels), debugSeed: 1);
        controller.Update(TimeSpan.FromSeconds(.1)); controller.Update(TimeSpan.FromSeconds(4));
        var expected = controller.State.Sets[0].Colors[0];
        var wrong = expected == ColorId.Color0 ? ColorId.Color1 : ColorId.Color0;
        controller.Submit(new(wrong));
        Assert.Equal(GamePhase.GameOver, controller.State.Phase);
        Assert.Equal(0, controller.State.CurrentHp);
    }

    [Fact]
    public void PatternLayout_CentersEachRowAndWrapsAtThreeSets()
    {
        var bounds = PatternLayout.SetBounds(6, 960, 70);
        Assert.Equal(6, bounds.Count);
        Assert.Equal(bounds[0].X, bounds[3].X);
        Assert.Equal((960 - (3 * PatternLayout.SetWidth + 2 * PatternLayout.SetGap)) / 2, bounds[0].X);
        var oneRow = PatternLayout.SetBounds(1, 960, 70);
        Assert.Equal((960 - PatternLayout.SetWidth) / 2, oneRow[0].X);
    }

    [Fact]
    public void SaveRepository_PreservesActiveRunAndIndependentUpgradeData()
    {
        var directory = Path.Combine(Path.GetTempPath(), "min-os-active-" + Guid.NewGuid().ToString("N"));
        try
        {
            var repository = new SaveGameRepository(directory);
            var state = State(GamePhase.WaitingForInput) with { CurrentLevel = 4, RunBits = 6.5m };
            repository.Save(1, new SaveSlotData { BankedBits = 6.5m, ActiveRun = state, UpgradeLevels = new() { [UpgradeId.Hp] = 2 } });
            repository.Save(2, new SaveSlotData { BankedBits = 1m, UpgradeLevels = new() { [UpgradeId.Hp] = 0 } });
            var slotA = repository.Load(1)!; var slotB = repository.Load(2)!;
            Assert.Equal(4, slotA.ActiveRun!.CurrentLevel);
            Assert.Equal(2, slotA.UpgradeLevels[UpgradeId.Hp]);
            Assert.Equal(0, slotB.UpgradeLevels[UpgradeId.Hp]);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public void InputDefaults_UsePhysicalNumpadAndNumericalNumberRow()
    {
        var bindings = MonoGameInputManager.DefaultBindings();
        Assert.Equal(ColorId.Color0, bindings[Keys.NumPad7]);
        Assert.Equal(ColorId.Color6, bindings[Keys.NumPad1]);
        Assert.Equal(ColorId.Color0, bindings[Keys.D1]);
        Assert.Equal(ColorId.Color8, bindings[Keys.D9]);
    }

    [Fact]
    public void InputRemapping_RejectsConflictsAndExportsAcceptedBinding()
    {
        var input = new MonoGameInputManager();
        Assert.False(input.TryRebind(ColorId.Color0, Keys.D2));
        Assert.True(input.TryRebind(ColorId.Color0, Keys.F1));
        Assert.Contains("F1", input.ExportBindings()[ColorId.Color0]);
    }

    [Fact]
    public void RunController_BreakpointFreezesTimerAcceptsNextInputAndTracksPrefix()
    {
        var levels = Enum.GetValues<UpgradeId>().ToDictionary(id => id, _ => 0);
        var controller = new RunController(new PatternGenerator(), new PatternValidator(), new HealthSystem(), new ScoreCalculator(), new UpgradeSystem(new StoreCatalog(), levels), startingLevel: 2, debugSeed: 3);
        controller.Update(TimeSpan.FromSeconds(.1)); controller.Update(TimeSpan.FromSeconds(4));
        foreach (var color in controller.State.Sets[0].Colors) controller.Submit(new(color));
        var before = controller.State.InputSecondsRemaining;
        Assert.Equal(GamePhase.Breakpoint, controller.State.Phase);
        controller.Update(TimeSpan.FromSeconds(1));
        Assert.Equal(before, controller.State.InputSecondsRemaining);
        var next = controller.State.Sets[1].Colors[0];
        controller.Submit(new(next));
        Assert.Equal(1, controller.State.CurrentSet);
        Assert.Equal([next], controller.State.AcceptedPrefix);
        var wrong = next == ColorId.Color0 ? ColorId.Color1 : ColorId.Color0;
        controller.Submit(new(wrong));
        Assert.Empty(controller.State.AcceptedPrefix);
    }

    [Fact]
    public void MemorySequencePlayer_EmitsRepeatedColorsInOrder()
    {
        var player = new MemorySequencePlayer();
        player.Start([ColorId.Color0, ColorId.Color1, ColorId.Color1, ColorId.Color2], TimeSpan.FromSeconds(4));
        var emitted = new List<ColorId>();
        for (var i = 0; i < 4; i++) emitted.AddRange(player.Update(TimeSpan.FromSeconds(1)));
        Assert.Equal([ColorId.Color0, ColorId.Color1, ColorId.Color1, ColorId.Color2], emitted);
    }

    [Fact]
    public void StatisticsTracker_TracksCountersAndDoesNotReduceBitsForPurchases()
    {
        var tracker = new StatisticsTracker();
        tracker.RecordInput(); tracker.RecordInput(); tracker.RecordSetCompleted(); tracker.RecordGameOver(); tracker.RecordLevel(3, 4.5m);
        Assert.Equal(new StatisticsData { Inputs = 2, CompletedSets = 1, GameOvers = 1, HighestLevel = 3, TotalBitsEarned = 4.5m }, tracker.Data);
    }

    [Fact]
    public void AutoComplete_ConsumesOneChargeAndAppliesCooldown()
    {
        var levels = Enum.GetValues<UpgradeId>().ToDictionary(id => id, _ => 0);
        levels[UpgradeId.AutoComplete] = 2;
        var controller = new RunController(new PatternGenerator(), new PatternValidator(), new HealthSystem(), new ScoreCalculator(), new UpgradeSystem(new StoreCatalog(), levels), debugSeed: 9);
        controller.Update(TimeSpan.FromSeconds(.1));
        controller.Update(TimeSpan.FromSeconds(4));

        Assert.True(controller.TryAutoComplete());
        Assert.Equal(1, controller.State.AutoCompleteCharges);
        Assert.Equal(GameBalance.DefaultAutoCooldownLevels, controller.State.AutoCompleteCooldownLevels);
        Assert.False(controller.TryAutoComplete());
    }

    [Fact]
    public void AutoCooldown_DecrementsWhenAdvancingToNextLevel()
    {
        var levels = Enum.GetValues<UpgradeId>().ToDictionary(id => id, _ => 0);
        levels[UpgradeId.AutoComplete] = 1;
        var controller = new RunController(new PatternGenerator(), new PatternValidator(), new HealthSystem(), new ScoreCalculator(), new UpgradeSystem(new StoreCatalog(), levels), debugSeed: 9);
        controller.Update(TimeSpan.FromSeconds(.1));
        controller.Update(TimeSpan.FromSeconds(4));
        Assert.True(controller.TryAutoComplete());

        // Finish the remaining input normally; this also verifies Auto-Complete did not
        // silently complete more sets than its configured charge.
        while (controller.State.Phase != GamePhase.LevelComplete)
        {
            if (controller.State.Phase == GamePhase.Breakpoint) controller.Update(TimeSpan.FromSeconds(3));
            else if (controller.State.Phase == GamePhase.WaitingForInput)
            {
                foreach (var color in controller.State.Sets[controller.State.CurrentSet].Colors.Skip(controller.State.InputIndex)) controller.Submit(new(color));
            }
            else controller.Update(TimeSpan.FromSeconds(1));
        }

        controller.StartNextLevel();
        Assert.Equal(GameBalance.DefaultAutoCooldownLevels - 1, controller.State.AutoCompleteCooldownLevels);
    }

    private static RunState State(GamePhase phase) => new()
    {
        CurrentLevel = 1, CurrentSet = 0, Sets = [new PatternSet(ColorId.Color0, ColorId.Color1, ColorId.Color2, ColorId.Color3)],
        InputIndex = 0, CurrentHp = 1, MaxHp = 1, InputSecondsRemaining = 30, MemorySecondsRemaining = 3,
        Multiplier = 1, ConsecutiveSets = 0, RunBits = 0, Phase = phase, ShowUsesRemaining = 0, AutoCompleteCooldownLevels = 0
    };
}

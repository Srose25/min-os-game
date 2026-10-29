using MinOS.Core;

namespace MinOS.Gameplay;

/// <summary>Authoritative, graphics-free state machine for one run.</summary>
public sealed class RunController : IRunController
{
    private readonly IPatternGenerator generator;
    private readonly IPatternValidator validator;
    private readonly IHealthSystem health;
    private readonly IScoreCalculator score;
    private readonly IUpgradeSystem upgrades;
    private readonly int? debugSeed;
    private bool paused;

    public RunState State { get; private set; }

    public RunController(IPatternGenerator generator, IPatternValidator validator, IHealthSystem health,
        IScoreCalculator score, IUpgradeSystem upgrades, int startingLevel = 1, int? debugSeed = null)
    {
        this.generator = generator; this.validator = validator; this.health = health;
        this.score = score; this.upgrades = upgrades; this.debugSeed = debugSeed;
        State = CreateLevel(Math.Clamp(startingLevel, 1, GameBalance.MaxLevel), 0m, GameBalance.InitialMultiplier);
    }

    public void SetPaused(bool value) => paused = value;

    public void StartNextLevel()
    {
        if (State.Phase != GamePhase.LevelComplete) return;
        var next = Math.Min(GameBalance.MaxLevel, State.CurrentLevel + 1);
        State = CreateLevel(next, State.RunBits, State.Multiplier);
    }

    public void StartNewRun(int startingLevel = 1, decimal bits = 0m)
    {
        State = CreateLevel(Math.Clamp(startingLevel, 1, GameBalance.MaxLevel), bits, GameBalance.InitialMultiplier);
        paused = false;
    }

    public bool TrySpendBits(decimal amount)
    {
        if (amount < 0 || State.RunBits < amount) return false;
        State = State with { RunBits = State.RunBits - amount };
        return true;
    }

    public void Update(TimeSpan elapsed)
    {
        if (paused || elapsed <= TimeSpan.Zero) return;
        var seconds = elapsed.TotalSeconds;
        var s = State;
        switch (s.Phase)
        {
            case GamePhase.Preparing:
                State = s with { Phase = GamePhase.ShowingPattern, MemorySecondsRemaining = GameBalance.DefaultMemorySeconds + upgrades.Levels.GetValueOrDefault(UpgradeId.MemoryTime) };
                break;
            case GamePhase.ShowingPattern:
                State = s with { MemorySecondsRemaining = s.MemorySecondsRemaining - seconds };
                if (State.MemorySecondsRemaining <= 0)
                    State = State with { Phase = GamePhase.WaitingForInput, MemorySecondsRemaining = 0, PatternRevealed = false,
                        InputSecondsRemaining = GameBalance.DefaultInputSeconds + upgrades.Levels.GetValueOrDefault(UpgradeId.InputTime) };
                break;
            case GamePhase.WaitingForInput:
                var revealRemaining = s.PatternRevealed ? s.MemorySecondsRemaining - seconds : s.MemorySecondsRemaining;
                State = s with { InputSecondsRemaining = s.InputSecondsRemaining - seconds,
                    MemorySecondsRemaining = Math.Max(0, revealRemaining), PatternRevealed = s.PatternRevealed && revealRemaining > 0 };
                if (State.InputSecondsRemaining <= 0) State = State with { InputSecondsRemaining = 0, Phase = GamePhase.GameOver, CurrentHp = 0 };
                break;
            case GamePhase.Breakpoint:
                State = s with { MemorySecondsRemaining = s.MemorySecondsRemaining - seconds };
                if (State.MemorySecondsRemaining <= 0) FinishBreakpoint();
                break;
        }
    }

    public ValidationResult Submit(ColorInput input)
    {
        var result = validator.Submit(State, input);
        if (result is ValidationResult.Ignored) return result;
        if (result is ValidationResult.Wrong)
        {
            var hp = health.ApplyMistake(State.CurrentHp);
            State = State with { CurrentHp = hp, InputIndex = 0, Multiplier = GameBalance.InitialMultiplier, ConsecutiveSets = 0,
                Phase = hp == 0 ? GamePhase.GameOver : GamePhase.WaitingForInput };
            return result;
        }
        if (result is ValidationResult.Correct) { State = State with { InputIndex = State.InputIndex + 1 }; return result; }
        CompleteSet();
        return result;
    }

    public bool TryAutoComplete()
    {
        if (State.Phase != GamePhase.WaitingForInput || upgrades.Levels.GetValueOrDefault(UpgradeId.AutoComplete) <= 0 || State.AutoCompleteCooldownLevels > 0) return false;
        var count = upgrades.Levels.GetValueOrDefault(UpgradeId.AutoComplete);
        for (var i = 0; i < count && State.Phase is GamePhase.WaitingForInput or GamePhase.Breakpoint; i++)
        {
            if (State.Phase == GamePhase.Breakpoint) FinishBreakpoint();
            if (State.Phase == GamePhase.WaitingForInput) CompleteSet();
        }
        return true;
    }

    public bool TryShow()
    {
        if (State.Phase != GamePhase.WaitingForInput || State.ShowUsesRemaining <= 0) return false;
        State = State with { ShowUsesRemaining = State.ShowUsesRemaining - 1, PatternRevealed = true,
            MemorySecondsRemaining = GameBalance.DefaultMemorySeconds + upgrades.Levels.GetValueOrDefault(UpgradeId.MemoryTime) };
        return true;
    }

    private void CompleteSet()
    {
        var nextMultiplier = score.MultiplierAfterSet(State.Multiplier);
        var nextCount = State.ConsecutiveSets + 1;
        State = State with { InputIndex = 0, Multiplier = nextMultiplier, ConsecutiveSets = nextCount };
        if (State.CurrentSet == State.Sets.Count - 1)
        {
            State = State with { Phase = GamePhase.LevelComplete, RunBits = State.RunBits + score.AwardedBits(State.CurrentLevel, State.Multiplier) };
            return;
        }
        State = State with { Phase = GamePhase.Breakpoint, MemorySecondsRemaining = GameBalance.DefaultBreakpointSeconds };
    }

    private void FinishBreakpoint()
    {
        var nextSet = State.CurrentSet + 1;
        var regenLevel = upgrades.Levels.GetValueOrDefault(UpgradeId.Regen);
        var regenerated = State.RegeneratedThisLevel;
        var hp = State.CurrentHp;
        if (health.ShouldRegenerate(State.ConsecutiveSets, State.Sets.Count, regenLevel, regenerated))
        { hp = health.Regenerate(hp, State.MaxHp); regenerated = true; }
        var restore = GameBalance.BreakpointTimeRestoreSeconds(upgrades.Levels.GetValueOrDefault(UpgradeId.Breakpoint));
        State = State with { CurrentSet = nextSet, Phase = GamePhase.WaitingForInput,
            InputSecondsRemaining = Math.Min(GameBalance.DefaultInputSeconds + upgrades.Levels.GetValueOrDefault(UpgradeId.InputTime), State.InputSecondsRemaining + restore),
            CurrentHp = hp, RegeneratedThisLevel = regenerated };
    }

    private RunState CreateLevel(int level, decimal bits, double multiplier) => new()
    {
        CurrentLevel = level, CurrentSet = 0, Sets = generator.Generate(level, debugSeed), InputIndex = 0,
        CurrentHp = health.MaxHp(upgrades.Levels.GetValueOrDefault(UpgradeId.Hp)), MaxHp = health.MaxHp(upgrades.Levels.GetValueOrDefault(UpgradeId.Hp)),
        InputSecondsRemaining = GameBalance.DefaultInputSeconds + upgrades.Levels.GetValueOrDefault(UpgradeId.InputTime), MemorySecondsRemaining = 0,
        Multiplier = multiplier, ConsecutiveSets = 0, RunBits = bits, Phase = GamePhase.Preparing,
        ShowUsesRemaining = upgrades.Levels.GetValueOrDefault(UpgradeId.Show), AutoCompleteCooldownLevels = 0,
        AutoCompleteCharges = upgrades.Levels.GetValueOrDefault(UpgradeId.AutoComplete), PatternRevealed = true, RegeneratedThisLevel = false
    };
}

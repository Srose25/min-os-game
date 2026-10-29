using MinOS.Core;

namespace MinOS.Gameplay;

public sealed class PatternValidator : IPatternValidator
{
    public ValidationResult Submit(RunState state, ColorInput input)
    {
        if (state.Phase != GamePhase.WaitingForInput) return ValidationResult.Ignored;
        var expected = state.Sets[state.CurrentSet].Colors[state.InputIndex];
        return expected == input.Color
            ? state.InputIndex == GameBalance.ColorsPerSet - 1 ? (state.CurrentSet == state.Sets.Count - 1 ? ValidationResult.LevelCompleted : ValidationResult.SetCompleted) : ValidationResult.Correct
            : ValidationResult.Wrong;
    }
}

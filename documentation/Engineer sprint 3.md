# Engineer Sprint 3

This document records the changes made from `documentation/QA sprint 2.md`.

## Settings and key bindings

Location: `src/Min-OS.Game/Input/MonoGameInputManager.cs`

Changes needed: Number-row defaults were mapped in physical-numpad order, remapping was unavailable, conflicts were not reported, and bindings were not exportable for save data.

Action taken: Corrected number-row defaults to `D1-D9 -> Color0-Color8` while retaining physical numpad order. Added key-edge capture, conflict rejection, binding export/import, and a default reset so one save slot cannot leak bindings into another.

Location: `src/Min-OS.Game/Game1.cs`

Changes needed: Settings was display-only and did not persist changes.

Action taken: Added a visible click-a-color-then-press-a-key workflow, conflict feedback, binding display, and save-slot settings persistence.

## Leaderboard and statistics

Location: `src/Min-OS.Game/Core/StatisticsTracker.cs`

Changes needed: Statistics were mixed with temporary run state or displayed as placeholders.

Action taken: Added an independent cumulative tracker for inputs, completed sets, game overs, highest level, and total bits earned. Store spending does not reduce total earned bits.

Location: `src/Min-OS.Game/Game1.cs`

Changes needed: The active flow did not update or display persisted statistics.

Action taken: Wired statistic updates into gameplay input, set completion, level rewards, game over, save, load, and the Leader Board screen.

## Breakpoint gameplay

Location: `src/Min-OS.Game/Gameplay/RunController.cs`

Changes needed: Inputs during `Breakpoint` were ignored, even though the breakpoint should be an automatic handoff.

Action taken: The first next-set input resolves the breakpoint exactly once, advances the set, performs regeneration, restores breakpoint time once, and validates the input. Input countdown is not reduced while the controller is in `Breakpoint`.

## Correct-input feedback

Location: `src/Min-OS.Game/Core/Domain.cs` and `src/Min-OS.Game/Gameplay/RunController.cs`

Changes needed: Only the input index changed after a correct input, so the accepted prefix was not available to rendering.

Action taken: Added immutable `AcceptedPrefix` state, append-on-correct behavior, and reset-on-wrong/set-completion behavior. The game renderer now shows the entered color-number prefix.

## Button visual feedback and gameplay controls

Location: `src/Min-OS.Game/Game1.cs`

Changes needed: Keypad buttons were always full brightness and Auto-Complete/Show had no visible controls.

Action taken: Added a darker idle state and a short keyboard/mouse-independent highlight after accepted input. Added visible `A: Auto-Complete` and `S: Show` controls plus matching mouse hit regions. Feedback is presentation-only and does not affect timing or validation.

## Sequential memory audio

Location: `src/Min-OS.Game/Audio/MemorySequencePlayer.cs`

Changes needed: All memory colors were sent to audio immediately, causing overlapping playback and no deterministic sequence seam.

Action taken: Added a deterministic, non-authoritative sequence player that emits one color at a time in pattern order, including repeated colors and set boundaries. The full pattern remains visible while the sequence runs.

Location: `src/Min-OS.Game/Game1.cs` and `src/Min-OS.Game/Audio/AudioManager.cs`

Action taken: Wired the sequence player to the nine generated placeholder tones and kept accepted-input audio separate from memory-sequence audio.

## Automated verification

Location: `src/Min-OS.Tests/CoreLogicTests.cs`

Action taken: Added coverage for numerical number-row mappings, physical numpad mappings, remapping conflicts, breakpoint timer/input behavior, accepted-prefix reset, repeated-color sequence ordering, and cumulative statistics.

## Verification evidence

Commands executed:

```powershell
dotnet test Min-OS.sln --no-restore -m:1 -v:minimal
dotnet build Min-OS.sln --no-restore -m:1 -v:minimal
```

Results: 17 tests passed; build succeeded with 0 warnings and 0 errors.

After the bounded game launch, the default test output was temporarily rejected by the host's Windows Application Control policy. Re-running the tests into a fresh output directory passed with:

```powershell
dotnet test src\Min-OS.Tests\Min-OS.Tests.csproj --no-restore -p:OutputPath=C:\Users\stock\Min-OS\test-output\ -p:IntermediateOutputPath=C:\Users\stock\Min-OS\src\Min-OS.Tests\obj\Debug\net10.0\ -m:1 -v:minimal
```

Result: 17 tests passed. The generated temporary output directory was removed after verification.

A bounded launch check started the MonoGame executable successfully and observed the process alive after four seconds before stopping it. Full interactive mouse QA was not performed through the automation environment; the next manual pass should exercise Start Game, all three save slots, Settings capture/conflict feedback, breakpoint input, Game Over, Store, and Leader Board.

## Remaining issue

The Sprint 2 report requested nine generated placeholder files under `Content/Audio` compiled by Content Builder. This implementation uses generated `SoundEffect` PCM tones in `AudioManager` so the game can run without external assets. The audio contract is stable, but replacing the runtime tones with nine `.wav` Content Builder assets remains a packaging follow-up.

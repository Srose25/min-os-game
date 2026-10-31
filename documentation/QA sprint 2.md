# QA Sprint 2 Review

## QA verdict

Engineer Sprint 2 is partially complete and is not approved for Version 0 yet.

The automated suite currently passes 12 tests, and the project builds successfully with `--no-restore`. Save-slot selection, centered pattern layout, corrected text layering, placeholder audio generation, and some persistence behavior were added.

The report also acknowledges that settings remapping and persisted statistics remain unfinished. The following items are Version 0 blockers.

## Required Engineer fixes

### 1. Settings and key bindings

The Settings screen remains display-only. It has no remapping workflow, key-capture mode, conflict feedback, or persisted binding changes.

The current number-row mapping follows the physical numpad layout:

```text
D7 -> Color0, D8 -> Color1, D9 -> Color2
D4 -> Color3, D5 -> Color4, D6 -> Color5
D1 -> Color6, D2 -> Color7, D3 -> Color8
```

This makes the number-row labels disagree with their numerical values.

Required changes:

- Keep the numpad mapping in physical keypad order.
- Make the default number-row mapping numerical: `1–9 -> Color0–Color8`.
- Allow both keyboard mappings to coexist unless an actual binding conflict exists.
- Add remapping for all nine color actions.
- Persist bindings in `SettingsData`.
- Add a visible key-capture workflow and conflict error message.
- Add tests for number-row mapping, numpad mapping, remapping, conflicts, persistence, and held-key behavior.

### 2. Leaderboard and statistics

The leaderboard still displays placeholder or incorrect values:

- Completed Sets uses the current run's consecutive-set count.
- Game Overs is hardcoded to `0`.
- Inputs is blank.
- Total Bits Earned displays current run bits instead of cumulative earned bits.
- Statistics are not saved through the active game flow.

Required changes:

- Track statistics independently from temporary run state.
- Increment Completed Sets whenever a set is completed.
- Increment Inputs for every submitted gameplay input, including wrong inputs.
- Increment Game Overs when a run ends.
- Update Highest Level whenever a new level record is reached.
- Add level rewards to cumulative Total Bits Earned; store purchases must not reduce this statistic.
- Save and restore statistics per save slot.
- Display the selected save slot's statistics.
- Add automated tests for every counter and persistence across restart.

### 3. Breakpoint gameplay

The current controller enters `GamePhase.Breakpoint`, while the validator ignores input outside `WaitingForInput`. Therefore, the player cannot make the next input during the breakpoint.

The breakpoint should freeze the timer while still allowing gameplay to continue.

Required changes:

- Freeze the input countdown during the breakpoint.
- Accept input for the next set during the breakpoint.
- Keep the breakpoint automatic and non-blocking.
- Restore the configured breakpoint time only once.
- Prevent duplicate set advancement when the breakpoint timer expires.
- Add tests proving input acceptance, frozen countdown, correct set advancement, regeneration, and timer restoration.

### 4. Correct-input feedback

After a correct input, only `InputIndex` changes. The current pattern does not visibly show the correctly entered prefix.

Required changes:

- Track the accepted input prefix for the current set.
- Render accepted colors visibly while unanswered colors remain hidden.
- Reset the visible prefix after a wrong input restarts the set.
- Add automated tests for prefix progression and reset behavior.

### 5. Button visual feedback

The nine game buttons are currently always rendered at full brightness.

Required changes:

- Render buttons in a darker idle state.
- Highlight the corresponding button after keyboard or mouse input.
- Keep the feedback visible long enough to be perceived.
- Use the same behavior for keyboard and mouse.
- Ensure visual feedback does not affect gameplay timing or validation.

### 6. Sequential memory audio

The current implementation calls `PlayColor` for every color immediately when memory display begins. That is not a Simon-style sequence and may cause the sounds to overlap.

Required changes:

- Play one color sound at a time in pattern order.
- Synchronize each sound with the corresponding visual tile highlight.
- Include repeated colors and set boundaries.
- Keep audio playback non-authoritative.
- Keep the full memory pattern visible while the sequence plays.
- Add a deterministic test using a fake audio service or event recorder.

## Acceptance evidence required for the next report

The next Engineer report must include:

- Exact build and test commands with results.
- Manual test steps and outcomes.
- Reproduction details for any remaining failures.
- Severity for each unfinished item.
- Evidence that the game was launched and exercised through Start Game, save-slot selection, gameplay, breakpoints, Game Over, Store, and Leaderboard.

## Approval status

Build/test status is currently green, but gameplay acceptance has failed. Version 0 approval is blocked until settings remapping, statistics, breakpoint input flow, correct-input feedback, button feedback, and sequential memory audio are implemented and verified.

# QA Sprint 3 Review

## Verdict

Sprint 3 is improved and passes the reported automated build/test checks, but it is not yet approved for Version 0.

Verified from the current workspace:

- `dotnet build Min-OS.sln --no-restore -m:1 -v:minimal` succeeds with 0 warnings and 0 errors.
- The automated suite passes 17 tests according to the Engineer report and current verification.
- Number-row defaults are now numerical while numpad defaults remain in physical keypad order.
- Binding export/import, conflict rejection, and reset behavior were added.
- Statistics are separated from temporary run state and are displayed from the tracker.
- Breakpoint input, accepted-prefix rendering, button highlighting, and sequential memory-sequence infrastructure were added.

The remaining issues below must be addressed or explicitly accepted before Version 0 approval.

## Blocking findings

### 1. Breakpoint behavior still needs manual acceptance

The new `RunController.Submit` behavior resolves a breakpoint immediately by calling `FinishBreakpoint()` and then validating the input. This is directionally correct, but the implementation still needs an end-to-end test proving that the first input during a breakpoint is accepted as the first input of the next set and that the breakpoint cannot advance twice.

Required verification:

- Complete a set and confirm the next set is ready during the breakpoint.
- Enter the first color before the breakpoint timer expires.
- Confirm the input timer remains frozen while waiting.
- Confirm the next input index is `1` after the accepted first color.
- Allow the breakpoint timer to expire and confirm the same set is not skipped or advanced twice.
- Confirm breakpoint regeneration and time restoration occur once only.
- Test both keyboard and mouse input during the breakpoint.

The existing automated test is useful but does not replace the required interactive pass.

### 2. Save-slot and Settings integration needs boundary testing

The report claims settings persistence, but the current flow allows the Settings screen to be opened before a save slot is selected. In that state, `SaveCurrentRun()` has no selected slot and cannot persist the change.

Required decision and fix:

- Either require a save slot to be selected before editing settings, or define a separate global-settings save path.
- Verify that settings changed in slot 1 do not appear in slot 2 or slot 3.
- Verify that a new empty slot receives defaults rather than bindings from a previously loaded slot.
- Verify that a loaded slot restores every binding and volume setting.
- Verify that conflict feedback leaves the previous valid binding unchanged.

### 3. Manual gameplay QA is still incomplete

The Engineer report explicitly states that full interactive mouse QA was not performed. This means the following acceptance criteria remain unverified:

- Start Game and all three save slots.
- Empty, occupied, and corrupt save-slot behavior.
- Settings capture and conflict feedback.
- Mouse color input and mouse controls for Auto-Complete and Show.
- Breakpoint input and timer behavior.
- Game Over, Store, and Leader Board transitions.
- Pause/resume timing.
- Visual button feedback and memory-sequence presentation.

The next report must include manual reproduction steps and observed results for each item.

## Non-blocking but required before final packaging

### 4. Placeholder audio packaging deviation

The report says the nine tones are generated at runtime using `SoundEffect` PCM data rather than being nine audio files compiled through MonoGame Content Builder.

This is acceptable as a temporary runtime implementation only if explicitly approved. The project requirements and Sprint 1 instructions requested nine replaceable placeholder assets under `Content/Audio`.

Required follow-up:

- Either add nine `.wav` placeholder assets to the Content Builder pipeline, or document and approve runtime-generated tones as the Version 0 implementation.
- Preserve the `ColorId` to sound mapping.
- Verify that the game starts and plays tones on the target machine with and without an available audio device.
- Verify SFX volume persistence and mute behavior.

### 5. Memory sequence test coverage

`MemorySequencePlayer` has deterministic ordering coverage, which is good. However, tests should also verify integration behavior:

- Audio events are emitted in the same order as visible tile highlights.
- Repeated colors are not collapsed.
- All set boundaries are preserved.
- The sequence does not alter timers or validation state.
- Pause stops the sequence and resumes it without skipping colors.

## Regression checks that passed

- Number-row mapping is numerical: `D1–D9 -> Color0–Color8`.
- Numpad mapping remains physical: `NumPad7–NumPad1` maps to the 3x3 keypad arrangement.
- Binding conflict rejection is present.
- Accepted input prefixes are reset after wrong input and set completion.
- Statistics tracking is independent of run bits and store spending.
- Centered pattern layout and text foreground/drop-shadow changes remain present.
- Build completed with no warnings or errors.

## Additional source-level observations

- The `TryRebind` implementation removes all existing aliases for a color when one replacement key is assigned. Confirm this is the intended UX; if remapping should preserve both numpad and number-row aliases, the binding model needs separate aliases per input device.
- `DefaultKey` and the input defaults now agree, but the manual UI should visibly show both the numpad and number-row bindings.
- Statistics are updated from `Game1`; add a focused integration test to ensure the same event is not counted twice after a level-complete or game-over screen transition.
- The report says a bounded launch succeeded, but this is not a substitute for the required manual interaction pass.

## Approval status

Automated verification: **Pass**.

Version 0 gameplay acceptance: **Pending**.

Approval requires completion of the breakpoint end-to-end test, save/settings boundary testing, and the documented manual QA pass. The audio packaging deviation must also be approved or replaced with Content Builder placeholder assets.

# Engineer Sprint 2

This document records the implementation changes made from `QA sprint 1.md`.

## Save-slot selection and persistence

Location: `src/Min-OS.Game/Game1.cs`

Changes needed: Start Game previously entered gameplay immediately. Save slots, active-run loading, permanent upgrades, banked bits, and corrupt-slot messaging were not wired into the screen flow.

Action taken: Added a three-slot selection screen, empty/available/corrupt slot states, occupied-slot loading, fresh level-1 initialization for empty slots, restoration of active runs and upgrade levels, and save calls on level completion, game over, pause-to-menu, and slot selection.

Location: `src/Min-OS.Game/Persistence/SaveGameRepository.cs`

Changes needed: The UI needed a safe way to distinguish missing files from existing files that became corrupt.

Action taken: Added `Exists(int slot)` while preserving the existing versioned JSON loading and affected-slot deletion behavior.

Location: `src/Min-OS.Game/Gameplay/RunController.cs`

Changes needed: A loaded active run needed to be restored without sharing mutable upgrade dictionaries or creating a new pattern.

Action taken: Added `initialState`, `LoadState`, and explicit fresh-run initialization APIs. New runs receive a new state and loaded runs replace the state with the saved snapshot.

## Audio placeholder behavior

Location: `src/Min-OS.Game/Audio/AudioManager.cs`

Changes needed: No color sound effects existed, and gameplay had no audio service to call.

Action taken: Added nine short, distinct generated placeholder tones, volume clamping, zero-volume mute behavior, and a non-authoritative `PlayColor` mapping. These tones are temporary and can be replaced by Content Builder assets without changing gameplay code.

Location: `src/Min-OS.Game/Game1.cs`

Changes needed: Accepted keyboard/mouse inputs and memory display did not trigger color sounds.

Action taken: Wired the audio service to accepted color inputs and the beginning of pattern display. Audio playback does not control validation or timers.

## Text rendering correction

Location: `src/Min-OS.Game/Rendering/UiStyle.cs`

Changes needed: Text foreground and shadow layers were reversed and colors were duplicated in screen code.

Action taken: Added centralized UI colors: white foreground, darker blue shadow, and a consistent two-pixel offset.

Location: `src/Min-OS.Game/Game1.cs`

Changes needed: `DrawText` drew blue text over the background with white behind it.

Action taken: Draws the dark shadow first and white text second. Unsupported SpriteFont glyphs are replaced with `?` so an invalid label cannot crash the render loop.

## Pattern layout correction

Location: `src/Min-OS.Game/Rendering/PatternLayout.cs`

Changes needed: Pattern rows were hard-coded from `x = 40` and were left-aligned.

Action taken: Added reusable centered bounds calculation with a maximum of three sets per row. Rows are independently centered and support levels 1, 3, 6, and 15.

Location: `src/Min-OS.Game/Game1.cs`

Changes needed: Rendering owned pattern positioning and did not use a layout abstraction.

Action taken: Updated pattern rendering to consume `PatternLayout` bounds and retain breakpoint markers and Cached Memory opacity behavior.

## Automated coverage added

Location: `src/Min-OS.Tests/CoreLogicTests.cs`

Action taken: Added tests for centered row layout, active-run persistence, banked bits, and independent upgrade data between save slots. Existing tests continue to cover generation, validation, health, score, store rules, bindings, and corrupt-slot isolation.

## Remaining follow-up

Settings remapping UI, persisted statistics, and actual replacement audio files still need a dedicated pass. The current audio implementation intentionally uses generated tones so QA can verify the nine-color sound behavior before final assets are supplied.

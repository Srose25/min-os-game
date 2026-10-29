# Min-OS Version 0 — Architecture and Agent Work Plan

## 1. Project definition

Min-OS is a small, keyboard-first 2D MonoGame game inspired by Simon Says and roguelike progression. The player memorizes a row of colored squares, reproduces the sequence with configurable keyboard or mouse inputs, earns bits by completing levels, buys permanent upgrades, and attempts to reach level 15.

Version 0 is a working local build for the developer's current machine. It does not need release packaging, touch support, music, crash recovery, tutorials, infinite level scaling, telemetry, or replay recording.

## 2. Version 0 decisions

These are the current source-of-truth rules.

### Pattern display

- A level contains as many sets as its level number: level 1 has one set, level 15 has fifteen sets.
- Every set contains exactly four colored inputs.
- The pattern is displayed in horizontal rows.
- A row contains no more than three sets.
- A bullet/asterisk-style breakpoint separates sets.
- A pattern with six sets is displayed as two rows of three sets.
- A pattern with fifteen sets is displayed as five rows of three sets.
- During memory time, the full pattern is visible.
- During input time, the memory pattern becomes invisible.
- The player reproduces colors, not positions.
- Patterns are genuinely random. Consecutive and repeated colors are allowed, including across set boundaries.
- A debug seed must be supported, but normal player runs use random seeds.

### Timing and phases

- Memory Time is the duration for which the complete pattern remains visible before input begins.
- Input Time is one countdown for the entire level, not one timer per set.
- Breakpoints do not stop the level or require confirmation. They provide a short regrouping moment and restore a configured amount of input time.
- Input during memory display or any other invalid phase is ignored without penalty.
- The game uses explicit phases: `Preparing`, `ShowingPattern`, `WaitingForInput`, `Breakpoint`, `LevelComplete`, and `GameOver`.
- A wrong input removes one HP and restarts the current set. It resets the multiplier.
- Completing a set advances to the next set through a breakpoint.
- Completing a level sends the player to the store while preserving the current run.
- Losing sends the player to Game Over and then the store before the next run.
- The player can save and quit a run at any time, with no crash recovery requirement.

### HP and regeneration

- Base maximum HP is 1, making the first wrong guess an immediate game over.
- HP is a mistake counter presented as health.
- Base regeneration occurs after completing an entire level.
- The Regen upgrade reduces the number of completed sets required before the next regeneration.
- The final Regen level allows regeneration after one completed set.
- Regeneration occurs at a breakpoint after the required number of completed sets.
- HP regeneration cannot exceed maximum HP.

### Scoring

- Bits are awarded when a level is completed.
- Base level reward doubles each level: level 1 = 1 bit, level 2 = 2, level 3 = 4, level 4 = 8, and so on.
- The multiplier is based on consecutively completed sets and carries across levels during a run.
- Completing a level does not itself increase the multiplier.
- A wrong input resets the multiplier because it removes HP.
- Auto-Complete does increase the multiplier as if the sets were completed normally.
- Fractional bits remain in the score and carry forward, even though store prices are whole numbers.
- Bits are permanently saved when the player completes/settles a level or game-over result. Unsaved progress may be lost if the game closes.
- The exact multiplier display and base multiplier start at `1.0x`; the initial increment is `0.1x` per completed set.

### Store and upgrades

- Store purchases are permanent within a save file.
- Purchases take effect immediately and remain active until a new game is started or the save file is deleted. The implementation should interpret “new game” as a new run, while preserving permanent purchase levels.
- Players may buy as many available upgrades as they can afford between levels.
- Current purchase limits are:

| Upgrade | Maximum purchases | Version 0 behavior |
|---|---:|---|
| Input Time | 50 | Adds input time to the level timer |
| Memory Time | 20 | Adds memory display time |
| HP | 25 | Increases maximum HP |
| Regen | 10 | Reduces sets required between HP regeneration |
| Cached Memory | 10 | Keeps early sets visible at low opacity |
| Auto-Complete | 5 | Increases sets completed by one Auto-Complete charge |
| Auto Cooldown | 5 | Reduces level cooldown between Auto-Complete uses |
| Breakpoint | 5 | Adds time to the input timer at each breakpoint |
| Show | 3 | Adds one current-pattern reveal use per level |
| Starting Level | 15 maximum | Changes the level at which a run starts |

- Prices use an increasing-price model. Exact price curves are a balance task and must be isolated in `StoreCatalog`/`GameBalance`.
- Starting Level is the only upgrade that can be adjusted downward after purchase. It is expensive and can be lowered if a later level is too difficult.
- Cached Memory works forward: the earliest purchased number of sets remains visible at low opacity during input. Cached sets are not removed from the pattern; they are visual assistance.
- Auto-Complete is usable only during input. One charge completes a number of sets equal to the Auto-Complete upgrade level, beginning with the current set. It awards normal bits/multiplier credit and advances through breakpoints as appropriate.
- Auto-Complete has a cooldown measured in levels. Auto Cooldown reduces the number of levels before it can be used again. The final upgrade permits use once per level.
- Show reveals the pattern again beginning at the current set. The first purchase grants one use per level; further purchases add one use per level.
- Upgrade effects are additive, not multiplicative.

## 3. Version 0 screens and styling

The version 0 screen set is:

- Start Menu.
- Settings Menu.
- Leader Board screen.
- Game screen.
- Store Menu.
- Game Over screen.

All pages center content with generous horizontal padding. Version 0 transitions are instant.

### Fonts

- Header font: Wallpoet if it can be legally and technically included through MonoGame content. If not, select a visually similar bundled font.
- Body font: Arial or a suitable bundled substitute.
- Do not depend on HTML/CSS or Google Fonts at runtime.

### Global visual style

- Main background: `#28517A`.
- Main text: `#4372A1`.
- Text border: white, thin but visible.
- All screen drawing should use a central layout helper so spacing and scaling remain consistent.

### Screen contents

Start Menu: Min-OS title, Start Game, Settings, Leader Board, Quit.

Settings: nine remappable color-input bindings, Music Volume, SFX Volume. Touch and gamepad settings are out of scope.

Leader Board/statistics: Completed Sets, Number of Game Overs, Number of Inputs, Highest Level, Total Bits Earned. Display the values in a table.

Store: display each upgrade in a table, bold text, current purchase count, maximum purchase count, price, and an incremental purchase slider/indicator. Store colors are background `#3B3DA1`, text `#3337F5`, white text border.

Game Over: Game Over header, Final Score, Try Again, Quit to Menu. Use background `#8630A6`, text `#A11B57`, and a white border.

Game screen: memory line in the top third, nine-button 3x3 game pad in the lower two thirds, and HUD information for level, timer, HP, multiplier, and bits. The pattern line uses colored square visuals and bullet breakpoints.

The button art may initially be regular squares. The future button PNG should be hue-tinted into the nine colors. The supplied palette currently contains eight explicit colors:

```text
#AE00FF  #FF00FF  #0000FF  #00FFFF
#00FF00  #FFFF00  #FF7300  #FFFFFF
```

Version 0 must add/choose a ninth distinguishable color and confirm the final palette during visual QA. Do not rely on color alone: use labels, shapes, or another clear identifier where feasible.

## 4. Input and controls

- Primary gameplay input is configurable keyboard input.
- Default numpad mapping follows the physical keypad:

```text
NumPad 7  NumPad 8  NumPad 9
NumPad 4  NumPad 5  NumPad 6
NumPad 1  NumPad 2  NumPad 3
```

- The keyboard number row is also supported.
- Mouse clicks on the nine on-screen buttons are supported.
- Touch is deferred.
- Menus and the store are mouse-controlled.
- Escape pauses the game. The pause menu is mouse-controlled.
- Holding a key never generates repeated color inputs. Only key-down edges create inputs.
- Num Lock should not be required. The engineer must verify the actual MonoGame/Windows key behavior. If the target environment cannot provide usable numpad input without Num Lock, display a warning and document the limitation.
- Settings must allow each of the nine color actions to be remapped. Binding conflicts must be detected and reported before saving.
- Invalid-phase inputs are ignored.

## 5. Technical architecture

Use one MonoGame executable project and one test project initially. Keep the implementation lightweight and avoid an ECS or external UI framework.

```text
src/
├─ Min-OS.Game/
│  ├─ Game1.cs
│  ├─ Core/
│  ├─ Input/
│  ├─ Gameplay/
│  ├─ Screens/
│  ├─ Rendering/
│  ├─ Audio/
│  ├─ Persistence/
│  └─ Content/
└─ Min-OS.Tests/
```

Use the newer MonoGame Content Builder workflow. Keep balance values in C# records/classes for version 0, not external JSON.

### Responsibilities

`Game1` owns MonoGame services, loads content, polls input once per update, and delegates to the active screen.

`ScreenManager` owns the active screen and performs instant screen replacement. A simple screen stack may be added only for pause/modal behavior.

`InputManager` converts keyboard and mouse state into semantic actions and edge-triggered color input events.

`RunController` owns phase transitions, timers, level progression, pattern access, HP, regeneration, set completion, Auto-Complete, Show, scoring, and run results.

`PatternGenerator` creates truly random patterns and accepts a debug seed.

`PatternValidator` validates ordered color input and reports `Ignored`, `Correct`, `Wrong`, `SetCompleted`, and `LevelCompleted` results.

`StoreCatalog` and `UpgradeSystem` own upgrade definitions, purchase limits, price curves, and modifier calculation.

`HealthSystem` owns max HP, current HP, regeneration thresholds, and breakpoint regeneration.

`Rendering` receives read-only view models and never mutates gameplay state.

`AudioManager` plays SFX only. Music is deferred, but the abstraction should support it later.

`SaveGameRepository` owns three save slots, serialization, settings, progression, and safe writes.

## 6. Timing and update loop

- Use MonoGame's `Update(GameTime)` as the simulation boundary.
- Poll input once per frame.
- Use edge detection for color actions.
- Use `GameTime.ElapsedGameTime` for countdowns.
- Prefer `IsFixedTimeStep = true` with a 60 Hz update.
- Stop all gameplay timers while paused.
- Ignore input during memory, transition, and other invalid phases.
- Breakpoint time is short and automatic; there is no confirmation screen.
- Memory rendering is a presentation concern, but the pattern data must remain unchanged and authoritative.

## 7. Data model

Use plain C# types without MonoGame graphics references:

```csharp
enum ColorId { Color0, Color1, Color2, Color3, Color4, Color5, Color6, Color7, Color8 }
readonly record struct ColorInput(ColorId Color);
readonly record struct PatternSet(ColorId A, ColorId B, ColorId C, ColorId D);
```

`RunState` contains current level, current set, all generated sets for the level, input index, HP, timers, multiplier, run bits, consecutive sets, upgrade modifiers, Auto-Complete cooldown/charges, Show uses, and current phase.

Level pattern generation is `setsInLevel = level`, with exactly four colors per set. Level 15 therefore has 15 sets displayed across five rows. The implementation must support row wrapping at three sets per row.

## 8. Persistence model

There are three save slots. A save slot stores:

- Permanent store purchase levels.
- Banked bits, including fractional bits.
- Starting level.
- Settings and key bindings.
- Volume values.
- Leaderboard/statistics values.
- Any active run state needed to support saving and quitting at any time.

The active run should be saved when the player explicitly saves/quits or leaves a safe screen. Crash recovery is not required.

Use a versioned `System.Text.Json` DTO. Store files in the platform-appropriate application-data directory. At the title flow, allow the player to select an existing slot or create a new slot. If a save is corrupted, delete that slot in version 0 and show a clear message.

Write through a temporary file and replace the target only after serialization succeeds. Do not perform save I/O every frame.

## 9. Audio

- No music in version 0.
- Each of the nine colors has its own sound effect asset.
- Play the corresponding sound on accepted color input and during pattern display when appropriate.
- SFX can be disabled or volume-adjusted independently.
- Audio timing is not authoritative for gameplay.
- Build `AudioManager` so music can be added later without changing screens.

## 10. Debug and QA requirements

The debug build must support:

- Fixed random seed.
- Starting at a chosen level.
- Granting bits.
- Unlocking or setting upgrade levels.
- Skipping timers.
- Revealing the hidden pattern.
- Logging phase transitions, input events, score changes, and save operations.

Automated tests should cover pure game logic. The QA agent may choose a suitable .NET test framework, with xUnit, NUnit, or MSTest acceptable if compatible with the installed .NET SDK.

Minimum automated coverage:

- Pattern generation count, size, randomness injection, and seed reproducibility.
- Ordered validation and set restart after errors.
- Ignored invalid-phase input.
- HP loss/game over.
- Breakpoint timer restoration and regeneration thresholds.
- Consecutive-set multiplier across levels and reset after HP loss.
- Level reward doubling and fractional currency preservation.
- Auto-Complete and cooldown rules.
- Show usage per level.
- Cached Memory visibility rules.
- Store affordability, limits, price growth, and Starting Level adjustment.
- Three save slots, serialization, missing files, and corrupt-file handling.
- Key edge detection and no repeat-on-hold behavior.

Manual QA must verify actual numpad behavior without Num Lock, mouse input, remapping, pause timer behavior, layout scaling, all six screens, the nine-color palette, audio toggles, and save-slot selection.

## 11. Agent instructions for version 0

### The Architect

The Architect owns the technical contract and integration boundaries.

Deliver:

1. A final project/solution structure compatible with the installed .NET SDK and MonoGame template.
2. The C# domain model and interfaces for `RunController`, `PatternGenerator`, `PatternValidator`, `HealthSystem`, `ScoreCalculator`, `StoreCatalog`, `InputManager`, `ScreenManager`, `AudioManager`, and `SaveGameRepository`.
3. A written state/phase transition diagram matching the rules in this document.
4. A data contract for three save slots, active-run persistence, settings, key bindings, and statistics.
5. A definition of the view models passed into renderers.
6. A balance configuration containing the version 0 defaults, with all uncertain price formulas isolated.
7. Integration notes for the Engineer and test seams for the QA agent.

Constraints:

- Do not introduce an ECS or a large UI dependency.
- Keep game logic independent of MonoGame graphics types.
- Resolve the ninth palette color before implementation is considered complete.
- Treat all rules in Section 2 as authoritative.
- Mark any remaining ambiguity as a documented assumption rather than silently changing behavior.

Acceptance criteria:

- The Engineer can implement screens and gameplay without inventing rules.
- The QA agent can instantiate and test gameplay logic without opening a game window.
- Save data has a clear version and slot schema.

### The Engineer

The Engineer implements the playable version 0 in vertical slices.

Order of work:

1. Create the MonoGame project using the newer Content Builder workflow and the installed .NET SDK.
2. Implement `Game1`, content loading, `SpriteBatch`, fixed update settings, and the screen manager.
3. Implement input polling, keyboard edge detection, default numpad/number-row bindings, mouse buttons, remapping, and pause.
4. Implement the pattern generator, row wrapping, memory display, input phase, set validation, breakpoints, wrong-input restart, HP, timers, and level progression.
5. Implement the HUD and the six version 0 screens using instant transitions.
6. Implement scoring, fractional bits, level reward doubling, multiplier carry-over/reset, and game-over flow.
7. Implement the store and all listed upgrades, including Cached Memory, Show, Auto-Complete, Auto Cooldown, Regen, Breakpoint, and Starting Level.
8. Implement three save slots, active-run save/quit, settings persistence, statistics, and corrupt-save deletion.
9. Add nine color SFX, volume controls, and the audio manager. Do not add music.
10. Add the debug controls and logging required by Section 10.

Implementation rules:

- Never poll device input from screens directly.
- Never mutate gameplay state from draw methods.
- Never use held-key state to submit repeated color inputs.
- Keep timers and scoring deterministic and injectable for tests.
- Use read-only render view models.
- Use temporary-file replacement for save writes.
- Keep external data files out of balance configuration for version 0.

The Engineer should provide a runnable build and a short implementation note after each vertical slice, including known assumptions and any deviation from this plan.

### The QA

The QA agent owns test design, automated coverage, and manual acceptance.

Deliver:

1. Select and configure a compatible .NET test framework.
2. Build the automated tests listed in Section 10.
3. Create deterministic fixtures using fixed random seeds and fake clocks/input sources.
4. Test gameplay state transitions without depending on rendering.
5. Test all store upgrades at minimum, maximum, and boundary values.
6. Test three save slots, missing saves, corrupt saves, persistence, and active-run save/quit.
7. Test keyboard edge behavior, number-row fallback, mouse input, remapping conflicts, and Num Lock behavior on the developer machine.
8. Execute a manual pass over every screen and the full run loop from Start Menu through level completion, store, game over, and restart.
9. Verify layout, fonts, colors, text borders, row wrapping, opacity for Cached Memory, and the ninth color.
10. Verify pause stops timers and invalid-phase input is ignored.
11. Verify the debug mode can reproduce a late-game run using a seed.

Report bugs with:

- Reproduction steps.
- Expected behavior from this plan.
- Actual behavior.
- Save slot/seed/level if relevant.
- Severity and whether it blocks version 0.

Version 0 is complete when all blocking automated tests pass, all six screens are usable, the core run can reach level 15, store progression persists, and no known blocker remains in the required local environment.

## 12. Version 0 milestones

### Milestone 1 — Project and input

- MonoGame project and Content Builder compile.
- Screen manager and Start Menu exist.
- Default bindings, number row, mouse, remapping, and edge detection work.
- Settings can be displayed and saved.

### Milestone 2 — Core run

- Level 1 pattern displays as one four-color set.
- Memory pattern disappears before input.
- Input validation, HP loss, set restart, breakpoint, and game over work.
- Nine colored buttons and color SFX work.

### Milestone 3 — Levels and score

- Levels 1–15 generate the correct number of sets.
- Rows wrap after three sets.
- Input time spans the level.
- Breakpoints restore time and perform regeneration checks.
- Bits, multiplier, level completion, and game-over settlement work.

### Milestone 4 — Store and persistence

- Store purchases work between levels and after game over.
- All upgrades work within their purchase limits.
- Three save slots, active-run save/quit, statistics, and corrupted-save deletion work.

### Milestone 5 — QA and handoff

- Automated tests pass.
- Manual QA passes the required environment.
- Debug mode can reproduce issues.
- README and run instructions are present.
- Version 0 is playable from a clean checkout/build.

## 13. Explicitly deferred

The following are not version 0 blockers:

- Tutorial.
- Touch input.
- Gamepad input.
- Music.
- Animated screen transitions.
- Crash recovery.
- Graceful save corruption recovery beyond deleting the slot.
- Release packaging and cross-platform support.
- Infinite level scaling after level 15.
- Telemetry, replay recording, and online leaderboards.
- External JSON balance editing.

## 14. Open implementation assumptions

The answers provide the design direction but leave a few implementation details for the Architect to document and the QA agent to validate:

- The exact increased-price formula for each store item.
- The exact ninth color in the palette.
- The exact number of seconds restored by each breakpoint level.
- The precise representation of Auto-Complete cooldown in saved state.
- The exact save-file directory selected for the installed local environment.
- The exact fallback font if Wallpoet cannot be bundled.
- The exact display format for fractional bits.

These are tuning or integration decisions, not reasons to delay the first playable slice. They must be centralized, documented, and covered by tests once chosen.

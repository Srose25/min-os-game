# Min-OS — MonoGame Architecture and Engine Plan

## 1. Purpose

Min-OS is a small, keyboard-first 2D game built with the MonoGame Framework. Its core loop is a roguelike version of Simon Says:

1. The game shows a sequence of colored squares.
2. The player reproduces the sequence using the numeric keypad.
3. A pattern is divided into sets of four color inputs, with a breakpoint between sets.
4. Correct sets award bits and increase the run multiplier.
5. Wrong inputs remove HP; running out of time or HP ends the run.
6. After game over, the player spends accumulated/progression currency in a store before starting another run.

The architecture should be intentionally small. It should support the current game without committing the project to a general-purpose engine, entity-component-system, or large dependency framework.

## 2. MonoGame assumptions and research notes

MonoGame provides the application loop and the low-level game services needed here:

- `Game.Initialize`/`LoadContent` are used for startup and asset loading.
- `Game.Update(GameTime)` is the authoritative place for input polling and simulation.
- `Game.Draw(GameTime)` renders the current state.
- `Keyboard.GetState()` supplies a snapshot of keyboard state; edge-triggered actions should compare current and previous states.
- `SpriteBatch` is sufficient for the square grid, HUD, menus, and store UI.
- The Content Pipeline/Content Builder should compile textures, fonts, and sound effects for runtime loading through `ContentManager`.

For a new project, use the current MonoGame Content Builder workflow where practical. Older projects may use an `.mgcb` file and MGCB Editor; the plan should not depend on editor-only behavior. The official documentation currently recommends Content Builder for projects created from newer templates.

References:

- [MonoGame: Understanding the Game1 code](https://docs.monogame.net/articles/getting_started/3_understanding_the_code.html)
- [MonoGame: Handling input](https://docs.monogame.net/articles/tutorials/building_2d_games/10_handling_input/)
- [MonoGame: Input management and previous-state checks](https://docs.monogame.net/articles/tutorials/building_2d_games/11_input_management/)
- [MonoGame: Scene management](https://docs.monogame.net/articles/tutorials/building_2d_games/17_scenes/)
- [MonoGame: Adding content](https://docs.monogame.net/articles/getting_started/content_pipeline/index.html)
- [MonoGame: Content Builder](https://docs.monogame.net/articles/getting_started/content_pipeline/content_builder/index.html)

## 3. Architectural goals

### Must have

- Very small runtime and dependency footprint.
- Deterministic, testable gameplay rules.
- Clear separation between game rules, input mapping, rendering, audio, and persistence.
- Keyboard/numeric-keypad support as the primary control scheme.
- Easy tuning of timers, prices, colors, and level progression.
- No allocation-heavy work in the per-frame update loop.
- A design that can be expanded with additional upgrades or pattern rules without rewriting the game loop.

### Should avoid

- A general ECS for a game with only a handful of gameplay objects.
- A large UI framework unless the store becomes substantially more complex.
- Putting score, timers, or upgrade calculations in drawing code.
- Letting screens directly poll `Keyboard.GetState()` in multiple places.
- Storing mutable run state in static global variables.

## 4. Proposed solution structure

Start with one executable project and one test project. Split into additional class libraries only if the codebase grows enough to justify the project overhead.

```text
Min-OS/
├─ Min-OS.sln
├─ src/
│  ├─ Min-OS.Game/
│  │  ├─ Game1.cs
│  │  ├─ GameHost.cs
│  │  ├─ Core/
│  │  │  ├─ IGameScreen.cs
│  │  │  ├─ ScreenManager.cs
│  │  │  ├─ GameClock.cs
│  │  │  ├─ RandomSource.cs
│  │  │  └─ ServiceRegistry.cs (optional; prefer constructor wiring first)
│  │  ├─ Input/
│  │  │  ├─ InputManager.cs
│  │  │  ├─ InputAction.cs
│  │  │  └─ KeyBindings.cs
│  │  ├─ Gameplay/
│  │  │  ├─ RunController.cs
│  │  │  ├─ PatternGenerator.cs
│  │  │  ├─ PatternValidator.cs
│  │  │  ├─ RunState.cs
│  │  │  ├─ UpgradeSystem.cs
│  │  │  ├─ StoreCatalog.cs
│  │  │  └─ GameBalance.cs
│  │  ├─ Screens/
│  │  │  ├─ TitleScreen.cs
│  │  │  ├─ GameplayScreen.cs
│  │  │  ├─ GameOverScreen.cs
│  │  │  ├─ StoreScreen.cs
│  │  │  └─ PauseScreen.cs (optional)
│  │  ├─ Rendering/
│  │  │  ├─ PatternRenderer.cs
│  │  │  ├─ HudRenderer.cs
│  │  │  ├─ UiRenderer.cs
│  │  │  └─ ColorPalette.cs
│  │  ├─ Audio/
│  │  │  └─ AudioManager.cs
│  │  ├─ Persistence/
│  │  │  ├─ SaveGame.cs
│  │  │  └─ SaveGameRepository.cs
│  │  └─ Content/
│  │     ├─ Assets/
│  │     └─ Builder.cs or Content.mgcb
│  └─ Min-OS.Tests/
│     ├─ PatternGeneratorTests.cs
│     ├─ PatternValidatorTests.cs
│     ├─ UpgradeSystemTests.cs
│     └─ RunControllerTests.cs
└─ README.md
```

Recommended initial implementation: keep all folders inside `Min-OS.Game` and use `Min-OS.Tests` for pure logic tests. If compile times or ownership boundaries become a problem, move `Gameplay` and its value types into `Min-OS.Core` without changing the design.

## 5. Runtime composition

`Game1` should be a thin MonoGame adapter. It owns the `GraphicsDeviceManager`, `SpriteBatch`, `ContentManager`, and the root services. It should not contain detailed game rules.

```text
Game1
 ├─ InputManager
 ├─ ScreenManager
 │   └─ active IGameScreen
 ├─ AudioManager
 ├─ AssetRepository / ContentManager
 ├─ SaveGameRepository
 └─ RunController (owned by GameplayScreen or shared through a run session)
```

Each screen receives the services it needs and exposes:

```csharp
public interface IGameScreen
{
    void OnEnter();
    void OnExit();
    void Update(GameTime gameTime, InputSnapshot input);
    void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}
```

The `ScreenManager` handles screen changes and ensures only the active screen updates gameplay. A simple replace-current-screen approach is enough. A stack is useful only if pause screens or modal store dialogs are introduced.

## 6. Game loop and timing

Use MonoGame's `Update` as the simulation boundary:

1. Poll input once in `Game1`.
2. Build an immutable `InputSnapshot` containing current and newly pressed actions.
3. Pass that snapshot to the active screen.
4. Advance gameplay timers using `GameTime.ElapsedGameTime`.
5. Draw the latest state.

Do not use `Thread.Sleep`, wall-clock reads, or timers owned by individual UI controls.

Suggested initial settings:

- `IsFixedTimeStep = true` for predictable gameplay updates.
- A normal fixed step of 1/60 second.
- `GameTime.ElapsedGameTime` converted to seconds for all countdowns.
- A pause flag that stops gameplay timers while a pause screen is active.

Visual flashing should be driven by a small presentation state (`flashColor`, `flashRemaining`) while the underlying pattern remains unchanged. Rendering may animate at the display rate, but success/failure decisions must happen in the gameplay update logic.

## 7. Input architecture

`InputManager` polls the keyboard once per update and compares the current `KeyboardState` with the previous state. It should expose semantic actions instead of letting gameplay know about `Keys`.

```csharp
public enum InputAction
{
    None,
    Color0, Color1, Color2, Color3, Color4,
    Color5, Color6, Color7, Color8,
    Confirm,
    Back,
    Pause
}

public readonly record struct InputEvent(InputAction Action, bool IsPressed);
```

The nine color actions map to the numeric keypad. The exact mapping must be decided and documented in-game. A natural default is:

```text
NumPad 7  NumPad 8  NumPad 9
NumPad 4  NumPad 5  NumPad 6
NumPad 1  NumPad 2  NumPad 3
```

This makes the input layout visually match a 3x3 color layout. Support the number-row keys as an accessibility fallback only if it does not make the controls confusing. Use `Keys.NumPad0` only for menu/debug actions, not as one of the nine colors.

Gameplay should consume a queue of discrete `ColorInput` values. Holding a key must not repeatedly submit a color; one color is queued on the key-down edge.

`InputSnapshot` should contain:

- `IReadOnlyList<ColorInput> ColorInputs` pressed this update.
- `bool ConfirmPressed`.
- `bool BackPressed`.
- `bool PausePressed`.
- Optional mouse state for menus, if mouse navigation is later supported.

## 8. Core gameplay model

### Value types

```csharp
public enum ColorId
{
    Color0, Color1, Color2, Color3, Color4,
    Color5, Color6, Color7, Color8
}

public readonly record struct ColorInput(ColorId Color);
public readonly record struct PatternSet(ColorId A, ColorId B, ColorId C, ColorId D);
```

Internally, use a compact `ColorId`/byte representation. Do not store `Color` objects in the simulation model; rendering maps `ColorId` to actual MonoGame colors.

### RunState

`RunState` is the mutable state for one run. It should contain:

- Current level.
- Current set index and total sets in the level.
- The generated pattern for the level/run.
- The player's current input position.
- Current HP and maximum HP.
- Input timer remaining.
- Memory timer remaining.
- Breakpoint timer remaining.
- Current multiplier.
- Bits earned this run.
- Consecutively completed sets.
- Upgrade effects effective for this run.
- Whether the run is active, won, or game over.

The model should not contain `Texture2D`, `SpriteFont`, `SpriteBatch`, or screen references.

### Pattern generation

`PatternGenerator` is deterministic when given a seed and an `IRandomSource`. It should generate four `ColorId` values per set. Store the generated pattern for the active run so that retries, display, validation, and tests all use the same sequence.

Recommended first version:

- `setsInLevel = level` (level 1 has one set, level 2 has two sets, etc.).
- Each set contains exactly four colors.
- Random choices are uniform among the nine colors.
- Adjacent repeats are allowed unless playtesting shows they are unreadable.
- Generate the full level at level start, then reveal it set-by-set.

### Validation

`PatternValidator` compares each queued input to the expected color at the current position. It returns a small result rather than mutating UI:

```csharp
public enum InputResult
{
    Ignored,
    Correct,
    Wrong,
    SetCompleted,
    LevelCompleted
}
```

On a wrong input:

1. Decrease HP.
2. Clear or preserve the current input position according to the final design decision.
3. Reset the consecutive-set multiplier if that is intended.
4. Trigger a short error feedback effect.
5. End the run if HP reaches zero.

On a completed set:

1. Award bits.
2. Increase the consecutive-set counter.
3. Recalculate the multiplier.
4. Enter the breakpoint phase.
5. Reveal the next set after the breakpoint.

## 9. Gameplay phases

Use an explicit phase enum rather than scattered boolean flags:

```csharp
public enum RunPhase
{
    Preparing,
    ShowingPattern,
    WaitingForInput,
    Breakpoint,
    LevelComplete,
    GameOver
}
```

Expected flow:

```text
Title
  -> Start Run
  -> Preparing
  -> ShowingPattern
  -> WaitingForInput
       -> Correct input -> next input
       -> Wrong input -> HP loss / Game Over
       -> set complete -> Breakpoint
  -> next set or LevelComplete
  -> next level -> ShowingPattern
  -> timeout / HP zero -> GameOver
  -> Store
  -> Start Run
```

Only `RunController` may change the phase. Screens observe the phase and render it. This prevents a UI button, renderer, and timer from independently changing the game state.

### ShowingPattern

The full current pattern is displayed one color at a time or as the final design specifies. `MemoryTime` controls the available time to see the pattern. If the design means “show the entire sequence for a total duration,” implement the first version that way; if it means “each color has a duration,” add a separate per-color flash duration later.

### WaitingForInput

The player enters colors until the current set is complete. `InputTime` is a countdown for the current input opportunity. The renderer should show progress, remaining time, and any permitted assistance.

### Breakpoint

No input is consumed during the breakpoint unless the final design explicitly permits buffering. The breakpoint timer is controlled by the `Breakpoint` upgrade.

## 10. Scoring and currency

Keep scoring in a pure `ScoreCalculator` or in `RunController`, not in the renderer.

Initial interpretation of the requirements:

```text
consecutiveSets += 1
multiplier = 1.0 + (0.1 * consecutiveSets)
setBits = baseBitsForSet * multiplier
runBits += floor(setBits)
```

The requirement says “each completed set will give you bits” and gives `multiplier += .1` as an example. The base bit value and rounding rule are not specified. Use `baseBitsForSet = 1` as a temporary balance value and isolate it in `GameBalance`.

Recommended reset rule to test: a wrong guess resets the consecutive-set counter and multiplier to the base value, while a timeout ends the run. If the intended roguelike design rewards partial consistency, change this in one scoring policy rather than the controller.

Use `double` for multiplier calculations and `int`/`long` for currency. Round only at the point where bits are awarded so fractional values cannot create repeated rounding errors.

## 11. HP, regeneration, and timing upgrades

Represent upgrades as modifiers on a `RunModifiers` value rather than putting upgrade-specific checks throughout gameplay.

```csharp
public sealed record RunModifiers
{
    public float InputTimeBonusSeconds { get; init; }
    public float MemoryTimeBonusSeconds { get; init; }
    public int MaxHpBonus { get; init; }
    public float RegenIntervalReductionSeconds { get; init; }
    public int CachedMemorySets { get; init; }
    public int AutoCompleteCharges { get; init; }
    public int AutoCooldownPatternReduction { get; init; }
    public float BreakpointBonusSeconds { get; init; }
    public int ShowCharges { get; init; }
    public int StartingLevel { get; init; }
}
```

The names above are implementation concepts; balance values should live in `GameBalance` and persistent purchase levels should live in `ProgressionState`.

### Initial upgrade interpretation

| Upgrade | Effect | Scope |
|---|---|---|
| Input Time | Adds seconds to input countdown | Per run |
| Memory Time | Adds seconds to pattern display | Per run |
| HP | Adds maximum wrong guesses/health | Per run |
| Regen | Reduces time before one HP is restored | Per run |
| Cached Memory | Allows additional sets to remain visible during input | Per run |
| Auto-Complete | Grants charges that complete a set without manual input | Per run |
| Auto Cooldown | Reduces cooldown between auto-complete uses by one pattern | Per run |
| Breakpoint | Adds pause time after a set | Per run |
| Show | Grants additional pattern reveals beginning at the current set | Per run |
| Starting Level | Changes the level where a new run begins | Persistent setting; adjustable |

“Regen” needs a precise definition. The plan assumes HP can recover during a run, but the requirements do not state whether recovery occurs only after correct sets, continuously over time, or at breakpoints. Encapsulate this in `HealthSystem` so the rule can change without touching pattern validation.

“Cached Memory” also needs a precise presentation rule. The simplest implementation is to retain the last N completed sets in a small on-screen history while the player enters the current set.

## 12. Store and progression

The store is a screen over persistent progression data plus the current run result.

```csharp
public sealed class ProgressionState
{
    public int BankedBits { get; set; }
    public int StartingLevel { get; set; }
    public Dictionary<UpgradeId, int> PurchaseLevels { get; } = new();
}
```

`StoreCatalog` contains static definitions:

- ID.
- Display name.
- Description.
- Base price.
- Price growth rule.
- Maximum level, if any.
- Effect conversion function.

The store controller should validate affordability, apply one purchase at a time, and return a result such as `Purchased`, `NotEnoughBits`, `AtMaxLevel`, or `InvalidItem`.

Use a single store currency policy. The requirements do not explicitly say whether bits are banked across runs or only earned during the current run. The proposed default is: bits earned in a run are banked when the game-over result is accepted, then spent in the store. If the intended model is “spend before game over” or “lose unspent bits,” change only the persistence/settlement step.

## 13. Rendering plan

This is a 2D UI-heavy game, so rendering can remain simple:

- One `SpriteBatch` per draw pass.
- A generated 1x1 white texture for solid rectangles, tinted by `ColorId`.
- One or two `SpriteFont` assets for text.
- Optional texture assets for logo, panels, icons, and keypad hints.
- `SamplerState.PointClamp` if the art direction is pixel-based; otherwise use the default linear sampler.
- A virtual resolution such as 1280x720 with a scale/letterbox transform for consistent layout.

Suggested draw layers:

1. Background.
2. Pattern squares and flash effects.
3. Keypad/input display.
4. HUD: level, HP, timer, multiplier, bits.
5. Menus, store cards, and modal overlays.

`PatternRenderer` should be given a read-only view model, for example `PatternView`, rather than the entire `RunState`. This keeps rendering independent from rule changes.

### Accessibility and readability

Do not rely on color alone. Each color should also have one of:

- A number/letter label.
- A distinct symbol.
- A distinct pattern/stripe treatment.
- An optional high-contrast mode.

The chosen nine colors should be checked for color-vision accessibility. This is especially important because the core mechanic requires fast visual recall.

## 14. Audio plan

Use a small `AudioManager` abstraction with named cues:

```csharp
public enum SoundCue
{
    ColorShown,
    InputAccepted,
    InputRejected,
    SetCompleted,
    LevelCompleted,
    Purchase,
    GameOver,
    MenuMove,
    MenuConfirm
}
```

Sound should be triggered by gameplay events/results, not by the renderer. Do not add an audio middleware dependency initially. `SoundEffect` and `SoundEffectInstance` are sufficient for short cues; add music only after the loop is fun and the timing/audio latency is tested.

## 15. Persistence

Persist only meta-progression, not the active run, in the first version:

- Banked bits.
- Upgrade purchase levels.
- Adjustable starting level.
- Settings such as volume, fullscreen, and colorblind/high-contrast mode.

Use `System.Text.Json` with a versioned save DTO. Write to a temporary file and replace the old save file after a successful write. Keep save I/O out of `Update` during active gameplay; save when leaving the store, returning to the title screen, or exiting.

Open question: whether the game needs run recovery after a crash. If it does, add a separate checkpoint format and save only at safe phase boundaries.

## 16. Configuration and content

Put tuning values in a single `GameBalance` object. At minimum it should define:

- Base memory time.
- Base input time.
- Base max HP.
- HP regeneration behavior.
- Base breakpoint duration.
- Base bits per set.
- Multiplier increment.
- Maximum level or level growth behavior.
- Store prices and price growth.
- Flash duration and feedback duration.

The first implementation can use C# constants/records. Move balance to JSON or a content-pipeline data asset only when designers need to tune without recompiling. Do not make gameplay rules depend on arbitrary content-file parsing in the first milestone.

## 17. Testing strategy

Most of the game can be tested without opening a MonoGame window.

### Unit tests

- A seed produces a repeatable pattern.
- Every generated set contains exactly four valid colors.
- Validation accepts a correct sequence in order.
- Validation rejects a wrong color at the correct position.
- A completed set increments the set counter exactly once.
- A held key does not enqueue repeated inputs.
- A timeout produces game over.
- HP reaches zero only after the expected number of wrong guesses.
- Multiplier increments by the configured amount.
- Score rounding is stable.
- Upgrade purchases reject unaffordable or maxed-out items.
- Starting level is clamped to a valid range.
- Save/load preserves progression and handles a missing save.

### Manual acceptance tests

- A physical numpad can complete a pattern reliably.
- Numpad Num Lock behavior is understood and documented.
- A key press near a timer boundary has predictable results.
- Flash timing is visible at the target frame rate.
- The game remains readable in windowed, fullscreen, and scaled modes.
- A player can use the store without a mouse if keyboard-only play is a goal.
- Color accessibility mode makes all nine inputs distinguishable.

## 18. Milestones

### Milestone 1 — Playable core

- Create MonoGame project and content pipeline.
- Add `Game1`, `InputManager`, `ScreenManager`, and title/gameplay screens.
- Draw nine colored squares and a text HUD.
- Map the numpad to nine colors.
- Generate and show one four-color set.
- Validate input and show success/failure.

### Milestone 2 — Run loop

- Add levels with increasing set counts.
- Add memory, input, and breakpoint timers.
- Add HP, game over, and restart.
- Add deterministic seed support for debugging.
- Add unit tests for generator, validator, and timers.

### Milestone 3 — Scoring and store

- Add bits, multiplier, and level completion.
- Add game-over result screen.
- Add banked currency and save file.
- Add store catalog and the first three upgrades: Input Time, Memory Time, and HP.

### Milestone 4 — Remaining upgrades and polish

- Add Regen, Cached Memory, Auto-Complete, Auto Cooldown, Breakpoint, Show, and Starting Level.
- Add audio cues and visual feedback.
- Add accessibility labels/high-contrast option.
- Add keyboard-only store navigation.
- Add settings and safe save handling.

### Milestone 5 — Balance and release hardening

- Playtest timing and difficulty curves.
- Decide all open rules listed below.
- Verify content builds on the target platform.
- Test save migration/versioning.
- Package a release build and test from a clean install directory.

## 19. Decisions still required before full production

These are intentionally not hidden behind architecture assumptions:

1. What exactly are the “four colored squares”: four positions in a row, a 2x2 display, or four independent squares that flash one at a time?
2. Does a level show all of its sets as one continuous pattern, or does the player complete one four-color set at a time with each breakpoint acting as a checkpoint?
3. Is `Memory Time` a total time for the whole set, a time per color, or a time per set?
4. Is `Input Time` measured per set, per color, or for the entire level?
5. Does a wrong guess reset the current set, remove only one HP, reset the multiplier, or all three?
6. Are bits banked permanently across runs? When exactly are they awarded and saved?
7. What is the base bits value and how should fractional multiplier results be rounded?
8. Is HP a number of mistakes, a regenerating resource, or both? What is the exact Regen trigger and interval?
9. What does Cached Memory display and how many sets can remain visible simultaneously?
10. Does Auto-Complete consume a charge per set, and what does “cooldown by 1 pattern” mean numerically?
11. Does Show reveal the current set again, show the full level, or consume one display of only the current set?
12. Is Starting Level a permanent upgrade, an adjustable cap, or a setting selected at run start?
13. Is there a maximum level, and if so how does difficulty grow after the maximum number of sets becomes impractical?
14. Should color repetition be allowed inside a set or between adjacent inputs?
15. What happens when a player presses a key during a flash, breakpoint, or transition?
16. Should the game support number-row keys, gamepad, mouse, or only the numeric keypad?
17. Is Num Lock required, and should the game detect/communicate that state?
18. Is the target platform Windows desktop only, or should the project preserve DesktopGL/cross-platform support?
19. What is the desired art direction: flat neon UI, pixel art, terminal/OS interface, or something else?
20. Is a run recoverable after a crash, or is only meta-progression persisted?

Resolve these in a short design-spec pass before implementing all upgrades. The architecture above keeps each uncertainty localized so the answers should not require a rewrite of the MonoGame host.

## 20. Recommended first implementation boundary

Build only this vertical slice first:

```text
Title -> one level -> one four-color set -> numpad input -> score -> game over -> restart
```

Do not implement the full store or every upgrade until this slice answers three questions through playtesting: whether the nine-color keypad is readable, whether the memory/input timing feels fair, and whether the four-color set/breakpoint structure is fun. Once those are stable, the remaining architecture is mostly straightforward data and phase work.

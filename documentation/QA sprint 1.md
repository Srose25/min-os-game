# QA Sprint 1

## Decisions confirmed by the product owner

- Save-slot selection must appear after selecting **Start Game**.
- Placeholder sound effects must be generated because final sound assets are not available yet.
- **New Game** always starts at level 1 with no upgrades.
- Loading a saved game may resume at a higher purchased Starting Level and must restore the upgrades purchased in that save.

## Engineer instructions

### 1. Add save-slot selection to Start Game

- Change the Start Menu's **Start Game** action to open a save-slot selection screen or modal.
- Display exactly three slots with clear states: empty, available save, or corrupted save.
- Each occupied slot should show enough information to identify it, such as current level, banked bits, upgrade count, and last-save status.
- Selecting an empty slot should create a new run at level 1 with no upgrades and default settings.
- Selecting an occupied slot should load its saved upgrades, settings, statistics, banked bits, and active run state.
- If a slot has an active run, offer **Continue Run**. If it has no active run, begin a new run using that save's permanent progression.
- Corrupt-save handling must delete only the affected slot and show a clear message before allowing the player to continue.
- Keep save-slot loading and creation outside rendering code. Use `SaveGameRepository` through an injected interface.
- Add automated tests for empty slots, all three independent slots, loading an active run, loading permanent upgrades, and corrupt-slot handling.

### 2. Generate placeholder sound effects

- Add one placeholder sound effect for each of the nine colors.
- Use short, distinct tones so every color can be identified by sound.
- Add the assets through the MonoGame Content Builder pipeline under the game's Content/Audio folder.
- Update `AudioManager` to load the nine assets and map each `ColorId` to the correct sound effect.
- Play the matching sound when a color input is accepted and when that color is displayed during memory time.
- Respect the persisted SFX volume setting and support SFX mute at zero volume.
- Keep audio non-authoritative: gameplay timing and validation must not depend on sound playback completing.
- Add a test seam or fake audio service so gameplay tests do not require a graphics or audio device.
- Document that these are temporary placeholders and can be replaced without changing gameplay code.

### 3. Separate New Game from Load Saved Game

- A **New Game** must always initialize at level 1 with zero upgrade levels.
- A New Game must not inherit upgrades, banked bits, active-run state, or the purchased Starting Level from another save.
- A **Load Saved Game** action must restore the selected save's permanent upgrades, settings, statistics, banked bits, and active run when one exists.
- When beginning a fresh run from a loaded save, use the saved Starting Level upgrade as the run's starting level.
- Preserve permanent upgrades for that save while the player remains in that save slot.
- Do not share mutable upgrade dictionaries between save slots or between a new run and a loaded run.
- Define and implement the transition after Game Over and after completing a level so the correct save slot is updated before returning to the Store or Start Menu.
- Add automated tests proving that:
  - New Game starts at level 1 with no upgrades.
  - Loading a saved game restores its upgrades.
  - A loaded save can start at its purchased Starting Level.
  - Save slot A's upgrades do not appear in save slot B.
  - Banked bits and active-run state survive save/load.

## Remaining QA findings to address

- Wire save-slot selection, active-run persistence, settings persistence, statistics, and banked bits into `Game1` and the screen flow.
- Implement settings remapping, conflict feedback, and persistence.
- Add visible controls for Auto-Complete and Show during the input phase.
- Fix Cached Memory so only the configured number of earliest sets remain visible at low opacity.
- Verify Num Lock behavior on the target machine and display the required warning if numpad input is unavailable.
- Replace placeholder leaderboard values with persisted statistics.
- Expand automated coverage for pause timing, mouse input, Show, Cached Memory, upgrade boundaries, and layout-related view models.

## Styling corrections

### 1. Correct the text foreground and drop shadow

- The current text layers are visually reversed: the dominant foreground color blends into the blue background, while the white secondary layer is behind it.
- Make `#FFFFFF` the dominant foreground color for all standard text.
- Change the current blue primary text color to a slightly darker version of the current color and use it as the offset drop shadow.
- Draw the darker shadow first at a small consistent offset, then draw the white text at the intended position on top.
- Keep the shadow thin and visible without reducing the legibility of the white foreground.
- Apply this consistently to headers, body text, menu labels, HUD text, store text, leaderboard text, and button labels.
- Centralize the colors and shadow offset in the rendering/layout helper instead of duplicating them in individual screen draw methods.
- Verify the text remains readable against the normal blue background, Store background, and Game Over background.

Suggested implementation order in `DrawText`:

1. Draw the text in the darker blue shadow color at `(x + shadowOffset, y + shadowOffset)`.
2. Draw the same text in `Color.White` at `(x, y)`.

### 2. Center the pattern during memory time

- The pattern is currently anchored to the left side of the game screen. This is visually unbalanced and makes the memory phase harder to focus on.
- Center the complete pattern block horizontally in the game viewport.
- Calculate the block width from the number of sets in the current level, the four colors per set, the spacing between colors, the breakpoint marker width, and the spacing between sets.
- Preserve the required maximum of three sets per row.
- Center every row independently so levels with one, two, or three sets do not appear left-aligned.
- For levels with multiple rows, center the entire pattern area vertically within the designated upper game-screen region while keeping the HUD above it and the game pad below it.
- Ensure the layout works for level 1, level 3, level 6, and level 15, including five rows of three sets.
- Keep the pattern centered during both full-memory display and any Show reveal. Cached Memory opacity should not change its position.
- Move pattern positioning into a reusable layout helper or view-model calculation so rendering does not contain hard-coded `x = 40` positioning.
- Confirm that the centered pattern does not overlap the HUD, game pad, or viewport edges at the supported resolution.

## Acceptance criteria for the next QA pass

- Start Game opens save-slot selection before gameplay begins.
- New Game and Load Saved Game are visibly distinct actions with the behaviors defined above.
- Nine placeholder sound effects build through Content Builder and play at the correct volume.
- Save slots remain independent across restart, Game Over, level completion, and new runs.
- Text uses white as the foreground with a darker blue offset shadow.
- The pattern is horizontally centered on every row and vertically balanced in the memory region.
- The automated test suite passes, and the game can be manually exercised from Start Menu through gameplay, Store, Game Over, save, load, and restart.

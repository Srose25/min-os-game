# Engineer handoff

The architecture is deliberately graphics-free at its core. Implement `Game1`, screen classes, and MonoGame-specific input/rendering around the interfaces in `src/Min-OS.Game/Gameplay/Contracts.cs`.

1. Confirm the installed MonoGame template/package version and update the package pins if required.
2. Keep `PatternGenerator`, `PatternValidator`, score, health, store, and save logic injectable and testable without a `Game` window.
3. Poll keyboard/mouse once in `Game1.Update`; emit only key-down edges. Never let screens poll devices directly.
4. Build read-only `GameViewModel` snapshots for drawing. The renderer must not mutate `RunState`.
5. Implement the explicit phase transitions in `Architecture/STATE-MACHINE.md`; pause should stop elapsed-time advancement.
6. Add a `Game1`/screen shell and content pipeline assets, then add the nine color SFX. Wallpoet must be bundled only if licensing and Content Builder support are confirmed; otherwise use a bundled substitute.
7. Treat the linear price formulas and timing constants in `GameBalance` as the only tuning seam. Breakpoint restore starts at 1 second and adds 1 second per Breakpoint purchase. Log any changes as implementation notes.
8. Verify numpad behavior with Num Lock off on the target machine. If MonoGame exposes only `Keys.NumPad*` behavior requiring Num Lock, show the planned warning and document the limitation.

Known architecture TODOs are intentionally marked with `Engineer:` comments in source files.

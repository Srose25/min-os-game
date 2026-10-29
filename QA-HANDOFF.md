# QA handoff

Use xUnit with deterministic seeds and fake clocks/input sources. Start with pure tests for `PatternGenerator`, `PatternValidator`, `HealthSystem`, `ScoreCalculator`, `StoreCatalog`, `BindingValidator`, and `SaveGameRepository`.

The most important boundary assertions are: invalid phases return `Ignored`; repeated held keys do not produce extra `ColorInput`; level generation yields exactly `level` sets of four colors; save writes use three independent slots; and corrupt JSON deletes only its slot.

Manual QA must still verify MonoGame-specific numpad/Num Lock behavior, mouse hit testing, layout scaling, all six screens, cached opacity, pause timer freezing, nine-color distinguishability, and audio toggles.

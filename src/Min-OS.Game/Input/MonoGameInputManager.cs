using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MinOS.Core;
using MinOS.Gameplay;
using System.Collections.Immutable;

namespace MinOS.Input;

/// <summary>Polls devices once per frame and emits only edge-triggered color actions.</summary>
public sealed class MonoGameInputManager : IInputManager
{
    private KeyboardState previousKeyboard;
    private MouseState previousMouse;
    private KeyboardState keyboard;
    private MouseState mouse;
    private readonly Dictionary<Keys, ColorId> bindings = DefaultBindings();
    public static Dictionary<Keys, ColorId> DefaultBindings() => new()
    {
        [Keys.NumPad7] = ColorId.Color0, [Keys.NumPad8] = ColorId.Color1, [Keys.NumPad9] = ColorId.Color2,
        [Keys.NumPad4] = ColorId.Color3, [Keys.NumPad5] = ColorId.Color4, [Keys.NumPad6] = ColorId.Color5,
        [Keys.NumPad1] = ColorId.Color6, [Keys.NumPad2] = ColorId.Color7, [Keys.NumPad3] = ColorId.Color8,
        // QA sprint 3: number-row keys are numerical; numpad keys retain physical keypad order.
        [Keys.D1] = ColorId.Color0, [Keys.D2] = ColorId.Color1, [Keys.D3] = ColorId.Color2,
        [Keys.D4] = ColorId.Color3, [Keys.D5] = ColorId.Color4, [Keys.D6] = ColorId.Color5,
        [Keys.D7] = ColorId.Color6, [Keys.D8] = ColorId.Color7, [Keys.D9] = ColorId.Color8
    };
    public bool PausePressed { get; private set; }
    public bool LeftClickPressed { get; private set; }
    public Point MousePosition => mouse.Position;
    public void Poll() { previousKeyboard = keyboard; previousMouse = mouse; keyboard = Keyboard.GetState(); mouse = Mouse.GetState(); PausePressed = keyboard.IsKeyDown(Keys.Escape) && !previousKeyboard.IsKeyDown(Keys.Escape); LeftClickPressed = mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released; }
    public IReadOnlyList<ColorInput> ReadColorEdges() => bindings.Where(x => keyboard.IsKeyDown(x.Key) && !previousKeyboard.IsKeyDown(x.Key)).Select(x => new ColorInput(x.Value)).ToImmutableArray();
    public IReadOnlyList<Keys> ReadKeyEdges() => Enum.GetValues<Keys>().Where(key => keyboard.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key)).ToImmutableArray();

    public bool TryRebind(ColorId color, Keys key)
    {
        if (bindings.TryGetValue(key, out var existing) && existing != color) return false;
        foreach (var pair in bindings.Where(pair => pair.Value == color).ToArray()) bindings.Remove(pair.Key);
        bindings[key] = color;
        return true;
    }

    public Dictionary<ColorId, string> ExportBindings() => bindings.GroupBy(pair => pair.Value).ToDictionary(group => group.Key, group => string.Join("|", group.Select(pair => pair.Key.ToString())));

    public void ImportBindings(IReadOnlyDictionary<ColorId, string> saved)
    {
        if (saved.Count == 0) return;
        bindings.Clear();
        foreach (var text in saved)
        {
            foreach (var name in text.Value.Split('|', StringSplitOptions.RemoveEmptyEntries))
                if (Enum.TryParse<Keys>(name, out var key) && !bindings.ContainsKey(key)) bindings[key] = text.Key;
        }
    }

    // QA sprint 3: a new save slot must not inherit a loaded slot's mutable bindings.
    public void ResetDefaults()
    {
        bindings.Clear();
        foreach (var pair in DefaultBindings()) bindings[pair.Key] = pair.Value;
    }
    public ColorInput? ReadMouseColorEdge(int width, int padTop)
    {
        if (!LeftClickPressed) return null;
        var column = (mouse.X - (width / 2 - 150)) / 100;
        var row = (mouse.Y - padTop) / 70;
        if (column is < 0 or > 2 || row is < 0 or > 2) return null;
        var rectangle = new Rectangle(width / 2 - 150 + column * 100, padTop + row * 70, 60, 50);
        return rectangle.Contains(mouse.Position) ? new ColorInput((ColorId)(row * 3 + column)) : null;
    }
}

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
    private readonly IReadOnlyDictionary<Keys, ColorId> bindings = new Dictionary<Keys, ColorId>
    {
        [Keys.NumPad7] = ColorId.Color0, [Keys.NumPad8] = ColorId.Color1, [Keys.NumPad9] = ColorId.Color2,
        [Keys.NumPad4] = ColorId.Color3, [Keys.NumPad5] = ColorId.Color4, [Keys.NumPad6] = ColorId.Color5,
        [Keys.NumPad1] = ColorId.Color6, [Keys.NumPad2] = ColorId.Color7, [Keys.NumPad3] = ColorId.Color8,
        [Keys.D7] = ColorId.Color0, [Keys.D8] = ColorId.Color1, [Keys.D9] = ColorId.Color2,
        [Keys.D4] = ColorId.Color3, [Keys.D5] = ColorId.Color4, [Keys.D6] = ColorId.Color5,
        [Keys.D1] = ColorId.Color6, [Keys.D2] = ColorId.Color7, [Keys.D3] = ColorId.Color8
    };
    public bool PausePressed { get; private set; }
    public bool LeftClickPressed { get; private set; }
    public Point MousePosition => mouse.Position;
    public void Poll() { previousKeyboard = keyboard; previousMouse = mouse; keyboard = Keyboard.GetState(); mouse = Mouse.GetState(); PausePressed = keyboard.IsKeyDown(Keys.Escape) && !previousKeyboard.IsKeyDown(Keys.Escape); LeftClickPressed = mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released; }
    public IReadOnlyList<ColorInput> ReadColorEdges() => bindings.Where(x => keyboard.IsKeyDown(x.Key) && !previousKeyboard.IsKeyDown(x.Key)).Select(x => new ColorInput(x.Value)).ToImmutableArray();
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

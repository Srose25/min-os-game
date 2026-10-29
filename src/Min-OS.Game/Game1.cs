using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MinOS;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch? spriteBatch;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
    }

    protected override void Initialize()
    {
        // Engineer: construct ScreenManager/InputManager/AudioManager here and load the selected save slot.
        base.Initialize();
    }

    protected override void LoadContent() => spriteBatch = new SpriteBatch(GraphicsDevice);

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) Exit();
        // Engineer: poll input exactly once, then pass semantic edge events to the active screen/controller.
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.FromNonPremultiplied(40, 81, 122, 255));
        // Engineer: draw the active screen from read-only Rendering view models only.
        base.Draw(gameTime);
    }
}

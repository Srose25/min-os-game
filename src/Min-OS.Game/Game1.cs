using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MinOS.Core;
using MinOS.Audio;
using MinOS.Gameplay;
using MinOS.Input;
using MinOS.Persistence;
using MinOS.Rendering;
using MinOS.Screens;
using MinOS.Store;

namespace MinOS;

/// <summary>MonoGame composition root. Screens route input; RunController owns gameplay state.</summary>
public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager graphics;
    private readonly ScreenManager screens = new();
    private readonly MonoGameInputManager input = new();
    private readonly AudioManager audio = new();
    private readonly RunController run;
    private readonly UpgradeSystem upgrades;
    private readonly Dictionary<UpgradeId, int> upgradeLevels;
    private readonly SaveGameRepository saves = new();
    private int? selectedSlot;
    private string? saveMessage;
    private readonly SaveSlotData?[] slotPreviews = new SaveSlotData?[3];
    private readonly bool[] slotFilesExist = new bool[3];
    private readonly Texture2D?[] colorTextures = new Texture2D?[9];
    private SpriteBatch? spriteBatch;
    private SpriteFont? font;
    private bool paused;
    private GamePhase lastPhase = GamePhase.Preparing;
    private static readonly Color[] Palette =
    [ new(174, 0, 255), new(255, 0, 255), new(0, 0, 255), new(0, 255, 255),
      new(0, 255, 0), new(255, 255, 0), new(255, 115, 0), Color.White, new(128, 128, 128) ];

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsFixedTimeStep = true; TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
        IsMouseVisible = true; graphics.PreferredBackBufferWidth = 960; graphics.PreferredBackBufferHeight = 640;
        upgradeLevels = Enum.GetValues<UpgradeId>().ToDictionary(id => id, _ => 0);
        upgrades = new UpgradeSystem(new StoreCatalog(), upgradeLevels);
        run = new RunController(new PatternGenerator(), new PatternValidator(), new HealthSystem(), new ScoreCalculator(), upgrades);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice); font = Content.Load<SpriteFont>("Arial");
        for (var i = 0; i < colorTextures.Length; i++) { colorTextures[i] = new Texture2D(GraphicsDevice, 1, 1); colorTextures[i]!.SetData([Palette[i]]); }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) Exit();
        input.Poll(); HandleGlobalInput(); HandleScreenClick();
        if (screens.ActiveScreen == "Game" && !paused)
        {
            run.Update(gameTime.ElapsedGameTime);
            foreach (var color in input.ReadColorEdges()) { if (run.Submit(color) != ValidationResult.Ignored) audio.PlayColor(color.Color); }
            var mouseColor = input.ReadMouseColorEdge(GraphicsDevice.Viewport.Width, PadTop());
            if (mouseColor.HasValue) { if (run.Submit(mouseColor.Value) != ValidationResult.Ignored) audio.PlayColor(mouseColor.Value.Color); }
            if (run.State.Phase == GamePhase.ShowingPattern && lastPhase != GamePhase.ShowingPattern)
                foreach (var set in run.State.Sets) foreach (var color in set.Colors) audio.PlayColor(color);
            lastPhase = run.State.Phase;
            if (run.State.Phase == GamePhase.GameOver) { SaveCurrentRun(); screens.Show("GameOver"); paused = false; }
            else if (run.State.Phase == GamePhase.LevelComplete) { SaveCurrentRun(); screens.Show("Store"); }
        }
        base.Update(gameTime);
    }

    private void HandleGlobalInput()
    {
        if (!input.PausePressed) return;
        if (screens.ActiveScreen == "Game") { paused = true; run.SetPaused(true); screens.Show("Pause"); }
        else if (screens.ActiveScreen == "Pause") { paused = false; run.SetPaused(false); screens.Show("Game"); }
    }

    private void HandleScreenClick()
    {
        if (!input.LeftClickPressed) return;
        var point = input.MousePosition;
        switch (screens.ActiveScreen)
        {
            case "StartMenu":
                if (Hit(point, 220)) { RefreshSaveSlots(); screens.Show("SaveSlots"); } else if (Hit(point, 290)) screens.Show("Settings");
                else if (Hit(point, 360)) screens.Show("Leaderboard"); else if (Hit(point, 430)) Exit(); break;
            case "Settings":
            case "Leaderboard": if (Hit(point, 550)) screens.Show("StartMenu"); break;
            case "SaveSlots": HandleSaveSlotClick(point); break;
            case "Pause":
                if (Hit(point, 250)) { paused = false; run.SetPaused(false); screens.Show("Game"); }
                else if (Hit(point, 320)) { SaveCurrentRun(); paused = false; run.SetPaused(false); screens.Show("StartMenu"); } break;
            case "GameOver":
                if (Hit(point, 300)) screens.Show("Store");
                else if (Hit(point, 370)) { run.StartNewRun(1, run.State.RunBits); screens.Show("Game"); }
                else if (Hit(point, 440)) { SaveCurrentRun(); screens.Show("StartMenu"); } break;
            case "Store": HandleStoreClick(point); break;
        }
    }

    private void HandleStoreClick(Point point)
    {
        var catalog = new StoreCatalog(); var ids = Enum.GetValues<UpgradeId>();
        for (var i = 0; i < ids.Length; i++)
        {
            if (!new Rectangle(110, 100 + i * 38, 740, 32).Contains(point)) continue;
            var id = ids[i]; var level = upgrades.Levels.GetValueOrDefault(id);
            if (level < catalog.MaxPurchases(id))
            {
                var price = catalog.Price(id, level + 1);
                if (run.State.RunBits >= price && run.TrySpendBits(price))
                {
                    // UpgradeSystem is kept in sync through the same price/limit seam used by tests.
                    var bits = price; upgrades.TryPurchase(id, ref bits);
                }
            }
            return;
        }
        if (Hit(point, 530))
        {
            if (run.State.Phase == GamePhase.LevelComplete) { run.StartNextLevel(); screens.Show("Game"); }
            else { run.StartNewRun(1, run.State.RunBits); screens.Show("Game"); }
        }
    }

    private void HandleSaveSlotClick(Point point)
    {
        for (var slot = 1; slot <= 3; slot++)
        {
            if (!new Rectangle(110, 150 + (slot - 1) * 100, 740, 76).Contains(point)) continue;
            SelectSaveSlot(slot);
            return;
        }
        if (Hit(point, 500)) { saveMessage = null; screens.Show("StartMenu"); }
    }

    private void RefreshSaveSlots()
    {
        for (var index = 0; index < 3; index++)
        {
            slotFilesExist[index] = saves.Exists(index + 1);
            slotPreviews[index] = slotFilesExist[index] ? saves.Load(index + 1) : null;
        }
    }

    private void SelectSaveSlot(int slot)
    {
        // QA sprint 2: slot selection is completed before gameplay starts; this keeps
        // new-game state and loaded-save state from sharing mutable progression.
        var existed = saves.Exists(slot);
        var data = saves.Load(slot);
        if (existed && data is null)
        {
            saveMessage = $"Slot {slot} was corrupt and has been deleted. Select it again to start a new game.";
            screens.Show("SaveSlots");
            return;
        }

        selectedSlot = slot;
        upgradeLevels.Clear();
        foreach (var id in Enum.GetValues<UpgradeId>()) upgradeLevels[id] = data?.UpgradeLevels.GetValueOrDefault(id) ?? 0;
        if (data?.ActiveRun is not null) run.LoadState(data.ActiveRun);
        else run.StartNewRun(data?.UpgradeLevels.GetValueOrDefault(UpgradeId.StartingLevel) ?? 0, data?.BankedBits ?? 0m);
        SaveCurrentRun();
        saveMessage = null;
        screens.Show("Game");
    }

    private void SaveCurrentRun()
    {
        // QA sprint 2: save only at safe transitions, never from Draw or every frame.
        if (selectedSlot is not int slot) return;
        saves.Save(slot, new SaveSlotData
        {
            UpgradeLevels = new Dictionary<UpgradeId, int>(upgradeLevels),
            BankedBits = run.State.RunBits,
            ActiveRun = run.State
        });
    }

    private bool Hit(Point point, int y) => new Rectangle(140, y, GraphicsDevice.Viewport.Width - 280, 52).Contains(point);
    private int PadTop()
    {
        var rows = (run.State.Sets.Count + GameBalance.SetsPerPatternRow - 1) / GameBalance.SetsPerPatternRow;
        return Math.Max(110 + Math.Max(0, rows - 1) * 56, GraphicsDevice.Viewport.Height / 2);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(screens.ActiveScreen == "Store" ? UiStyle.StoreBackground : screens.ActiveScreen == "GameOver" ? UiStyle.GameOverBackground : UiStyle.Background);
        if (spriteBatch is not null)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            switch (screens.ActiveScreen)
            {
                case "Game": DrawGame(spriteBatch); break; case "StartMenu": DrawStartMenu(spriteBatch); break; case "SaveSlots": DrawSaveSlots(spriteBatch); break;
                case "Settings": DrawSettings(spriteBatch); break; case "Leaderboard": DrawLeaderboard(spriteBatch); break;
                case "Store": DrawStore(spriteBatch); break; case "GameOver": DrawGameOver(spriteBatch); break; case "Pause": DrawPause(spriteBatch); break;
            }
            spriteBatch.End();
        }
        base.Draw(gameTime);
    }

    private void DrawStartMenu(SpriteBatch batch) { DrawText(batch, "Min-OS", new Vector2(110, 88), 1.8f); var labels = new[] { "Start Game", "Settings", "Leader Board", "Quit" }; for (var i = 0; i < labels.Length; i++) DrawButton(batch, labels[i], 220 + i * 70); }
    private void DrawSaveSlots(SpriteBatch batch)
    {
        DrawText(batch, "Select Save Slot", new Vector2(110, 48), 1.4f);
        for (var slot = 1; slot <= 3; slot++)
        {
            var data = slotPreviews[slot - 1];
            var label = !slotFilesExist[slot - 1] ? $"Slot {slot}: Empty - New Game" : data is null ? $"Slot {slot}: Corrupted - select to delete" : $"Slot {slot}: Level {data.ActiveRun?.CurrentLevel ?? 1}   Bits {data.BankedBits:0.0}   Upgrades {data.UpgradeLevels.Values.Sum()}";
            DrawPanel(batch, new Rectangle(110, 150 + (slot - 1) * 100, 740, 76), new Color(59, 61, 161));
            DrawText(batch, label, new Vector2(132, 174 + (slot - 1) * 100), .75f);
        }
        if (saveMessage is not null) DrawText(batch, saveMessage, new Vector2(110, 465), .65f);
        DrawButton(batch, "Back", 500);
    }
    private void DrawSettings(SpriteBatch batch) { DrawText(batch, "Settings", new Vector2(110, 48), 1.4f); for (var i = 0; i < 9; i++) DrawText(batch, $"Color {i + 1}: {DefaultKey(i)}", new Vector2(140, 105 + i * 36)); DrawText(batch, "Music Volume: 0%    SFX Volume: 100%", new Vector2(140, 445)); DrawButton(batch, "Back", 550); }
    private void DrawLeaderboard(SpriteBatch batch) { DrawText(batch, "Leader Board", new Vector2(110, 48), 1.4f); var lines = new[] { $"Completed Sets: {run.State.ConsecutiveSets}", "Number of Game Overs: 0", "Number of Inputs: ", $"Highest Level: {run.State.CurrentLevel}", $"Total Bits Earned: {run.State.RunBits:0.0}" }; for (var i = 0; i < lines.Length; i++) DrawText(batch, lines[i], new Vector2(140, 130 + i * 54)); DrawButton(batch, "Back", 550); }
    private void DrawStore(SpriteBatch batch) { var catalog = new StoreCatalog(); var ids = Enum.GetValues<UpgradeId>(); DrawText(batch, "Store", new Vector2(110, 30), 1.4f); DrawText(batch, $"Bits: {run.State.RunBits:0.0}", new Vector2(680, 38)); for (var i = 0; i < ids.Length; i++) { var id = ids[i]; var level = upgrades.Levels.GetValueOrDefault(id); var max = catalog.MaxPurchases(id); DrawPanel(batch, new Rectangle(110, 100 + i * 38, 740, 32), new Color(51, 55, 245)); DrawText(batch, $"{id,-16} {level,2}/{max,-2}  Next: {(level < max ? $"{catalog.Price(id, level + 1):0}" : "MAX")}", new Vector2(128, 104 + i * 38), .7f); } DrawButton(batch, "Continue / New Run", 530); }
    private void DrawGameOver(SpriteBatch batch) { DrawText(batch, "Game Over", new Vector2(110, 80), 1.6f); DrawText(batch, $"Final Score: {run.State.RunBits:0.0}", new Vector2(140, 190)); DrawButton(batch, "Go To Store", 300); DrawButton(batch, "Try Again", 370); DrawButton(batch, "Quit To Menu", 440); }
    private void DrawPause(SpriteBatch batch) { DrawText(batch, "Paused", new Vector2(110, 100), 1.6f); DrawButton(batch, "Resume", 250); DrawButton(batch, "Quit To Menu", 320); }

    private void DrawGame(SpriteBatch batch)
    {
        DrawText(batch, $"Level {run.State.CurrentLevel}   Time {Math.Max(0, run.State.InputSecondsRemaining):0.0}s   HP {run.State.CurrentHp}/{run.State.MaxHp}   x{run.State.Multiplier:0.0}   Bits {run.State.RunBits:0.0}", new Vector2(40, 8), .7f);
        var bounds = PatternLayout.SetBounds(run.State.Sets.Count, GraphicsDevice.Viewport.Width, 70);
        for (var set = 0; set < run.State.Sets.Count; set++)
        {
            var pattern = run.State.Sets[set].Colors;
            var cached = set < upgrades.Levels.GetValueOrDefault(UpgradeId.CachedMemory);
            var bound = bounds[set];
            for (var c = 0; c < pattern.Count; c++)
            {
                var visible = run.State.Phase == GamePhase.ShowingPattern || run.State.PatternRevealed || cached || (set < run.State.CurrentSet && run.State.Phase == GamePhase.WaitingForInput);
                var color = visible ? Palette[(int)pattern[c]] : new Color(20, 30, 40);
                batch.Draw(colorTextures[(int)pattern[c]], new Rectangle(bound.X + c * 18, bound.Y, 14, 28), color * (cached && run.State.Phase == GamePhase.WaitingForInput ? .35f : 1f));
            }
            if (set % 3 < 2 && set < run.State.Sets.Count - 1) DrawText(batch, "*", new Vector2(bound.Right + 7, bound.Y - 2), .7f);
        }
        for (var i = 0; i < 9; i++) { var button = new Rectangle(GraphicsDevice.Viewport.Width / 2 - 150 + (i % 3) * 100, PadTop() + (i / 3) * 70, 60, 50); batch.Draw(colorTextures[i], button, Palette[i]); DrawText(batch, (i + 1).ToString(), new Vector2(button.X + 22, button.Y + 12), .8f); }
    }

    private void DrawButton(SpriteBatch batch, string text, int y) { var rect = new Rectangle(140, y, GraphicsDevice.Viewport.Width - 280, 52); DrawPanel(batch, rect, new Color(59, 61, 161)); DrawText(batch, text, new Vector2(rect.X + 24, rect.Y + 12)); }
    private void DrawPanel(SpriteBatch batch, Rectangle rectangle, Color color) => batch.Draw(colorTextures[0], rectangle, color);
    private void DrawText(SpriteBatch batch, string text, Vector2 position, float scale = 1f)
    {
        if (font is null) return;
        // QA: SpriteFont content is intentionally ASCII-only for now. Replace unsupported
        // glyphs instead of allowing one bad statistic or label to crash the render loop.
        var safeText = new string(text.Select(character => font.Characters.Contains(character) ? character : '?').ToArray());
        // QA sprint 2: draw the darker shadow first and the white foreground second so
        // standard text remains readable on the blue, store, and game-over backgrounds.
        batch.DrawString(font, safeText, position + UiStyle.TextShadowOffset, UiStyle.TextShadow, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        batch.DrawString(font, safeText, position, UiStyle.TextForeground, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
    private static string DefaultKey(int index) => index switch { 0 => "NumPad7 / 7", 1 => "NumPad8 / 8", 2 => "NumPad9 / 9", 3 => "NumPad4 / 4", 4 => "NumPad5 / 5", 5 => "NumPad6 / 6", 6 => "NumPad1 / 1", 7 => "NumPad2 / 2", _ => "NumPad3 / 3" };
}

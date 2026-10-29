namespace MinOS.Screens;

public sealed class ScreenManager : MinOS.Gameplay.IScreenManager
{
    public string ActiveScreen { get; private set; } = "StartMenu";
    public void Show(string screenName) => ActiveScreen = screenName;
}

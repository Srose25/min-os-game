using Microsoft.Xna.Framework;

namespace MinOS.Rendering;

/// <summary>Shared UI colors and text treatment. Keep rendering decisions here so screens stay consistent.</summary>
public static class UiStyle
{
    public static readonly Color Background = new(40, 81, 122);
    public static readonly Color TextShadow = new(32, 61, 96);
    public static readonly Color TextForeground = Color.White;
    public static readonly Vector2 TextShadowOffset = new(2, 2);
    public static readonly Color StoreBackground = new(59, 61, 161);
    public static readonly Color GameOverBackground = new(134, 48, 166);
}

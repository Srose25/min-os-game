using Microsoft.Xna.Framework;
using MinOS.Core;

namespace MinOS.Rendering;

/// <summary>Calculates centered pattern positions; draw code does not own layout math.</summary>
public static class PatternLayout
{
    public const int SetWidth = 72;
    public const int SetGap = 28;
    public const int RowHeight = 56;

    public static IReadOnlyList<Rectangle> SetBounds(int setCount, int viewportWidth, int top)
    {
        var bounds = new List<Rectangle>(setCount);
        for (var rowStart = 0; rowStart < setCount; rowStart += GameBalance.SetsPerPatternRow)
        {
            var count = Math.Min(GameBalance.SetsPerPatternRow, setCount - rowStart);
            var totalWidth = count * SetWidth + (count - 1) * SetGap;
            var left = (viewportWidth - totalWidth) / 2;
            for (var column = 0; column < count; column++)
                bounds.Add(new Rectangle(left + column * (SetWidth + SetGap), top + (rowStart / GameBalance.SetsPerPatternRow) * RowHeight, SetWidth, 28));
        }
        return bounds;
    }
}

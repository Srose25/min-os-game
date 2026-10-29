using Microsoft.Xna.Framework;
using MinOS.Core;

namespace MinOS;

internal static class Program
{
    public static void Main() => usingGame().Run();

    private static Game1 usingGame() => new();
}

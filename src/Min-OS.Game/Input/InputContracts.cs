using MinOS.Core;

namespace MinOS.Input;

public sealed record KeyBindingData(ColorId Color, string KeyName);

public static class BindingValidator
{
    public static bool HasConflicts(IEnumerable<KeyBindingData> bindings) => bindings.GroupBy(x => x.KeyName, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1);
}

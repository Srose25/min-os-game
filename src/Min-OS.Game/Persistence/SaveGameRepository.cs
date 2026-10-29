using System.Text.Json;
using MinOS.Core;
using MinOS.Gameplay;

namespace MinOS.Persistence;

public sealed class SaveGameRepository : ISaveGameRepository
{
    private readonly string directory;
    private readonly JsonSerializerOptions options = new(JsonSerializerDefaults.General) { WriteIndented = true };
    public SaveGameRepository(string? directory = null) => this.directory = directory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Min-OS");

    public SaveSlotData? Load(int slot)
    {
        ValidateSlot(slot);
        var path = PathFor(slot);
        if (!File.Exists(path)) return null;
        try { return JsonSerializer.Deserialize<SaveSlotData>(File.ReadAllText(path), options) ?? Corrupt(path); }
        catch (JsonException) { File.Delete(path); return null; } // Version 0 policy: delete corrupted slots and let UI explain it.
    }

    public void Save(int slot, SaveSlotData data)
    {
        ValidateSlot(slot);
        Directory.CreateDirectory(directory);
        var path = PathFor(slot);
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, options));
        File.Move(temp, path, true);
    }

    public void Delete(int slot) { ValidateSlot(slot); var path = PathFor(slot); if (File.Exists(path)) File.Delete(path); }
    // QA sprint 2: callers use this before Load so empty and corrupt slots can be shown differently.
    public bool Exists(int slot) { ValidateSlot(slot); return File.Exists(PathFor(slot)); }
    private string PathFor(int slot) => Path.Combine(directory, $"save-{slot}.json");
    private static SaveSlotData? Corrupt(string path) { File.Delete(path); return null; }
    private static void ValidateSlot(int slot)
    {
        if (slot is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(slot), "Min-OS provides exactly three save slots.");
    }
}

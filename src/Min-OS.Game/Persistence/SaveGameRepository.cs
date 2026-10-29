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
        var path = PathFor(slot);
        if (!File.Exists(path)) return null;
        try { return JsonSerializer.Deserialize<SaveSlotData>(File.ReadAllText(path), options); }
        catch (JsonException) { File.Delete(path); return null; } // Version 0 policy: delete corrupted slots and let UI explain it.
    }

    public void Save(int slot, SaveSlotData data)
    {
        Directory.CreateDirectory(directory);
        var path = PathFor(slot);
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, options));
        File.Move(temp, path, true);
    }

    public void Delete(int slot) { var path = PathFor(slot); if (File.Exists(path)) File.Delete(path); }
    private string PathFor(int slot) => Path.Combine(directory, $"save-{slot}.json");
}

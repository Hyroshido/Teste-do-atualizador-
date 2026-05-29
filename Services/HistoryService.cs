using System.Text.Json;
using DataSmartUpdater.Models;

namespace DataSmartUpdater.Services;

public sealed class HistoryService
{
    private readonly string _historyPath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public HistoryService(string backupDir)
    {
        var historyDir = Path.Combine(backupDir, "Historico");
        Directory.CreateDirectory(historyDir);
        _historyPath = Path.Combine(historyDir, "historico.json");
    }

    public void Save(UpdateHistoryEntry entry)
    {
        try
        {
            var history = LoadHistory().ToList();
            history.Add(entry);
            File.WriteAllText(_historyPath, JsonSerializer.Serialize(history, _options));
        }
        catch
        {
        }
    }

    public IReadOnlyList<UpdateHistoryEntry> LoadHistory()
    {
        try
        {
            if (!File.Exists(_historyPath))
                return Array.Empty<UpdateHistoryEntry>();

            var json = File.ReadAllText(_historyPath);
            return JsonSerializer.Deserialize<List<UpdateHistoryEntry>>(json, _options) ?? Array.Empty<UpdateHistoryEntry>();
        }
        catch
        {
            return Array.Empty<UpdateHistoryEntry>();
        }
    }
}

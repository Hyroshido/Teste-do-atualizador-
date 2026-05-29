using System.Text.Json;

namespace DataSmartUpdater.Services;

public sealed class AppConfig
{
    public string AppName { get; set; } = "DATA SMART DEPLOY CENTER";
    public string AppSubtitle { get; set; } = "Central Inteligente de Atualização e Implantação";
    public string ManifestUrl { get; set; } = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/manifest.json";
    public string DefaultInstallPath { get; set; } = "C:\\DataSmart";
    public string BackupRoot { get; set; } = "Backup";
    public string DatabaseUpdaterFileName { get; set; } = "atualizador de banco de dados.exe";
    public int DownloadTimeoutSeconds { get; set; } = 120;
    public long MinimumFileSizeBytes { get; set; } = 1024;
    public bool AutoOpenDatabaseUpdater { get; set; } = true;
    public bool AutoClickCarregarArquivos { get; set; } = true;
    public bool AutoCloseDatabaseUpdater { get; set; } = true;

    public static AppConfig Load()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
            {
                var config = new AppConfig();
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                return config;
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            }) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }
}

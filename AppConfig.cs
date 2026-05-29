using System.Text.Json;

namespace DataSmartUpdater;

public sealed class AppConfig
{
    public string AppName { get; set; } = "Data Smart Deploy Center";
    public string AppSubtitle { get; set; } = "Plataforma corporativa de atualização automática";
    public string DefaultInstallPath { get; set; } = @"C:\DataSmart";
    public string ManifestUrl { get; set; } = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/manifest.json";
    public string BackupRoot { get; set; } = "Backups";
    public int DownloadTimeoutSeconds { get; set; } = 120;
    public long MinimumFileSizeBytes { get; set; } = 1024;
    public bool AutoOpenDatabaseUpdater { get; set; } = true;
    public bool AutoClickCarregarArquivos { get; set; } = true;
    public string DatabaseUpdaterFileName { get; set; } = "atualizador de banco de dados.exe";

    public static AppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
        {
            var config = new AppConfig();
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonContent = JsonSerializer.Serialize(config, jsonOptions);
            File.WriteAllText(path, jsonContent);
            return config;
        }

        var content = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppConfig>(content) ?? new AppConfig();
    }
}

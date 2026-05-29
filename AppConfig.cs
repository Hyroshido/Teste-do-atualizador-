using System.Text.Json;

namespace DataSmartUpdater;

public sealed class AppConfig
{
    public string DefaultInstallPath { get; set; } = @"C:\DataSmart";
    public string ManifestUrl { get; set; } = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/manifest.json";
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
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            return config;
        }

        var content = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppConfig>(content) ?? new AppConfig();
    }
}

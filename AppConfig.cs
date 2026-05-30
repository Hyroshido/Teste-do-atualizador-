using System.Text.Json;
using System.Runtime.InteropServices;

namespace DataSmartUpdater;

public sealed class AppConfig
{
    public string AppName { get; set; } = "DataSmart Deploy Center";
    public string AppSubtitle { get; set; } = "Central Inteligente de Atualizacao e Implantacao";
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
            // Define um caminho padrão sensível ao sistema operacional para facilitar desenvolvimento no Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                config.DefaultInstallPath = @"C:\DataSmart";
            else
                config.DefaultInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DataSmart");
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonContent = JsonSerializer.Serialize(config, jsonOptions);
            File.WriteAllText(path, jsonContent);
            return config;
        }

        var content = File.ReadAllText(path);
        var cfg = JsonSerializer.Deserialize<AppConfig>(content) ?? new AppConfig();

        if (string.IsNullOrWhiteSpace(cfg.DefaultInstallPath))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                cfg.DefaultInstallPath = @"C:\DataSmart";
            else
                cfg.DefaultInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DataSmart");
        }

        return cfg;
    }
}

using System.Text.Json;

namespace DataSmartUpdater.Services;

public sealed class AppConfig
{
    public string AppName { get; set; } = "DATA SMART DEPLOY CENTER";
    public string AppSubtitle { get; set; } = "Central Inteligente de Atualização e Implantação";
    public string ManifestUrl { get; set; } = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/manifest.json";
    public string UpdaterManifestUrl { get; set; } = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/updater-manifest.json";
    public string DataSmartPath { get; set; } = "C:\\DataSmart";
    public string BackupRoot { get; set; } = "Backup";
    public string DatabaseUpdaterFileName { get; set; } = "Atualizador de Banco de Dados.exe";
    public int DownloadTimeoutSeconds { get; set; } = 120;
    public long MinimumFileSizeBytes { get; set; } = 1024;
    public bool UseExeFolder { get; set; } = true;
    public string BancoUpdaterMode { get; set; } = "OnlyLoad";
    public string DatabaseUpdaterOption { get; set; } = "OnlyLoad";

    [System.Text.Json.Serialization.JsonIgnore]
    public string DefaultInstallPath => DataSmartPath;

    public static string GetSettingsFilePath()
    {
        var configFolder = Path.Combine("C:\\DataSmart", "Backup", "Config");
        Directory.CreateDirectory(configFolder);
        return Path.Combine(configFolder, "appsettings.json");
    }

    public static AppConfig Load()
    {
        try
        {
            var path = GetSettingsFilePath();
            AppConfig config;

            if (!File.Exists(path))
            {
                config = new AppConfig();
                File.WriteAllText(path, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                var json = File.ReadAllText(path);
                config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                }) ?? new AppConfig();
            }

            if (!PathService.IsValidDataSmartPath(config.DataSmartPath))
            {
                var detected = PathService.FindDataSmartPath(config.DataSmartPath);
                if (!string.IsNullOrWhiteSpace(detected))
                {
                    config.DataSmartPath = detected;
                    config.Save();
                }
            }
            else
            {
                config.Save();
            }

            return config;
        }
        catch
        {
            return new AppConfig();
        }
    }

    public void Save()
    {
        try
        {
            var path = GetSettingsFilePath();
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch
        {
        }
    }
}

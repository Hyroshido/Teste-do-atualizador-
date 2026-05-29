using System.Text.Json;

namespace DataSmartUpdater.Services;

public sealed class AppConfig
{
    public string ManifestUrl { get; set; } = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/manifest.json";

    public static AppConfig Load()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path)) return new AppConfig();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }
}

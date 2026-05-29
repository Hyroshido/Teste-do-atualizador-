using System.Net.Http.Headers;

namespace DataSmartUpdater.Services;

public sealed class EnvironmentValidatorResult
{
    public bool Sucesso { get; set; }
    public List<string> Mensagens { get; } = new();
}

public sealed class EnvironmentValidatorService
{
    private readonly AppConfig _config;
    private readonly string _dataDir;

    public EnvironmentValidatorService(AppConfig config, string dataDir)
    {
        _config = config;
        _dataDir = dataDir;
    }

    public async Task<EnvironmentValidatorResult> ValidateAsync(ManifestFile manifest)
    {
        var result = new EnvironmentValidatorResult { Sucesso = true };

        if (!Directory.Exists(_dataDir))
        {
            result.Sucesso = false;
            result.Mensagens.Add("A pasta DataSmart não existe ou não está acessível.");
        }

        if (!HasWritePermission(_dataDir))
        {
            result.Sucesso = false;
            result.Mensagens.Add("Permissões insuficientes na pasta DataSmart.");
        }

        if (!HasDiskSpace(_dataDir, 200))
        {
            result.Sucesso = false;
            result.Mensagens.Add("Espaço em disco insuficiente em C:\\DataSmart.");
        }

        if (!await HasInternetAsync())
        {
            result.Sucesso = false;
            result.Mensagens.Add("Sem conexão com a internet. Verifique sua rede.");
        }

        if (manifest == null || manifest.Arquivos == null || manifest.Arquivos.Count == 0)
        {
            result.Sucesso = false;
            result.Mensagens.Add("Manifest inválido ou vazio.");
        }

        foreach (var item in manifest.Arquivos)
        {
            if (string.IsNullOrWhiteSpace(item.Nome) || string.IsNullOrWhiteSpace(item.Url))
            {
                result.Sucesso = false;
                result.Mensagens.Add($"Módulo inválido no manifest: {item.Nome}.");
            }
        }

        return result;
    }

    private static bool HasWritePermission(string path)
    {
        try
        {
            var testFile = Path.Combine(path, $"perm_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool HasDiskSpace(string path, long minimumMegabytes)
    {
        try
        {
            var root = Path.GetPathRoot(path) ?? path;
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => string.Equals(d.RootDirectory.FullName, root, StringComparison.OrdinalIgnoreCase));
            if (drive == null) return false;
            return drive.AvailableFreeSpace >= minimumMegabytes * 1024L * 1024L;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasInternetAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("DataSmartUpdater/1.0"));
            var response = await client.GetAsync(_config.ManifestUrl, HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

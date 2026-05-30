using System.Diagnostics;
using System.Net;
using System.Text.Json;
using DataSmartUpdater;
using DataSmartUpdater.Models;

namespace DataSmartUpdater.Services;

public sealed class SelfUpdateService
{
    private readonly AppConfig _config;
    private readonly LogService _log;
    private readonly HttpClient _http = new();

    public SelfUpdateService(AppConfig config, LogService log)
    {
        _config = config;
        _log = log;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("DataSmartUpdater/1.0");
        _http.Timeout = TimeSpan.FromSeconds(_config.DownloadTimeoutSeconds);
    }

    public async Task<UpdaterManifest?> CheckForUpdateAsync()
    {
        try
        {
            _log.Info("Checking for DataSmartUpdater self-update...");
            var json = await _http.GetStringAsync(_config.UpdaterManifestUrl);
            var manifest = JsonSerializer.Deserialize<UpdaterManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (manifest == null)
            {
                _log.Warn("Updater manifest could not be parsed.");
                return null;
            }

            if (!manifest.Nome.Contains("DataSmartUpdater.exe", StringComparison.OrdinalIgnoreCase))
            {
                _log.Warn("Updater manifest target name does not match DataSmartUpdater.exe.");
                return null;
            }

            if (IsNewerVersion(manifest.Versao, Application.ProductVersion))
            {
                _log.Info($"Self-update available: {manifest.Versao}");
                return manifest;
            }

            _log.Info("No newer self-update found.");
            return null;
        }
        catch (Exception ex)
        {
            _log.Warn($"Self-update check failed: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> PerformUpdateAsync(UpdaterManifest manifest)
    {
        try
        {
            _log.Info($"Downloading self-update: {manifest.Url}");
            var destination = Path.Combine(AppContext.BaseDirectory, "DataSmartUpdater.new.exe");
            var tempDestination = Path.Combine(Path.GetTempPath(), $"DataSmartUpdater_{Guid.NewGuid():N}.tmp");

            var downloadProgress = new Progress<int>(_ => { });
            await DownloadFileAsync(manifest.Url, tempDestination, downloadProgress);

            if (!File.Exists(tempDestination) || new FileInfo(tempDestination).Length < _config.MinimumFileSizeBytes)
            {
                _log.Error("Self-update download failed due to invalid file size.");
                return false;
            }

            if (!HashService.MatchesSha256(tempDestination, manifest.Sha256))
            {
                _log.Error("Self-update SHA256 validation failed.");
                return false;
            }

            File.Copy(tempDestination, destination, true);
            File.Delete(tempDestination);

            var scriptPath = Path.Combine(AppContext.BaseDirectory, "update-self.bat");
            CreateUpdaterScript(scriptPath);

            _log.Info("Self-update downloaded and verified.");
            _log.Info($"Updater script created: {scriptPath}");

            Process.Start(new ProcessStartInfo
            {
                FileName = scriptPath,
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            return true;
        }
        catch (Exception ex)
        {
            _log.Warn($"Self-update failed: {ex.Message}");
            return false;
        }
    }

    private async Task DownloadFileAsync(string url, string destination, IProgress<int> progress)
    {
        if (File.Exists(destination))
            File.Delete(destination);

        using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
        using var headResponse = await _http.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead);

        if (headResponse.StatusCode == HttpStatusCode.NotFound)
            throw new Exception($"File not found on GitHub. URL: {url}");

        if (!headResponse.IsSuccessStatusCode && headResponse.StatusCode != HttpStatusCode.MethodNotAllowed)
            _log.Warn($"Self-update HEAD request returned {headResponse.StatusCode}.");

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new Exception($"File not found on GitHub. URL: {url}");

        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1L;
        var canReport = total > 0;

        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var file = File.Create(destination);

        var buffer = new byte[81920];
        long readTotal = 0;

        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
            if (read == 0)
                break;

            await file.WriteAsync(buffer.AsMemory(0, read));
            readTotal += read;

            if (canReport)
            {
                var percent = (int)((readTotal * 100L) / total);
                progress.Report(Math.Clamp(percent, 0, 100));
            }
        }
    }

    private void CreateUpdaterScript(string scriptPath)
    {
        var content = new[]
        {
            "@echo off",
            "cd /d %~dp0",
            "echo Waiting for DataSmartUpdater.exe to exit...",
            "ping 127.0.0.1 -n 6 > nul",
            "if exist \"DataSmartUpdater.new.exe\" (",
            "  move /Y \"DataSmartUpdater.new.exe\" \"DataSmartUpdater.exe\" > nul",
            "  echo DataSmartUpdater.exe has been updated.",
            ") else (",
            "  echo DataSmartUpdater.new.exe not found.",
            ")",
            "start \"\" \"DataSmartUpdater.exe\""
        };

        File.WriteAllLines(scriptPath, content);
    }

    private static bool IsNewerVersion(string onlineVersion, string localVersion)
    {
        if (Version.TryParse(onlineVersion, out var online) && Version.TryParse(localVersion, out var local))
            return online > local;

        return string.Compare(onlineVersion, localVersion, StringComparison.OrdinalIgnoreCase) > 0;
    }
}

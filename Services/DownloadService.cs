namespace DataSmartUpdater.Services;

using System.Net;

public sealed class DownloadService
{
    private readonly HttpClient _http = new();
    private readonly LogService _log;

    public DownloadService(LogService log)
    {
        _log = log;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("DataSmartUpdater/1.0");
    }

    public async Task DownloadAsync(string url, string destination, IProgress<int> progress, CancellationToken token = default)
    {
        if (File.Exists(destination))
            File.Delete(destination);

        _log.Info($"Downloading {Path.GetFileName(url)}");
        _log.Info($"URL: {url}");
        _log.Info($"Temporary destination: {destination}");

        try
        {
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
            using var headResponse = await _http.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, token);
            if (headResponse.StatusCode == HttpStatusCode.NotFound)
                throw new Exception($"File not found on GitHub. Check if the file exists inside the EXE folder in the repository. URL: {url}");

            if (!headResponse.IsSuccessStatusCode && headResponse.StatusCode != HttpStatusCode.MethodNotAllowed)
                _log.Warn($"HEAD request returned {headResponse.StatusCode} for {url}");
        }
        catch (HttpRequestException ex)
        {
            _log.Warn($"HEAD request failed: {ex.Message}");
        }

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new Exception($"File not found on GitHub. Check if the file exists inside the EXE folder in the repository. URL: {url}");

        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1L;
        var canReport = total > 0;

        await using var stream = await response.Content.ReadAsStreamAsync(token);
        await using var file = File.Create(destination);

        var buffer = new byte[81920];
        long readTotal = 0;
        int read;

        while ((read = await stream.ReadAsync(buffer, token)) > 0)
        {
            await file.WriteAsync(buffer.AsMemory(0, read), token);
            readTotal += read;

            if (canReport)
            {
                var percent = (int)((readTotal * 100L) / total);
                progress.Report(Math.Clamp(percent, 0, 100));
            }
        }

        if (!File.Exists(destination) || new FileInfo(destination).Length < 1024)
            throw new Exception("Arquivo baixado inválido ou muito pequeno.");

        _log.Info($"Download concluído: {destination}");
    }
}

namespace DataSmartUpdater.Services;

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

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
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

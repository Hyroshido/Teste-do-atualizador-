namespace DataSmartUpdater.Services;

public sealed class BackupService
{
    private readonly string _dataDir;
    private readonly string _backupDir;
    private readonly LogService _log;

    public BackupService(string dataDir, LogService log)
    {
        _dataDir = dataDir;
        _backupDir = Path.Combine(dataDir, "Backup");
        _log = log;
        Directory.CreateDirectory(_backupDir);
    }

    public string CreateSessionBackup()
    {
        var path = Path.Combine(_backupDir, $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(path);
        _log.Info($"Pasta de backup criada: {path}");
        return path;
    }

    public void BackupDatabase(string sessionPath)
    {
        var db = Directory.GetFiles(_dataDir, "*", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(f => Path.GetFileName(f).Equals("COMERCIAL.DAT", StringComparison.OrdinalIgnoreCase));

        if (db == null)
        {
            _log.Warn("COMERCIAL.DAT não encontrado.");
            return;
        }

        var dest = Path.Combine(sessionPath, $"COMERCIAL_{DateTime.Now:yyyyMMdd}.DAT");
        File.Copy(db, dest, true);
        _log.Info($"Backup do COMERCIAL.DAT criado: {dest}");
    }

    public string? BackupExe(string exeName, string sessionPath)
    {
        var origin = Path.Combine(_dataDir, exeName);

        if (!File.Exists(origin))
        {
            _log.Warn($"Arquivo local não encontrado para backup: {origin}");
            return null;
        }

        var name = Path.GetFileNameWithoutExtension(exeName);
        var ext = Path.GetExtension(exeName);
        var backup = Path.Combine(sessionPath, $"{name}_{DateTime.Now:yyyyMMdd}{ext}");

        File.Copy(origin, backup, true);
        _log.Info($"Backup criado: {backup}");

        return backup;
    }
}

namespace DataSmartUpdater.Services;

public sealed class BackupService
{
    private readonly string _dataDir;
    private readonly string _exeDir;
    private readonly string _backupRoot;
    private readonly string _backupBanco;
    private readonly string _backupExecutaveis;
    private readonly string _backupLogs;
    private readonly string _backupHistorico;
    private readonly string _backupConfig;
    private readonly LogService _log;

    public BackupService(string dataDir, LogService log)
    {
        _dataDir = dataDir;
        _exeDir = Path.Combine(_dataDir, "EXE");
        _backupRoot = Path.Combine(_dataDir, "Backup");
        _backupBanco = Path.Combine(_backupRoot, "Banco");
        _backupExecutaveis = Path.Combine(_backupRoot, "Executaveis");
        _backupLogs = Path.Combine(_backupRoot, "Logs");
        _backupHistorico = Path.Combine(_backupRoot, "Historico");
        _backupConfig = Path.Combine(_backupRoot, "Config");
        _log = log;

        Directory.CreateDirectory(_exeDir);
        Directory.CreateDirectory(_backupRoot);
        Directory.CreateDirectory(_backupBanco);
        Directory.CreateDirectory(_backupExecutaveis);
        Directory.CreateDirectory(_backupLogs);
        Directory.CreateDirectory(_backupHistorico);
        Directory.CreateDirectory(_backupConfig);
    }

    public void MigrateRootExecutables()
    {
        var knownFiles = new[]
        {
            "SmartNFe.exe",
            "SmartNFSe.exe",
            "SmartFood.exe",
            "SmartCTE.exe",
            "SPED.exe",
            "SPED_Fiscal.exe",
            "SmartBackup.exe",
            "SmartBackup_SmartImoveis.exe",
            "SmartContador.exe",
            "SmartTools.exe",
            "SmartNFSe_260513.exe",
            "SmartNFe_Incopal.exe"
        };

        try
        {
            Directory.CreateDirectory(_exeDir);
            _log.Info("Verificando migração de executáveis antigos.");

            foreach (var fileName in knownFiles)
            {
                var rootFile = Path.Combine(_dataDir, fileName);
                var exeFile = Path.Combine(_exeDir, fileName);

                if (!File.Exists(rootFile))
                    continue;

                if (File.Exists(exeFile))
                {
                    _log.Warn($"File already exists in EXE, kept in root: {fileName}");
                    continue;
                }

                try
                {
                    File.Move(rootFile, exeFile);
                    _log.Info($"File migrated from root to EXE: {fileName}");
                }
                catch (Exception ex)
                {
                    _log.Warn($"Failed to migrate {fileName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _log.Warn($"EXE migration failed: {ex.Message}");
        }
    }

    public string CreateSessionBackup()
    {
        var path = Path.Combine(_backupRoot, $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(Path.Combine(path, "Banco"));
        Directory.CreateDirectory(Path.Combine(path, "Executaveis"));
        Directory.CreateDirectory(Path.Combine(path, "Logs"));
        Directory.CreateDirectory(Path.Combine(path, "Historico"));
        Directory.CreateDirectory(Path.Combine(path, "Config"));
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

        var dest = Path.Combine(sessionPath, "Banco", $"COMERCIAL_{DateTime.Now:yyyyMMdd_HHmmss}.DAT");
        File.Copy(db, dest, true);
        _log.Info($"Backup do COMERCIAL.DAT criado: {dest}");
    }

    public string? BackupExe(string exeName, string sessionPath)
    {
        var origin = FindLocalExePath(exeName);

        if (origin == null)
        {
            _log.Warn($"Arquivo local não encontrado para backup: {exeName}");
            return null;
        }

        var name = Path.GetFileNameWithoutExtension(exeName);
        var ext = Path.GetExtension(exeName);
        var backup = Path.Combine(sessionPath, "Executaveis", $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}");

        File.Copy(origin, backup, true);
        _log.Info($"Backup criado: {backup}");

        return backup;
    }

    public string? FindLocalExePath(string exeName)
    {
        var exePath = Path.Combine(_exeDir, exeName);
        if (File.Exists(exePath))
            return exePath;

        var rootPath = Path.Combine(_dataDir, exeName);
        if (File.Exists(rootPath))
            return rootPath;

        return null;
    }

    public void RestoreSession(string sessionPath)
    {
        try
        {
            var executaveis = Directory.GetFiles(Path.Combine(sessionPath, "Executaveis"), "*.exe", SearchOption.TopDirectoryOnly);
            foreach (var backupFile in executaveis)
            {
                var targetFileName = GetOriginalFileName(backupFile);
                var target = Path.Combine(_exeDir, targetFileName);
                File.Copy(backupFile, target, true);
                _log.Info($"Rollback: restaurado {targetFileName}");
            }
        }
        catch (Exception ex)
        {
            _log.Warn($"Falha ao restaurar backup de sessão: {ex.Message}");
        }
    }

    private static string GetOriginalFileName(string backupFile)
    {
        var fileName = Path.GetFileName(backupFile) ?? string.Empty;
        var ext = Path.GetExtension(fileName);
        var name = Path.GetFileNameWithoutExtension(fileName);

        var separator = name.LastIndexOf('_');
        if (separator > 0 && separator + 1 < name.Length)
        {
            var maybeTimestamp = name[(separator + 1)..];
            if (maybeTimestamp.Length == 15 && long.TryParse(maybeTimestamp.Replace("_", string.Empty), out _))
            {
                return name[..separator] + ext;
            }
        }

        return fileName;
    }
}

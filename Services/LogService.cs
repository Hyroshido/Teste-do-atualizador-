namespace DataSmartUpdater.Services;

public sealed class LogService
{
    public string LogFile { get; }

    public LogService(string backupDir)
    {
        Directory.CreateDirectory(backupDir);
        LogFile = Path.Combine(backupDir, $"Atualizador_{DateTime.Now:yyyyMMdd_HHmmss}.log");
    }

    public void Info(string msg) => Write("INFO", msg);
    public void Warn(string msg) => Write("AVISO", msg);
    public void Error(string msg) => Write("ERRO", msg);

    private void Write(string type, string msg)
    {
        try
        {
            File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {msg}{Environment.NewLine}");
        }
        catch { }
    }
}

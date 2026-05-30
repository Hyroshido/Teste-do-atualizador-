namespace DataSmartUpdater.Models;

public sealed class ModuleInfo
{
    public string Nome { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Versao { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;

    public string LocalPath { get; set; } = string.Empty;
    public string LocalVersion { get; set; } = string.Empty;
    public ModuleUpdateState Estado { get; set; } = ModuleUpdateState.Unknown;

    public string LocalSource => string.IsNullOrWhiteSpace(LocalPath)
        ? "Nao instalado"
        : LocalPath.Contains("\\EXE\\", StringComparison.OrdinalIgnoreCase)
            ? "EXE"
            : "Raiz";

    public string VersaoDisponivel => string.IsNullOrWhiteSpace(Versao) ? "Desconhecido" : Versao;
    public string TamanhoText => TamanhoBytes > 0 ? FormatBytes(TamanhoBytes) : "Desconhecido";
    public string DataText => string.IsNullOrWhiteSpace(Data) ? "-" : Data;
    public string StatusText => Estado switch
    {
        ModuleUpdateState.UpToDate => "Atualizado",
        ModuleUpdateState.UpdateAvailable => "Atualizacao disponivel",
        ModuleUpdateState.NotInstalled => "Nao instalado",
        ModuleUpdateState.Error => "Erro",
        _ => "Desconhecido"
    };

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
    }
}

public enum ModuleUpdateState
{
    Unknown,
    UpToDate,
    UpdateAvailable,
    NotInstalled,
    Error
}

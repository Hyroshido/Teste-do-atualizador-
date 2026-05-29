namespace DataSmartUpdater.Models;

public sealed class UpdateHistoryEntry
{
    public DateTime DataHora { get; set; }
    public string Usuario { get; set; } = "Administrador";
    public string Status { get; set; } = "Concluído";
    public List<string> ArquivosAtualizados { get; set; } = new();
    public string TempoTotal { get; set; } = string.Empty;
    public string VelocidadeMedia { get; set; } = string.Empty;
    public string BackupCriado { get; set; } = string.Empty;
    public string StatusBanco { get; set; } = string.Empty;
}

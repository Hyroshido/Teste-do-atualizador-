namespace DataSmartUpdater.Models;

public sealed class UpdaterManifest
{
    public string Nome { get; set; } = string.Empty;
    public string Versao { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public bool Obrigatorio { get; set; }
    public string Notas { get; set; } = string.Empty;
}

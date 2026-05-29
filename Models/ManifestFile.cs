namespace DataSmartUpdater.Models;

public sealed class ManifestFile
{
    public string Versao { get; set; } = "";
    public List<ManifestItem> Arquivos { get; set; } = new();
}

namespace DataSmartUpdater.Models;

public sealed class ManifestItem
{
    public string Nome { get; set; } = "";
    public string Url { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Versao { get; set; } = "";
    public long TamanhoBytes { get; set; }
    public string Sha256 { get; set; } = "";
    public string Data { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
}

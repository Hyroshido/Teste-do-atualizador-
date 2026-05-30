using DataSmartUpdater.Models;
using System.Text.Json;

namespace DataSmartUpdater.Services;

public sealed class ManifestService
{
    private readonly HttpClient _http = new();

    public async Task<ManifestFile> LoadAsync(string manifestUrl)
    {
        try
        {
            var json = await _http.GetStringAsync(manifestUrl);
            var manifest = JsonSerializer.Deserialize<ManifestFile>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (manifest != null && manifest.Arquivos.Count > 0)
                return manifest;
        }
        catch
        {
        }

        return new ManifestFile
        {
            Versao = DateTime.Now.ToString("yyyy.MM.dd"),
            Arquivos = new List<ManifestItem>
            {
                new() { Nome = "SmartNFe.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SmartNFe.exe", Descricao = "Modulo de NF-e: emissao, transmissao e gerenciamento de notas fiscais eletronicas.", Versao = "2.5.18" },
                new() { Nome = "SmartNFSe.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SmartNFSe.exe", Descricao = "Modulo de NFS-e: emissao e integracao de notas fiscais de servico.", Versao = "2.5.18" },
                new() { Nome = "SmartFood.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SmartFood.exe", Descricao = "Modulo Food: restaurante, delivery e balcao.", Versao = "2.5.18" },
                new() { Nome = "SmartCTE.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SmartCTE.exe", Descricao = "Modulo CT-e: emissao e manutencao de conhecimento de transporte.", Versao = "2.5.18" },
                new() { Nome = "SPED.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SPED.exe", Descricao = "Modulo SPED: geracao e manutencao de arquivos fiscais.", Versao = "2.5.18" }
            }
        };
    }
}

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
                new() { Nome = "SmartNFe.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SmartNFe.exe", Descricao = "Módulo de NF-e: emissão, transmissão e gerenciamento de notas fiscais eletrônicas.", Versao = "2.5.18" },
                new() { Nome = "SmartNFSe.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SmartNFSe.exe", Descricao = "Módulo de NFS-e: emissão e integração de notas fiscais de serviço.", Versao = "2.5.18" },
                new() { Nome = "SmartFood.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SmartFood.exe", Descricao = "Módulo Food: restaurante, delivery e balcão.", Versao = "2.5.18" },
                new() { Nome = "SmartCTE.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SmartCTE.exe", Descricao = "Módulo CT-e: emissão e manutenção de conhecimento de transporte.", Versao = "2.5.18" },
                new() { Nome = "SPED.exe", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/EXE/SPED.exe", Descricao = "Módulo SPED: geração e manutenção de arquivos fiscais.", Versao = "2.5.18" }
            }
        };
    }
}

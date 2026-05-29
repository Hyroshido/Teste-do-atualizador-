# DataSmartUpdater

Atualizador profissional em C# + .NET 8 + Windows Forms.

## Como funciona

O cliente executa apenas:

```txt
DataSmartUpdater.exe
```

O executável:

1. Baixa o `manifest.json` do GitHub.
2. Mostra os módulos disponíveis.
3. Permite selecionar um ou vários módulos.
4. Cria backup do `COMERCIAL.DAT`.
5. Cria backup dos `.exe` antigos com data, exemplo: `SPED_20260529.exe`.
6. Baixa os arquivos novos.
7. Substitui os módulos.
8. Abre o `atualizador de banco de dados.exe`.
9. Clica automaticamente apenas em `Carregar arquivos`.
10. Não clica em `Processar arquivos`.

## Arquivos importantes

```txt
DataSmartUpdater.exe      -> arquivo que vai para a área de trabalho do cliente
appsettings.json          -> configuração do atualizador
manifest.json             -> arquivo que deve subir no GitHub
```

## Como publicar

No computador de desenvolvimento:

```powershell
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

O executável ficará em:

```txt
bin\Release\net8.0-windows\win-x64\publish\DataSmartUpdater.exe
```

Copie para a área de trabalho do cliente:

```txt
DataSmartUpdater.exe
appsettings.json
```

## Onde colocar o manifest no GitHub

Suba o arquivo `manifest.json` para o repositório:

```txt
https://github.com/Hyroshido/Teste-do-atualizador-
```

O link RAW esperado é:

```txt
https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/manifest.json
```

## Estrutura recomendada no GitHub

```txt
Teste-do-atualizador-/
├── manifest.json
├── SmartNFe.exe
├── SmartNFSe.exe
├── SmartFood.exe
├── SmartCTE.exe
└── SPED.exe
```

## Observação

O `DataSmartUpdater.exe` não precisa ser alterado sempre que mudar os módulos.
Você só atualiza o `manifest.json` e os `.exe` no GitHub.

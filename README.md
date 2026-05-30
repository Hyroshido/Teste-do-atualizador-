# 🚀 DataSmart Deploy Center

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Windows](https://img.shields.io/badge/Windows-Forms-0078D4)
![Status](https://img.shields.io/badge/Status-Enterprise-blue)
![License](https://img.shields.io/badge/License-Internal-lightgrey)

## 📌 Visão Geral

`DataSmart Deploy Center` é o motor de implantação Windows Forms para clientes DataSmart. Ele carrega um manifesto do GitHub, baixa módulos em um diretório `EXE`, faz backups seguros, migra executáveis legados da raiz e automatiza o fluxo interno do `Atualizador de Banco de Dados.exe`.

O aplicativo também suporta autoatualização do próprio `DataSmartUpdater.exe` por meio de um manifesto dedicado `updater-manifest.json`.

## ✨ Funcionalidades Principais

- 🌐 Atualização de módulos via GitHub usando `manifest.json`
- 📦 Implantação organizada no diretório `EXE`
- 🔁 Migração segura de executáveis legados da raiz para `EXE`
- 💾 Backup automático de `COMERCIAL.DAT` antes da implantação
- 🛡️ Validação de download com SHA256 e tamanho mínimo
- 🧩 Seleção clara de módulos e status de atualização
- ⚙️ Automação do atualizador de banco de dados com opção de fechar
- 📜 Console de log ao vivo e arquivos de log persistentes
- 🔄 Autoatualização do `DataSmartUpdater.exe`
- 🧠 Detecção inteligente do caminho do DataSmart e configuração persistente

## 🗂️ Estrutura do Projeto

```
DataSmartUpdater/
├── EXE/
│   ├── SmartNFe.exe
│   ├── SmartNFSe.exe
│   ├── SmartFood.exe
│   ├── SmartCTE.exe
│   ├── SPED.exe
│   ├── SPED_Fiscal.exe
│   ├── SmartBackup.exe
│   ├── SmartBackup_SmartImoveis.exe
│   ├── SmartContador.exe
│   ├── SmartTools.exe
│   ├── SmartNFSe_260513.exe
│   └── SmartNFe_Incopal.exe
├── Imagens/
├── Models/
├── Services/
├── publish/
├── app.manifest
├── appsettings.example.json
├── manifest.json
├── updater-manifest.json
├── DataSmartUpdater.csproj
├── Program.cs
├── MainForm.cs
└── README.md
```

## 🖥️ Estrutura do Cliente

```
C:\DataSmart
├── EXE
│   ├── SmartNFe.exe
│   ├── SmartNFSe.exe
│   ├── SmartFood.exe
│   ├── SmartCTE.exe
│   └── SPED.exe
├── Backup
│   ├── Banco
│   ├── Executaveis
│   ├── Logs
│   ├── Historico
│   └── Config
│       └── appsettings.json
├── Atualizador de Banco de Dados.exe
├── COMERCIAL.DAT
└── outros arquivos do DataSmart
```

## 🚀 Fluxo de Atualização

1. O atualizador carrega `C:\DataSmart\Backup\Config\appsettings.json`.
2. Se o caminho do DataSmart estiver inválido, ele procura caminhos conhecidos e atalhos na área de trabalho.
3. O atualizador carrega `manifest.json` do GitHub.
4. Os módulos são exibidos com origem local, versão local, versão disponível e status de atualização.
5. Os módulos selecionados são copiados em backup, baixados, validados e implantados em `C:\DataSmart\EXE`.
6. O atualizador interno de banco de dados é aberto e processado conforme configurado.
7. Logs e histórico são gravados em `C:\DataSmart\Backup`.

## 📦 Publicar Nova Versão de Módulo

1. Coloque o novo executável no diretório `EXE/` do repositório.
2. Atualize `manifest.json` com a versão nova e a URL raw do GitHub.
3. Faça commit e envie as alterações.
4. O cliente fará download do módulo a partir do novo caminho do manifesto.

## 🔁 Publicar Nova Versão do DataSmartUpdater.exe

1. Gere o novo build de `DataSmartUpdater.exe`.
2. Faça upload para o diretório `publish/` do repositório.
3. Atualize `updater-manifest.json` com a versão, URL e notas de release.
4. Os clientes receberão a notificação de autoatualização ao iniciar o updater.

## 🛠️ Compilar

```bash
dotnet build -c Release
```

## 📤 Publicar EXE Único

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
```

## 📍 Entrega ao Cliente

Entregue apenas:

- `DataSmartUpdater.exe`

O atualizador gerencia o download e a implantação dos módulos usando os manifestos GitHub e a estrutura local `C:\DataSmart`.

## 🧾 Logs

Os logs são gravados em:

- `C:\DataSmart\Backup\Logs`

## 🔐 Segurança

O atualizador inclui:

- Validação SHA256 de downloads
- Validação de tamanho mínimo antes da implantação
- Estágio temporário de download antes da substituição
- Autoatualização segura via script auxiliar
- Migração de executáveis legados com log de evento

## 🧯 Solução de Problemas

- HTTP 404: verifique a URL raw do GitHub e o caminho sensível a maiúsculas `EXE`.
- Sem internet: o atualizador exibirá falha no manifesto e continuará com segurança.
- Arquivo em uso: feche os módulos DataSmart antes de implantar.
- GitHub indisponível: tente novamente mais tarde.
- Pasta DataSmart não encontrada: selecione a pasta quando solicitado.
- Botão do atualizador de banco não encontrado: o atualizador registra aviso e mantém a janela aberta.
- Autoatualização falhou: o executável atual permanece intacto.

## 🧭 Roadmap

- Assinatura de código para todos os binários
- CDN para downloads mais rápidos
- Patches de atualização incrementais
- Dashboard web para gerenciamento de releases
- Canais de release e modo silencioso
- Dashboard de telemetria interna

## 👤 Mantenedor

DataSmart Support / TI Interno

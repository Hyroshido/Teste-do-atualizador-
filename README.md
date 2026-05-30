# 🚀 DataSmart Deploy Center

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Windows](https://img.shields.io/badge/Windows-Forms-0078D4)
![Status](https://img.shields.io/badge/Status-Enterprise-blue)
![License](https://img.shields.io/badge/License-Internal-lightgrey)

## 📌 Overview

`DataSmart Deploy Center` is the new Windows Forms deployment engine for DataSmart clients. It loads a GitHub manifest, downloads module executables into a structured `EXE` folder, performs safe backups, migrates legacy root executables, and automates the internal `Atualizador de Banco de Dados.exe` flow.

This tool also supports self-updating the updater itself from a dedicated `updater-manifest.json` release manifest.

## ✨ Key Features

- 🌐 GitHub-based module updates using `manifest.json`
- 📦 Organized `EXE` folder deployment
- 🔁 Legacy root executable migration support
- 💾 Automatic `COMERCIAL.DAT` backup before deployment
- 🛡️ Safe update validation with SHA256 support
- 🧩 Clear module selection and update status
- ⚙️ Database updater automation and optional closing
- 📜 Live log console and persistent log files
- 🔄 Self-update for `DataSmartUpdater.exe`
- 🧠 Smart `DataSmart` path detection and configuration

## 🗂️ Project Structure

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

## 🖥️ Client Machine Structure

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
└── other DataSmart files
```

## 🚀 How the Update Flow Works

1. The updater loads `C:\DataSmart\Backup\Config\appsettings.json`.
2. If the DataSmart installation path is invalid, it searches known paths and desktop shortcuts.
3. The updater loads `manifest.json` from GitHub.
4. Modules are displayed with local source, local version, available version, and update status.
5. Selected modules are backed up, downloaded, validated, and deployed to `C:\DataSmart\EXE`.
6. The internal database updater is launched and optionally processed.
7. Logs and history are persisted under `C:\DataSmart\Backup`.

## 📦 How to Publish a New Module Version

1. Place the new executable inside the repository `EXE/` folder.
2. Update `manifest.json` with the new version and GitHub raw URL.
3. Commit and push the changes.
4. The client will download the module from the new manifest path.

## 🔁 How to Publish a New DataSmartUpdater.exe Version

1. Publish the new `DataSmartUpdater.exe` build.
2. Upload it into the repository `publish/` folder.
3. Update `updater-manifest.json` with the new version, URL, and release notes.
4. Clients will be prompted to self-update when they start the updater.

## 🛠️ Build

```bash
dotnet build -c Release
```

## 📤 Publish Single EXE

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
```

## 📍 What to Deliver to the Client

Deliver only:

- `DataSmartUpdater.exe`

The updater manages module downloads and deployment using GitHub manifests and the client-side `C:\DataSmart` layout.

## 🧾 Logs

Logs are written to:

- `C:\DataSmart\Backup\Logs`

## 🔐 Security

The updater now includes:

- SHA256 validation support for downloads
- File size validation before deployment
- Temporary download staging before replacement
- Safe self-update via helper script
- Legacy executable migration with logging

## 🧯 Troubleshooting

- HTTP 404: Verify the GitHub raw URL and case-sensitive `EXE` folder path.
- No internet: the updater will show a manifest load error and continue safely.
- File in use: close the running DataSmart modules before deploying.
- GitHub unavailable: retry later.
- DataSmart folder not found: choose the installation folder when prompted.
- Database updater button not found: the updater logs a friendly warning and leaves the updater open.
- Self-update failed: the current executable stays intact.

## 🧭 Roadmap

- Code signing for all binaries
- CDN distribution for faster downloads
- Incremental update patches
- Web dashboard for release tracking
- Release channels and silent mode
- Internal telemetry dashboard

## 👤 Maintainer

DataSmart Support / Internal IT

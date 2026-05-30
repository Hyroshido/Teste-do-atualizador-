# Publicação

Para publicar uma nova versão do atualizador:

1. Execute `dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true`.
2. Copie `DataSmartUpdater.exe` para `publish/`.
3. Atualize `updater-manifest.json` com a nova versão e URL.
4. Commit e push as mudanças.

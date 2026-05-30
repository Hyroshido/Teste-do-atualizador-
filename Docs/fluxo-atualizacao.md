# Fluxo de Atualização

1. O app carrega o `appsettings.json` em `C:\DataSmart\Backup\Config`.
2. Detecta o caminho do DataSmart via configuração, caminhos conhecidos ou atalho.
3. Carrega `manifest.json` do GitHub.
4. Exibe módulos e fontes locais (EXE ou root).
5. Faz backup de `COMERCIAL.DAT` e executáveis selecionados.
6. Baixa, valida e substitui arquivos em `C:\DataSmart\EXE`.
7. Executa `Atualizador de Banco de Dados.exe` quando selecionado.
8. Registra histórico e logs.

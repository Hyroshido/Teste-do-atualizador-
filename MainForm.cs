using DataSmartUpdater.Models;
using DataSmartUpdater.Services;
using System.Diagnostics;

namespace DataSmartUpdater;

public sealed class MainForm : Form
{
    private readonly AppConfig _config;
    private readonly string _dataDir;
    private readonly string _exeDir;
    private readonly string _backupRoot;
    private readonly LogService _log;
    private readonly ManifestService _manifestService;
    private readonly DownloadService _downloadService;
    private readonly BackupService _backupService;
    private readonly EnvironmentValidatorService _validatorService;
    private readonly HistoryService _historyService;
    private readonly BancoUpdaterService _bancoUpdaterService;

    private readonly Label _lblAppTitle = new();
    private readonly Label _lblSubtitle = new();
    private readonly Label _lblAppVersion = new();
    private readonly Label _lblManifestVersion = new();
    private readonly Label _lblConnection = new();
    private readonly Label _lblStatus = new();
    private readonly PictureBox _logoPicture = new();
    private readonly PictureBox _avatarPicture = new();
    private readonly Label[] _dashboardValue = new Label[6];
    private readonly Label[] _dashboardLabel = new Label[6];
    private readonly DataGridView _gridModules = new();
    private readonly RichTextBox _logConsole = new();
    private readonly ProgressBar _progressTotal = new();
    private readonly Label _lblProgressPercent = new();
    private readonly RadioButton _rbNoDb = new();
    private readonly RadioButton _rbOnlyLoad = new();
    private readonly RadioButton _rbLoadProcess = new();
    private readonly RadioButton _rbLoadProcessClose = new();
    private readonly Button _btnSelectAll = new();
    private readonly Button _btnClearAll = new();
    private readonly Button _btnRefresh = new();
    private readonly Button _btnUpdate = new();
    private readonly Button _btnOpenLog = new();
    private readonly Button _btnClose = new();

    private readonly List<ModuleInfo> _modules = new();
    private ManifestFile _manifest = new();
    private bool _environmentOk;

    public MainForm(AppConfig config)
    {
        _config = config;
        _dataDir = _config.DefaultInstallPath;
        _exeDir = Path.Combine(_dataDir, "EXE");
        _backupRoot = Path.Combine(_dataDir, _config.BackupRoot);

        Directory.CreateDirectory(_dataDir);
        Directory.CreateDirectory(_exeDir);
        Directory.CreateDirectory(_backupRoot);

        _log = new LogService(_backupRoot);
        _manifestService = new ManifestService();
        _downloadService = new DownloadService(_log);
        _backupService = new BackupService(_dataDir, _log);
        _validatorService = new EnvironmentValidatorService(_config, _dataDir);
        _historyService = new HistoryService(_backupRoot);
        _bancoUpdaterService = new BancoUpdaterService(_log);

        BuildUi();
        Shown += async (_, _) => await InitializeAsync();
    }

    private void BuildUi()
    {
        Text = _config.AppName;
        Width = 980;
        Height = 820;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(12, 18, 32);
        Font = new Font("Segoe UI", 9);

        var blue = Color.FromArgb(56, 189, 248);
        var cardBackground = Color.FromArgb(22, 30, 48);
        var panelBackground = Color.FromArgb(18, 24, 38);
        var textGray = Color.FromArgb(168, 178, 198);

        var imageDir = Path.Combine(AppContext.BaseDirectory, "Imagens");
        var logoImage = LoadLogoImage(imageDir);
        var heroImage = LoadImage(imageDir, "Marcos.png");

        var headerPanel = new Panel
        {
            Left = 20,
            Top = 20,
            Width = 940,
            Height = 160,
            BackColor = Color.FromArgb(20, 28, 50)
        };
        Controls.Add(headerPanel);

        if (logoImage is not null)
        {
            _logoPicture.Image = logoImage;
            _logoPicture.Left = 20;
            _logoPicture.Top = 18;
            _logoPicture.Width = 64;
            _logoPicture.Height = 64;
            _logoPicture.SizeMode = PictureBoxSizeMode.Zoom;
            headerPanel.Controls.Add(_logoPicture);
        }

        var headerLeft = logoImage is not null ? 100 : 20;

        _lblAppTitle.Text = _config.AppName;
        _lblAppTitle.ForeColor = blue;
        _lblAppTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        _lblAppTitle.AutoSize = true;
        _lblAppTitle.Left = headerLeft;
        _lblAppTitle.Top = 20;
        headerPanel.Controls.Add(_lblAppTitle);

        _lblSubtitle.Text = _config.AppSubtitle;
        _lblSubtitle.ForeColor = Color.White;
        _lblSubtitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        _lblSubtitle.AutoSize = true;
        _lblSubtitle.Left = headerLeft;
        _lblSubtitle.Top = 54;
        headerPanel.Controls.Add(_lblSubtitle);

        _lblAppVersion.Text = "Vers�o do Atualizador: 1.0.0";
        _lblAppVersion.ForeColor = textGray;
        _lblAppVersion.AutoSize = true;
        _lblAppVersion.Left = headerLeft;
        _lblAppVersion.Top = 82;
        headerPanel.Controls.Add(_lblAppVersion);

        _lblManifestVersion.Text = "Vers�o do Manifest: desconhecida";
        _lblManifestVersion.ForeColor = textGray;
        _lblManifestVersion.AutoSize = true;
        _lblManifestVersion.Left = headerLeft;
        _lblManifestVersion.Top = 104;
        headerPanel.Controls.Add(_lblManifestVersion);

        _lblConnection.Text = "? Online - Pronto para atualizar seu ambiente.";
        _lblConnection.ForeColor = Color.FromArgb(122, 220, 191);
        _lblConnection.AutoSize = true;
        _lblConnection.Left = headerLeft;
        _lblConnection.Top = 126;
        headerPanel.Controls.Add(_lblConnection);

        if (heroImage is not null)
        {
            _avatarPicture.Image = heroImage;
            _avatarPicture.Left = 820;
            _avatarPicture.Top = 10;
            _avatarPicture.Width = 110;
            _avatarPicture.Height = 140;
            _avatarPicture.SizeMode = PictureBoxSizeMode.Zoom;
            headerPanel.Controls.Add(_avatarPicture);
        }

        var dashboardPanel = new Panel
        {
            Left = 20,
            Top = 200,
            Width = 940,
            Height = 120,
            BackColor = Color.Transparent
        };
        Controls.Add(dashboardPanel);

        var cardLabels = new[]
        {
            "M�dulos Instalados",
            "Atualiza��es Dispon�veis",
            "�ltima Implanta��o",
            "Espa�o Livre",
            "Status do Banco",
            "Status da Conex�o"
        };

        for (var i = 0; i < cardLabels.Length; i++)
        {
            var card = CreateMetricCard(cardLabels[i]);
            card.Left = 10 + (i * 150);
            card.Top = 0;
            dashboardPanel.Controls.Add(card);
            _dashboardValue[i] = (Label)card.Controls[0];
            _dashboardLabel[i] = (Label)card.Controls[1];
        }

        var moduleCard = new Panel
        {
            Left = 20,
            Top = 330,
            Width = 940,
            Height = 260,
            BackColor = cardBackground,
            Padding = new Padding(16)
        };
        Controls.Add(moduleCard);

        moduleCard.Controls.Add(new Label
        {
            Text = "Lista de M�dulos",
            ForeColor = blue,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Left = 0,
            Top = 0
        });

        _gridModules.Left = 0;
        _gridModules.Top = 36;
        _gridModules.Width = 908;
        _gridModules.Height = 170;
        _gridModules.BackgroundColor = panelBackground;
        _gridModules.BorderStyle = BorderStyle.None;
        _gridModules.EnableHeadersVisualStyles = false;
        _gridModules.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(17, 24, 39);
        _gridModules.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _gridModules.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _gridModules.RowHeadersVisible = false;
        _gridModules.AllowUserToAddRows = false;
        _gridModules.AllowUserToDeleteRows = false;
        _gridModules.AllowUserToResizeRows = false;
        _gridModules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridModules.ForeColor = Color.White;
        _gridModules.RowTemplate.Height = 32;
        _gridModules.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _gridModules.RowTemplate.DefaultCellStyle.BackColor = Color.FromArgb(16, 22, 37);
        _gridModules.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(56, 189, 248);
        _gridModules.CellValueChanged += (_, _) => UpdateSelectionCount();
        _gridModules.CurrentCellDirtyStateChanged += (_, _) => { if (_gridModules.IsCurrentCellDirty) _gridModules.CommitEdit(DataGridViewDataErrorContexts.Commit); };

        _gridModules.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Selecionar", HeaderText = "Selecionar", Width = 40 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nome", HeaderText = "Nome", FillWeight = 120 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Descricao", HeaderText = "Descri��o", FillWeight = 220 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "VersaoAtual", HeaderText = "Vers�o Atual", FillWeight = 70 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "VersaoDisponivel", HeaderText = "Vers�o Nova", FillWeight = 70 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tamanho", HeaderText = "Tamanho", FillWeight = 70 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Data", HeaderText = "Data", FillWeight = 80 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", FillWeight = 80 });
        moduleCard.Controls.Add(_gridModules);

        ConfigureButton(_btnSelectAll, "Selecionar todos", 0, 214, 140, 32, panelBackground, Color.White, moduleCard);
        ConfigureButton(_btnClearAll, "Limpar sele��o", 150, 214, 140, 32, panelBackground, Color.White, moduleCard);
        _btnSelectAll.Click += (_, _) => SetAllSelection(true);
        _btnClearAll.Click += (_, _) => SetAllSelection(false);

        var databaseCard = new Panel
        {
            Left = 20,
            Top = 606,
            Width = 620,
            Height = 180,
            BackColor = cardBackground,
            Padding = new Padding(16)
        };
        Controls.Add(databaseCard);

        databaseCard.Controls.Add(new Label
        {
            Text = "Atualizador de Banco",
            ForeColor = blue,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Left = 0,
            Top = 0
        });

        _rbNoDb.Text = "N�o executar";
        _rbNoDb.Left = 0;
        _rbNoDb.Top = 30;
        _rbNoDb.Width = 260;
        _rbNoDb.ForeColor = Color.White;
        _rbNoDb.Checked = true;
        databaseCard.Controls.Add(_rbNoDb);

        _rbOnlyLoad.Text = "Apenas carregar arquivos";
        _rbOnlyLoad.Left = 0;
        _rbOnlyLoad.Top = 58;
        _rbOnlyLoad.Width = 260;
        _rbOnlyLoad.ForeColor = Color.White;
        databaseCard.Controls.Add(_rbOnlyLoad);

        _rbLoadProcess.Text = "Carregar e processar arquivos";
        _rbLoadProcess.Left = 0;
        _rbLoadProcess.Top = 86;
        _rbLoadProcess.Width = 260;
        _rbLoadProcess.ForeColor = Color.White;
        databaseCard.Controls.Add(_rbLoadProcess);

        _rbLoadProcessClose.Text = "Carregar, processar e fechar";
        _rbLoadProcessClose.Left = 0;
        _rbLoadProcessClose.Top = 114;
        _rbLoadProcessClose.Width = 260;
        _rbLoadProcessClose.ForeColor = Color.White;
        databaseCard.Controls.Add(_rbLoadProcessClose);

        _lblStatus.Text = "Inicializando...";
        _lblStatus.ForeColor = Color.White;
        _lblStatus.Left = 0;
        _lblStatus.Top = 150;
        _lblStatus.Width = 560;
        databaseCard.Controls.Add(_lblStatus);

        var logCard = new Panel
        {
            Left = 650,
            Top = 606,
            Width = 310,
            Height = 180,
            BackColor = cardBackground,
            Padding = new Padding(12)
        };
        Controls.Add(logCard);

        logCard.Controls.Add(new Label
        {
            Text = "Log em Tempo Real",
            ForeColor = blue,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Left = 0,
            Top = 0
        });

        _logConsole.Left = 0;
        _logConsole.Top = 28;
        _logConsole.Width = 288;
        _logConsole.Height = 140;
        _logConsole.BackColor = Color.FromArgb(12, 18, 32);
        _logConsole.ForeColor = Color.FromArgb(180, 210, 235);
        _logConsole.BorderStyle = BorderStyle.None;
        _logConsole.ReadOnly = true;
        _logConsole.Font = new Font("Consolas", 9);
        logCard.Controls.Add(_logConsole);

        var footerPanel = new Panel
        {
            Left = 20,
            Top = 800,
            Width = 940,
            Height = 60,
            BackColor = Color.FromArgb(18, 24, 38)
        };
        Controls.Add(footerPanel);

        _progressTotal.Left = 20;
        _progressTotal.Top = 18;
        _progressTotal.Width = 600;
        _progressTotal.Height = 18;
        _progressTotal.Minimum = 0;
        _progressTotal.Maximum = 100;
        footerPanel.Controls.Add(_progressTotal);

        _lblProgressPercent.Text = "0%";
        _lblProgressPercent.ForeColor = blue;
        _lblProgressPercent.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _lblProgressPercent.Left = 640;
        _lblProgressPercent.Top = 15;
        _lblProgressPercent.Width = 80;
        footerPanel.Controls.Add(_lblProgressPercent);

        ConfigureButton(_btnRefresh, "Atualizar vis�o", 730, 14, 120, 32, panelBackground, Color.White, footerPanel);
        ConfigureButton(_btnUpdate, "Implantar Agora", 620, 14, 100, 32, blue, Color.White, footerPanel);
        ConfigureButton(_btnOpenLog, "Abrir log", 850, 14, 80, 32, panelBackground, Color.White, footerPanel);
        ConfigureButton(_btnClose, "Fechar", 850, 14, 80, 32, panelBackground, Color.White, footerPanel);

        _btnUpdate.Click += async (_, _) => await RunUpdateAsync();
        _btnClose.Click += (_, _) => Close();
        _btnRefresh.Click += async (_, _) => await InitializeAsync();
        _btnOpenLog.Click += (_, _) => OpenLogFile();
    }

    private Panel CreateMetricCard(string title)
    {
        var card = new Panel
        {
            Width = 150,
            Height = 110,
            BackColor = Color.FromArgb(18, 24, 38)
        };

        var valueLabel = new Label
        {
            Text = "0",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Left = 10,
            Top = 10
        };

        var textLabel = new Label
        {
            Text = title,
            ForeColor = Color.FromArgb(160, 180, 210),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            AutoSize = true,
            Left = 10,
            Top = 52
        };

        card.Controls.Add(valueLabel);
        card.Controls.Add(textLabel);
        return card;
    }

    private void ConfigureButton(Button btn, string text, int left, int top, int width, int height, Color back, Color fore, Control parent)
    {
        btn.Text = text;
        btn.Left = left;
        btn.Top = top;
        btn.Width = width;
        btn.Height = height;
        btn.BackColor = back;
        btn.ForeColor = fore;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Color.FromArgb(56, 189, 248);
        btn.FlatAppearance.BorderSize = 1;
        btn.MouseEnter += (_, _) => { if (btn.Enabled) btn.BackColor = ControlPaint.Light(back); };
        btn.MouseLeave += (_, _) => { btn.BackColor = btn.Enabled ? back : Color.FromArgb(80, 86, 102); };
        parent.Controls.Add(btn);
    }

    private async Task InitializeAsync()
    {
        SetStatus("Carregando configura��es e manifest...");
        _lblAppVersion.Text = $"Vers�o do Atualizador: {Application.ProductVersion}";
        AppendLog("Inicializando Data Smart Deploy Center...");

        try
        {
            _manifest = await _manifestService.LoadAsync(_config.ManifestUrl);
            _lblManifestVersion.Text = $"Vers�o do Manifest: {(_manifest.Versao ?? "Desconhecida")}";
            AppendLog($"Manifest carregado: vers�o {_manifest.Versao}");
        }
        catch (Exception ex)
        {
            AppendLog($"Falha ao carregar manifest: {ex.Message}");
            MessageBox.Show($"Falha ao carregar manifest.json.\n{ex.Message}", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        await PopulateModulesAsync();
        await RunPreflightAsync();
        RefreshMetrics();
        SetStatus("Pronto para atualizar seu ambiente.");
    }

    private async Task RunPreflightAsync()
    {
        SetStatus("Validando ambiente...");
        AppendLog("Validando ambiente...");
        var validation = await _validatorService.ValidateAsync(_manifest);
        _environmentOk = validation.Sucesso;

        foreach (var message in validation.Mensagens)
            AppendLog(message);

        if (!_environmentOk)
        {
            SetStatus("Ambiente com problemas. Verifique o log.");
            _btnUpdate.Enabled = false;
            return;
        }

        SetStatus("Ambiente validado com sucesso.");
    }

    private async Task PopulateModulesAsync()
    {
        _gridModules.Rows.Clear();
        _modules.Clear();

        foreach (var item in _manifest.Arquivos)
        {
            var module = new ModuleInfo
            {
                Nome = item.Nome,
                Url = item.Url,
                Descricao = item.Descricao,
                Versao = item.Versao,
                TamanhoBytes = item.TamanhoBytes,
                Sha256 = item.Sha256,
                Data = item.Data
            };

            module.LocalPath = _backupService.FindLocalExePath(module.Nome) ?? string.Empty;
            module.LocalVersion = string.IsNullOrWhiteSpace(module.LocalPath)
                ? "N�o instalado"
                : VersionService.GetFileVersion(module.LocalPath);

            module.Estado = DetermineModuleState(module);
            _modules.Add(module);
            var rowIndex = _gridModules.Rows.Add(false, module.Nome, module.Descricao, module.LocalVersion, module.VersaoDisponivel, module.TamanhoText, module.DataText, module.StatusText);
            SetRowStatusStyle(_gridModules.Rows[rowIndex], module.Estado);
        }
    }

    private ModuleUpdateState DetermineModuleState(ModuleInfo module)
    {
        if (string.IsNullOrWhiteSpace(module.LocalPath))
            return ModuleUpdateState.NotInstalled;

        if (!string.IsNullOrWhiteSpace(module.Versao) && VersionService.IsUpdateAvailable(module.LocalVersion, module.Versao))
            return ModuleUpdateState.UpdateAvailable;

        return ModuleUpdateState.UpToDate;
    }

    private void SetRowStatusStyle(DataGridViewRow row, ModuleUpdateState estado)
    {
        row.DefaultCellStyle.BackColor = estado switch
        {
            ModuleUpdateState.UpdateAvailable => Color.FromArgb(44, 62, 80),
            ModuleUpdateState.NotInstalled => Color.FromArgb(71, 58, 130),
            ModuleUpdateState.UpToDate => Color.FromArgb(15, 75, 121),
            _ => Color.FromArgb(16, 22, 37)
        };
    }

    private void SetAllSelection(bool select)
    {
        foreach (DataGridViewRow row in _gridModules.Rows)
            row.Cells[0].Value = select;

        UpdateSelectionCount();
    }

    private void UpdateSelectionCount()
    {
        var selected = GetSelectedModules().Count;
        _btnUpdate.Enabled = selected > 0 && _environmentOk;
    }

    private List<ModuleInfo> GetSelectedModules()
    {
        var selected = new List<ModuleInfo>();

        for (var i = 0; i < _gridModules.Rows.Count; i++)
        {
            if (_gridModules.Rows[i].Cells[0].Value is bool value && value)
                selected.Add(_modules[i]);
        }

        return selected;
    }

    private void RefreshMetrics()
    {
        var installed = _modules.Count(m => m.Estado != ModuleUpdateState.NotInstalled);
        var updates = _modules.Count(m => m.Estado == ModuleUpdateState.UpdateAvailable);
        var history = _historyService.LoadHistory().LastOrDefault();
        var freeSpace = GetDriveFreeSpace(_dataDir);
        var bancoStatus = _environmentOk ? "Pronto" : "Aten��o";
        var connectionStatus = _environmentOk ? "Online" : "Offline";

        SetDashboardCard(0, installed.ToString(), "M�dulos Instalados");
        SetDashboardCard(1, updates.ToString(), "Atualiza��es Dispon�veis");
        SetDashboardCard(2, history != null ? history.DataHora.ToString("dd/MM/yyyy HH:mm") : "Nenhuma", "�ltima Implanta��o");
        SetDashboardCard(3, freeSpace, "Espa�o Livre");
        SetDashboardCard(4, bancoStatus, "Status do Banco");
        SetDashboardCard(5, connectionStatus, "Status da Conex�o");
    }

    private void SetDashboardCard(int index, string value, string label)
    {
        if (index < 0 || index >= _dashboardValue.Length) return;
        _dashboardValue[index].Text = value;
        _dashboardLabel[index].Text = label;
    }

    private static string GetDriveFreeSpace(string path)
    {
        try
        {
            var root = Path.GetPathRoot(path) ?? path;
            var drive = new DriveInfo(root);
            return $"{drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
        catch
        {
            return "Desconhecido";
        }
    }

    private async Task RunUpdateAsync()
    {
        var selected = GetSelectedModules();
        if (selected.Count == 0)
        {
            MessageBox.Show("Selecione pelo menos um m�dulo.", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!_environmentOk)
        {
            MessageBox.Show("O ambiente n�o est� pronto para a atualiza��o. Verifique o log.", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetControls(false);
        var session = _backupService.CreateSessionBackup();
        var started = DateTime.Now;
        var updatedFiles = new List<string>();
        var totalBytes = 0L;

        try
        {
            SetStatus("Criando backup do banco...");
            AppendLog("Criando backup do banco...");
            _backupService.BackupDatabase(session);

            for (var i = 0; i < selected.Count; i++)
            {
                var module = selected[i];
                SetStatus($"Baixando {module.Nome} ({i + 1}/{selected.Count})...");
                AppendLog($"Baixando {module.Nome}...");
                var destination = GetModuleDestinationPath(module.Nome);
                var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tmp");
                _backupService.BackupExe(module.Nome, session);
                await DownloadModuleAsync(module, tempFile, i, selected.Count);

                if (!ValidateDownloadedModule(tempFile, module))
                    throw new Exception($"Falha na valida��o de {module.Nome}.");

                ReplaceModuleFile(tempFile, destination);
                updatedFiles.Add(module.Nome);
                totalBytes += new FileInfo(destination).Length;
            }

            var doProcess = _rbLoadProcess.Checked || _rbLoadProcessClose.Checked;
            var closeAfter = _rbLoadProcessClose.Checked;
            SetStatus("Abrindo atualizador de banco...");
            AppendLog("Abrindo atualizador de banco...");
            var bancoResult = _bancoUpdaterService.OpenAndRun(_dataDir, doProcess, closeAfter);
            AppendLog(bancoResult);

            var elapsed = DateTime.Now - started;
            var speed = totalBytes > 0 ? $"{(totalBytes / 1024.0 / 1024.0) / elapsed.TotalSeconds:F2} MB/s" : "-";
            _historyService.Save(new UpdateHistoryEntry
            {
                DataHora = DateTime.Now,
                ArquivosAtualizados = updatedFiles,
                TempoTotal = elapsed.ToString(@"hh\:mm\:ss"),
                VelocidadeMedia = speed,
                BackupCriado = session,
                StatusBanco = bancoResult
            });

            AppendLog("Atualização concluída com sucesso.");
            SetStatus("Processo finalizado com sucesso.");
            var durationText = elapsed.ToString(@"hh\:mm\:ss");
            var completedMessage = $"Atualização concluída com sucesso.\n\nArquivos atualizados:\n- {string.Join("\n- ", updatedFiles)}\n\nTempo total: {durationText}\nVelocidade média: {speed}\nBackup: {session}\nLog: {_log.LogFile}";
            MessageBox.Show(completedMessage, _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog($"Erro: {ex.Message}");
            _log.Error(ex.Message);
            _backupService.RestoreSession(session);
            var rollbackMessage = $"Falha na atualização.\n\nErro: {ex.Message}\n\nRollback aplicado.\nBackup: {session}\nLog: {_log.LogFile}";
            MessageBox.Show(rollbackMessage, _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetControls(true);
            RefreshMetrics();
        }
    }

    private async Task DownloadModuleAsync(ModuleInfo module, string destination, int current, int total)
    {
        var downloadProgress = new Progress<int>(percent =>
        {
            var overall = (int)((current / (double)total) * 100 + (percent / (double)total));
            _progressTotal.Value = Math.Clamp(overall, 0, 100);
            _lblProgressPercent.Text = $"{overall}%";
            SetStatus($"Baixando {module.Nome}: {percent}% | Total {overall}%");
        });

        await _downloadService.DownloadAsync(module.Url, destination, downloadProgress);
    }

    private bool ValidateDownloadedModule(string path, ModuleInfo module)
    {
        if (!File.Exists(path)) return false;
        if (new FileInfo(path).Length < _config.MinimumFileSizeBytes) return false;
        if (!HashService.MatchesSha256(path, module.Sha256)) return false;
        return true;
    }

    private void ReplaceModuleFile(string sourcePath, string destinationPath)
    {
        if (File.Exists(destinationPath))
            File.Delete(destinationPath);

        File.Move(sourcePath, destinationPath, true);
    }

    private string GetModuleDestinationPath(string moduleName) => Path.Combine(_exeDir, moduleName);

    private void SetControls(bool enabled)
    {
        _btnUpdate.Enabled = enabled;
        _btnRefresh.Enabled = enabled;
        _btnClose.Enabled = enabled;
        _btnOpenLog.Enabled = enabled;
        _btnSelectAll.Enabled = enabled;
        _btnClearAll.Enabled = enabled;
        _gridModules.Enabled = enabled;
    }

    private void SetStatus(string message) => _lblStatus.Text = message;

    private void AppendLog(string message)
    {
        _log.Info(message);
        var text = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
        if (_logConsole.InvokeRequired)
            _logConsole.Invoke(() => _logConsole.AppendText(text));
        else
            _logConsole.AppendText(text);
    }

    private void OpenLogFile()
    {
        try
        {
            if (File.Exists(_log.LogFile))
                Process.Start("notepad.exe", _log.LogFile);
        }
        catch
        {
            MessageBox.Show("N�o foi poss�vel abrir o log.", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static Image? LoadImage(string baseDir, string fileName)
    {
        try
        {
            var path = Path.Combine(baseDir, fileName);
            return File.Exists(path) ? Image.FromFile(path) : null;
        }
        catch
        {
            return null;
        }
    }

    private static Image? LoadLogoImage(string baseDir)
    {
        return LoadImage(baseDir, "logo-datasmart_ext.png") ?? LoadImage(baseDir, "logo-datasmart.png");
    }
}

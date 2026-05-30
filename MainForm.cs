using DataSmartUpdater.Models;
using DataSmartUpdater.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
    private readonly SelfUpdateService _selfUpdateService;

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

        _log = new LogService(Path.Combine(_backupRoot, "Logs"));
        _manifestService = new ManifestService();
        _downloadService = new DownloadService(_log);
        _backupService = new BackupService(_dataDir, _log);
        _validatorService = new EnvironmentValidatorService(_config, _dataDir);
        _historyService = new HistoryService(_backupRoot);
        _bancoUpdaterService = new BancoUpdaterService(_log);
        _selfUpdateService = new SelfUpdateService(_config, _log);

        BuildUi();
        DoubleBuffered = true;
        Shown += async (_, _) => await InitializeAsync();
    }

    private void BuildUi()
    {
        Text = _config.AppName;
        Width = 980;
        Height = 820;
        MinimumSize = new Size(980, 720);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = SystemColors.Control;
        Font = new Font("Segoe UI", 9);
        // Estilo Windows Forms configurado em código C#; não há arquivo CSS para o formulário.
        // Improve text rendering
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        var primary = Color.FromArgb(0, 120, 215);
        var cardBackground = SystemColors.Window;
        var secondaryCard = SystemColors.ControlLight;
        var panelBackground = SystemColors.Control;
        var mutedText = SystemColors.GrayText;

        var imageDir = Path.Combine(AppContext.BaseDirectory, "Imagens");
        var logoImage = LoadLogoImage(imageDir);
        var heroImage = LoadImage(imageDir, "Marcos.png");

        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = BackColor,
            Padding = new Padding(16),
            Margin = new Padding(0)
        };
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150f));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 95f));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
        // Aumenta a altura do rodapé para evitar corte de botões em resoluções menores
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100f));
        Controls.Add(rootLayout);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = SystemColors.ControlLight,
            Padding = new Padding(18)
        };
        rootLayout.Controls.Add(headerPanel, 0, 0);

        var headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
        headerPanel.Controls.Add(headerLayout);

        var brandLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false,
            Margin = new Padding(0)
        };
        headerLayout.Controls.Add(brandLayout, 0, 0);

        if (logoImage is not null)
        {
            _logoPicture.Image = logoImage;
            _logoPicture.Size = new Size(72, 72);
            _logoPicture.SizeMode = PictureBoxSizeMode.Zoom;
            _logoPicture.Margin = new Padding(0, 0, 0, 10);
            brandLayout.Controls.Add(_logoPicture);
        }

        _lblAppTitle.Text = _config.AppName;
        _lblAppTitle.ForeColor = primary;
        _lblAppTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        _lblAppTitle.AutoSize = true;
        brandLayout.Controls.Add(_lblAppTitle);

        _lblSubtitle.Text = _config.AppSubtitle;
        _lblSubtitle.ForeColor = Color.White;
        _lblSubtitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        _lblSubtitle.AutoSize = true;
        brandLayout.Controls.Add(_lblSubtitle);

        _lblAppVersion.Text = "Versão: 1.0.0";
        _lblAppVersion.ForeColor = mutedText;
        _lblAppVersion.AutoSize = true;
        brandLayout.Controls.Add(_lblAppVersion);

        _lblManifestVersion.Text = "Manifest: desconhecido";
        _lblManifestVersion.ForeColor = mutedText;
        _lblManifestVersion.AutoSize = true;
        brandLayout.Controls.Add(_lblManifestVersion);

        _lblConnection.Text = "Verificando rede e manifesto...";
        _lblConnection.ForeColor = Color.FromArgb(122, 220, 191);
        _lblConnection.AutoSize = true;
        brandLayout.Controls.Add(_lblConnection);

        // separator
        var sep = new Panel { Height = 1, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(30, 40, 60), Margin = new Padding(0,8,0,0) };
        headerPanel.Controls.Add(sep);

        if (heroImage is not null)
        {
            _avatarPicture.Image = heroImage;
            _avatarPicture.Size = new Size(100, 126);
            _avatarPicture.SizeMode = PictureBoxSizeMode.Zoom;
            _avatarPicture.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerLayout.Controls.Add(_avatarPicture, 1, 0);
        }

        var dashboardPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };
        rootLayout.Controls.Add(dashboardPanel, 0, 1);

        var dashboardFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        dashboardPanel.Controls.Add(dashboardFlow);

        var cardLabels = new[]
        {
            "Modulos Instalados",
            "Atualizacoes Disponiveis",
            "Ultima Implantacao",
            "Espaco Livre",
            "Status do Banco",
            "Status da Conexao"
        };

        for (var i = 0; i < cardLabels.Length; i++)
        {
            var card = CreateMetricCard(cardLabels[i]);
            card.Margin = new Padding(0, 0, 12, 0);
            dashboardFlow.Controls.Add(card);
            _dashboardValue[i] = (Label)card.Controls[0];
            _dashboardLabel[i] = (Label)card.Controls[1];
        }

        var moduleCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = cardBackground,
            Padding = new Padding(16)
        };
        rootLayout.Controls.Add(moduleCard, 0, 2);

        var moduleLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            AutoSize = false,
            Margin = new Padding(0)
        };
        moduleLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        moduleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        moduleLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
        moduleCard.Controls.Add(moduleLayout);

        var moduleTitle = new Label
        {
            Text = "Lista de Modulos",
            ForeColor = primary,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top
        };
        moduleLayout.Controls.Add(moduleTitle, 0, 0);

        _gridModules.Dock = DockStyle.Fill;
        _gridModules.BackgroundColor = SystemColors.Window;
        _gridModules.BorderStyle = BorderStyle.FixedSingle;
        _gridModules.EnableHeadersVisualStyles = false;
        _gridModules.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
        _gridModules.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
        _gridModules.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _gridModules.ColumnHeadersHeight = 34;
        _gridModules.RowHeadersVisible = false;
        _gridModules.AllowUserToAddRows = false;
        _gridModules.AllowUserToDeleteRows = false;
        _gridModules.AllowUserToResizeRows = false;
        _gridModules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridModules.MultiSelect = false;
        _gridModules.ForeColor = SystemColors.ControlText;
        _gridModules.RowTemplate.Height = 34;
        _gridModules.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _gridModules.DefaultCellStyle.BackColor = SystemColors.Window;
        _gridModules.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
        _gridModules.DefaultCellStyle.ForeColor = SystemColors.ControlText;
        _gridModules.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        _gridModules.DefaultCellStyle.SelectionForeColor = Color.White;
        _gridModules.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _gridModules.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _gridModules.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        _gridModules.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.White;
        _gridModules.CellValueChanged += (_, _) => UpdateSelectionCount();
        _gridModules.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_gridModules.IsCurrentCellDirty)
                _gridModules.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };

        _gridModules.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Selecionar", HeaderText = "Selecionar", Width = 45, FillWeight = 20 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nome", HeaderText = "Nome", FillWeight = 120 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Descricao", HeaderText = "Descricao", FillWeight = 220 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "LocalSource", HeaderText = "Origem", FillWeight = 80 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "VersaoAtual", HeaderText = "Versao Local", FillWeight = 70 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "VersaoDisponivel", HeaderText = "Versao Online", FillWeight = 80 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tamanho", HeaderText = "Tamanho", FillWeight = 70 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Data", HeaderText = "Data", FillWeight = 70 });
        _gridModules.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", FillWeight = 90 });
        moduleLayout.Controls.Add(_gridModules, 0, 1);

        var moduleActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0)
        };
        moduleLayout.Controls.Add(moduleActions, 0, 2);

        ConfigureButton(_btnClearAll, "Limpar seleção", 0, 0, 120, 32, panelBackground, SystemColors.ControlText, moduleActions);
        ConfigureButton(_btnSelectAll, "Selecionar todos", 0, 0, 120, 32, panelBackground, SystemColors.ControlText, moduleActions);
        _btnClearAll.Margin = new Padding(0, 0, 8, 0);
        _btnSelectAll.Margin = new Padding(0, 0, 8, 0);
        _btnSelectAll.Click += (_, _) => SetAllSelection(true);
        _btnClearAll.Click += (_, _) => SetAllSelection(false);

        var bottomPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
        rootLayout.Controls.Add(bottomPanel, 0, 3);

        var databaseCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = cardBackground,
            Padding = new Padding(16)
        };
        bottomPanel.Controls.Add(databaseCard, 0, 0);

        var databaseLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Margin = new Padding(0)
        };
        databaseLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        databaseLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        databaseLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        databaseLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        databaseLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        databaseLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        databaseCard.Controls.Add(databaseLayout);

        var databaseTitle = new Label
        {
            Text = "Atualizador de Banco",
            ForeColor = primary,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top
        };
        databaseLayout.Controls.Add(databaseTitle, 0, 0);

        _rbNoDb.Text = "Nao executar";
        _rbNoDb.AutoSize = true;
        _rbNoDb.ForeColor = Color.White;
        _rbNoDb.Checked = true;
        databaseLayout.Controls.Add(_rbNoDb, 0, 1);

        _rbOnlyLoad.Text = "Apenas carregar arquivos";
        _rbOnlyLoad.AutoSize = true;
        _rbOnlyLoad.ForeColor = Color.White;
        databaseLayout.Controls.Add(_rbOnlyLoad, 0, 2);

        _rbLoadProcess.Text = "Carregar e processar arquivos";
        _rbLoadProcess.AutoSize = true;
        _rbLoadProcess.ForeColor = Color.White;
        databaseLayout.Controls.Add(_rbLoadProcess, 0, 3);

        _rbLoadProcessClose.Text = "Carregar, processar e fechar";
        _rbLoadProcessClose.AutoSize = true;
        _rbLoadProcessClose.ForeColor = Color.White;
        databaseLayout.Controls.Add(_rbLoadProcessClose, 0, 4);

        _lblStatus.Text = "Inicializando...";
        _lblStatus.ForeColor = Color.White;
        _lblStatus.AutoSize = true;
        _lblStatus.Dock = DockStyle.Top;
        databaseLayout.Controls.Add(_lblStatus, 0, 5);

        var logCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = cardBackground,
            Padding = new Padding(16)
        };
        bottomPanel.Controls.Add(logCard, 1, 0);

        var logTitle = new Label
        {
            Text = "Log em Tempo Real",
            ForeColor = primary,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top
        };
        logCard.Controls.Add(logTitle);

        _logConsole.Dock = DockStyle.Fill;
        _logConsole.BackColor = SystemColors.Window;
        _logConsole.ForeColor = SystemColors.ControlText;
        _logConsole.BorderStyle = BorderStyle.FixedSingle;
        _logConsole.ReadOnly = true;
        _logConsole.Font = new Font("Consolas", 9);
        logCard.Controls.Add(_logConsole);

        var footerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = secondaryCard,
            Padding = new Padding(16)
        };
        rootLayout.Controls.Add(footerPanel, 0, 4);

        // Layout do rodapé com 3 colunas: área principal (progresso), label de % e ações
        var footerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        footerPanel.Controls.Add(footerLayout);

        _progressTotal.Dock = DockStyle.Fill;
        _progressTotal.Height = 18;
        _progressTotal.Minimum = 0;
        _progressTotal.Maximum = 100;
        footerLayout.Controls.Add(_progressTotal, 0, 0);

        _lblProgressPercent.Text = "0%";
        _lblProgressPercent.ForeColor = primary;
        _lblProgressPercent.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _lblProgressPercent.AutoSize = true;
        _lblProgressPercent.TextAlign = ContentAlignment.MiddleRight; // alinha o texto dentro da célula do TableLayout
        footerLayout.Controls.Add(_lblProgressPercent, 1, 0);

        var footerActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        footerLayout.Controls.Add(footerActions, 2, 0);

        ConfigureButton(_btnRefresh, "Verificar", 0, 0, 100, 32, panelBackground, SystemColors.ControlText, footerActions);
        ConfigureButton(_btnOpenLog, "Abrir log", 0, 0, 100, 32, panelBackground, SystemColors.ControlText, footerActions);
        ConfigureButton(_btnClose, "Fechar", 0, 0, 100, 32, panelBackground, SystemColors.ControlText, footerActions);
        ConfigureButton(_btnUpdate, "Implantar", 0, 0, 140, 32, primary, Color.White, footerActions);

        _btnRefresh.Margin = new Padding(0, 0, 8, 0);
        _btnOpenLog.Margin = new Padding(0, 0, 8, 0);
        _btnClose.Margin = new Padding(0, 0, 8, 0);
        _btnUpdate.Margin = new Padding(0, 0, 0, 0);

        _btnUpdate.Click += async (_, _) => await RunUpdateAsync();
        _btnClose.Click += (_, _) => Close();
        _btnRefresh.Click += async (_, _) => await InitializeAsync();
        _btnOpenLog.Click += (_, _) => OpenLogFile();

        // Add hover effects for footer buttons
        foreach (Button b in new[] { _btnRefresh, _btnOpenLog, _btnClose, _btnUpdate })
        {
            var normal = b.BackColor;
            b.MouseEnter += (_, _) => b.BackColor = ControlPaint.Light(normal, 0.06f);
            b.MouseLeave += (_, _) => b.BackColor = normal;
        }
    }

    private Panel CreateCard(Rectangle bounds, Color backColor)
    {
        return new Panel
        {
            Left = bounds.Left,
            Top = bounds.Top,
            Width = bounds.Width,
            Height = bounds.Height,
            BackColor = backColor,
            Padding = new Padding(14)
        };
    }

    private Panel CreateMetricCard(string title)
    {
        var card = new Panel
        {
            Width = 160,
            Height = 110,
            BackColor = SystemColors.Window,
            Padding = new Padding(10)
        };

        // Apply rounded corners
        try { SetRoundedPanel(card, 10); } catch { }

        var valueLabel = new Label
        {
            Text = "0",
            ForeColor = SystemColors.ControlText,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Top,
            Height = 48
        };

        var textLabel = new Label
        {
            Text = title,
            ForeColor = SystemColors.GrayText,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Bottom,
            Height = 30
        };

        card.Controls.Add(valueLabel);
        card.Controls.Add(textLabel);
        return card;
    }

    private void SetRoundedPanel(Panel panel, int radius)
    {
        var rect = new Rectangle(0, 0, panel.Width, panel.Height);
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var arc = new Rectangle(rect.Location, new Size(radius, radius));
        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - radius;
        path.AddArc(arc, 270, 90);
        arc.Y = rect.Bottom - radius;
        path.AddArc(arc, 0, 90);
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        panel.Region = new Region(path);
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
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back);
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(back);
        btn.Cursor = Cursors.Hand;
        btn.Padding = new Padding(6, 4, 6, 4);
        parent.Controls.Add(btn);
        try
        {
            // Aplicar cantos arredondados para botões principais
            SetRoundedButton(btn, 6);
        }
        catch { }
    }

    private void SetRoundedButton(Button btn, int radius)
    {
        var rect = new Rectangle(0, 0, btn.Width, btn.Height);
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var arc = new Rectangle(rect.Location, new Size(radius, radius));
        // top-left
        path.AddArc(arc, 180, 90);
        // top-right
        arc.X = rect.Right - radius;
        path.AddArc(arc, 270, 90);
        // bottom-right
        arc.Y = rect.Bottom - radius;
        path.AddArc(arc, 0, 90);
        // bottom-left
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        btn.Region = new Region(path);
    }

    private async Task InitializeAsync()
    {
        SetStatus("Carregando configuracoes e manifesto...");
        _lblAppVersion.Text = $"Versao do Atualizador: {Application.ProductVersion}";
        AppendLog("Inicializando o DataSmart Deploy Center...");

        _backupService.MigrateRootExecutables();
        await CheckForSelfUpdateAsync();

        try
        {
            _manifest = await _manifestService.LoadAsync(_config.ManifestUrl);
            _lblManifestVersion.Text = $"Versao do Manifest: {(_manifest.Versao ?? "Desconhecida")}";
            AppendLog($"Manifesto carregado: versao {_manifest.Versao}");
        }
        catch (Exception ex)
        {
            AppendLog($"Falha ao carregar o manifesto: {ex.Message}");
            MessageBox.Show($"Falha ao carregar manifest.json.\n{ex.Message}", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        await PopulateModulesAsync();
        await RunPreflightAsync();
        if (_environmentOk)
            _lblConnection.Text = "Online - Pronto para atualizar seu ambiente.";
        RefreshMetrics();
        SetStatus("Pronto para atualizar seu ambiente.");
    }

    private async Task CheckForSelfUpdateAsync()
    {
        var update = await _selfUpdateService.CheckForUpdateAsync();
        if (update == null)
            return;

        var prompt = $"Uma nova versao do DataSmart Updater esta disponivel.\n\nNotas:\n{update.Notas}";
        var result = MessageBox.Show(prompt, _config.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result != DialogResult.Yes)
            return;

        if (await _selfUpdateService.PerformUpdateAsync(update))
        {
            AppendLog("Autoatualizacao iniciada. Fechando aplicacao...");
            MessageBox.Show("O atualizador sera reiniciado com a versao mais recente.", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }
        else
        {
            AppendLog("A autoatualizacao nao pode ser iniciada.");
        }
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
                ? "Nao instalado"
                : VersionService.GetFileVersion(module.LocalPath);
            module.Estado = DetermineModuleState(module);
            _modules.Add(module);

            var rowIndex = _gridModules.Rows.Add(false, module.Nome, module.Descricao, module.LocalSource, module.LocalVersion, module.VersaoDisponivel, module.TamanhoText, module.DataText, module.StatusText);
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
        var bancoStatus = _environmentOk ? "Pronto" : "Atencao";
        var connectionStatus = _environmentOk ? "Conectado" : "Desconectado";

        SetDashboardCard(0, installed.ToString(), "Modulos Instalados");
        SetDashboardCard(1, updates.ToString(), "Atualizacoes Disponiveis");
        SetDashboardCard(2, history != null ? history.DataHora.ToString("dd/MM/yyyy HH:mm") : "Nenhum", "Ultima Implantacao");
        SetDashboardCard(3, freeSpace, "Espaco Livre");
        SetDashboardCard(4, bancoStatus, "Status do Banco");
        SetDashboardCard(5, connectionStatus, "Status da Conexao");
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
            return "Unknown";
        }
    }

    private async Task RunUpdateAsync()
    {
        var selected = GetSelectedModules();
        if (selected.Count == 0)
        {
            MessageBox.Show("Selecione pelo menos um modulo.", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!_environmentOk)
        {
            MessageBox.Show("O ambiente nao esta pronto para atualizacao. Verifique o log.", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetControls(false);
        var session = _backupService.CreateSessionBackup();
        var started = DateTime.Now;
        var updatedFiles = new List<string>();
        var totalBytes = 0L;

        try
        {
            SetStatus("Criando backup do banco de dados...");
            AppendLog("Criando backup do arquivo COMERCIAL.DAT...");
            _backupService.BackupDatabase(session);

            for (var i = 0; i < selected.Count; i++)
            {
                var module = selected[i];
                SetStatus($"Downloading {module.Nome} ({i + 1}/{selected.Count})...");
                AppendLog($"Downloading {module.Nome}...");
                var destination = GetModuleDestinationPath(module.Nome);
                var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tmp");
                _backupService.BackupExe(module.Nome, session);
                await DownloadModuleAsync(module, tempFile, i, selected.Count);

                if (!ValidateDownloadedModule(tempFile, module))
                throw new Exception($"Validacao falhou para {module.Nome}.");

                ReplaceModuleFile(tempFile, destination);
                updatedFiles.Add(module.Nome);
                totalBytes += new FileInfo(destination).Length;
            }

            var doProcess = _rbLoadProcess.Checked || _rbLoadProcessClose.Checked;
            var closeAfter = _rbLoadProcessClose.Checked;
            SetStatus("Abrindo o atualizador de banco...");
            AppendLog("Abrindo o atualizador de banco...");
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

            AppendLog("Atualizacao concluida com sucesso.");
            SetStatus("Processo finalizado com sucesso.");
            var durationText = elapsed.ToString(@"hh\:mm\:ss");
            var completedMessage = $"Atualizacao concluida com sucesso.\n\nArquivos atualizados:\n- {string.Join("\n- ", updatedFiles)}\n\nTempo total: {durationText}\nVelocidade media: {speed}\nBackup: {session}\nLog: {_log.LogFile}";
            MessageBox.Show(completedMessage, _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog($"Erro: {ex.Message}");
            _log.Error(ex.Message);
            _backupService.RestoreSession(session);
            var rollbackMessage = $"Atualizacao falhou.\n\nErro: {ex.Message}\n\nRollback aplicado.\nBackup: {session}\nLog: {_log.LogFile}";
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
            SetStatus($"Downloading {module.Nome}: {percent}% | Total {overall}%");
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
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? _exeDir);

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
            if (!File.Exists(_log.LogFile))
            {
                MessageBox.Show("Arquivo de log nao encontrado.", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("notepad.exe", _log.LogFile) { UseShellExecute = true });
                return;
            }

            if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo("xdg-open", _log.LogFile) { UseShellExecute = true });
                return;
            }

            if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo("open", _log.LogFile) { UseShellExecute = true });
                return;
            }

            // fallback
            Process.Start(new ProcessStartInfo { FileName = _log.LogFile, UseShellExecute = true });
        }
        catch
        {
            MessageBox.Show("Nao foi possivel abrir o log.", _config.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

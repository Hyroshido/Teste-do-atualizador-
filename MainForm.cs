using DataSmartUpdater.Models;
using DataSmartUpdater.Services;

namespace DataSmartUpdater;

public sealed class MainForm : Form
{
    private readonly AppConfig _config;
    private readonly string _dataDir;
    private readonly string _exeDir;
    private readonly string _backupDir;
    private readonly LogService _log;
    private readonly ManifestService _manifestService;
    private readonly DownloadService _downloadService;
    private readonly BackupService _backupService;
    private readonly BancoUpdaterService _bancoUpdaterService;

    private readonly CheckedListBox _list = new();
    private readonly Label _lblStatus = new();
    private readonly Label _lblPercent = new();
    private readonly Label _lblSelected = new();
    private readonly ProgressBar _progress = new();
    private readonly Button _btnUpdate = new();
    private readonly Button _btnClose = new();
    private readonly Button _btnAll = new();
    private readonly Button _btnNone = new();
    private readonly Button _btnLog = new();
    private readonly RadioButton _rbSomenteCarregar = new();
    private readonly RadioButton _rbCarregarProcessar = new();
    private readonly PictureBox _logoPicture = new();
    private readonly PictureBox _heroPicture = new();

    private ManifestFile _manifest = new();

    public MainForm(AppConfig config)
    {
        _config = config;
        _dataDir = PathService.GetDataSmartPath();
        _exeDir = Path.Combine(_dataDir, "EXE");
        _backupDir = Path.Combine(_dataDir, "Backup");

        Directory.CreateDirectory(_exeDir);
        Directory.CreateDirectory(_backupDir);

        _log = new LogService(_backupDir);
        _manifestService = new ManifestService();
        _downloadService = new DownloadService(_log);
        _backupService = new BackupService(_dataDir, _log);
        _bancoUpdaterService = new BancoUpdaterService(_log);

        BuildUi();

        Shown += async (_, _) => await LoadManifestAsync();
    }

    private void BuildUi()
    {
        Text = "Data Smart Enterprise - Atualizador";
        Width = 960;
        Height = 820;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
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
            Width = 920,
            Height = 170,
            BackColor = Color.FromArgb(20, 28, 50)
        };
        Controls.Add(headerPanel);

        if (logoImage is not null)
        {
            _logoPicture.Image = logoImage;
            _logoPicture.Left = 20;
            _logoPicture.Top = 20;
            _logoPicture.Width = 72;
            _logoPicture.Height = 72;
            _logoPicture.SizeMode = PictureBoxSizeMode.Zoom;
            headerPanel.Controls.Add(_logoPicture);
        }

        var headerLeft = logoImage is not null ? 110 : 20;
        var headerTitle = new Label
        {
            Text = "DATA SMART ENTERPRISE",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            Left = headerLeft,
            Top = 24
        };
        headerPanel.Controls.Add(headerTitle);

        var headerSubtitle = new Label
        {
            Text = "Atualizador seguro com backup automático e deploy de módulos",
            ForeColor = textGray,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            AutoSize = true,
            Left = headerLeft,
            Top = 68
        };
        headerPanel.Controls.Add(headerSubtitle);

        var headerDetail = new Label
        {
            Text = $"Diretório detectado: {_dataDir}",
            ForeColor = Color.FromArgb(160, 170, 190),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = true,
            Left = headerLeft,
            Top = 96
        };
        headerPanel.Controls.Add(headerDetail);

        if (heroImage is not null)
        {
            _heroPicture.Image = heroImage;
            _heroPicture.Left = 760;
            _heroPicture.Top = 18;
            _heroPicture.Width = 140;
            _heroPicture.Height = 140;
            _heroPicture.SizeMode = PictureBoxSizeMode.Zoom;
            headerPanel.Controls.Add(_heroPicture);
        }

        var infoCard = new Panel
        {
            Left = 20,
            Top = 202,
            Width = 920,
            Height = 80,
            BackColor = cardBackground,
            Padding = new Padding(20)
        };
        Controls.Add(infoCard);

        infoCard.Controls.Add(new Label
        {
            Text = "Processo: 1) criar backup de COMERCIAL.DAT, 2) backup dos executáveis, 3) download dos módulos selecionados, 4) substituir arquivos e 5) abrir o atualizador de banco.",
            ForeColor = textGray,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = false,
            Width = 860,
            Height = 40,
            Left = 0,
            Top = 12
        });

        var listCard = new Panel
        {
            Left = 20,
            Top = 298,
            Width = 920,
            Height = 340,
            BackColor = cardBackground,
            Padding = new Padding(20)
        };
        Controls.Add(listCard);

        listCard.Controls.Add(new Label
        {
            Text = "Módulos disponíveis",
            ForeColor = blue,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Left = 0,
            Top = 0
        });

        _list.Left = 0;
        _list.Top = 34;
        _list.Width = 880;
        _list.Height = 220;
        _list.BackColor = panelBackground;
        _list.ForeColor = Color.White;
        _list.CheckOnClick = true;
        _list.BorderStyle = BorderStyle.None;
        _list.ItemCheck += (_, _) => BeginInvoke(new Action(UpdateSelectedCount));
        listCard.Controls.Add(_list);

        _lblSelected.Text = "Nenhum selecionado";
        _lblSelected.ForeColor = textGray;
        _lblSelected.Left = 0;
        _lblSelected.Top = 265;
        _lblSelected.Width = 300;
        listCard.Controls.Add(_lblSelected);

        ConfigureButton(_btnAll, "Marcar todos", 600, 260, 125, 34, panelBackground, Color.White, listCard);
        ConfigureButton(_btnNone, "Desmarcar todos", 735, 260, 125, 34, panelBackground, Color.White, listCard);

        var bottomCard = new Panel
        {
            Left = 20,
            Top = 650,
            Width = 920,
            Height = 140,
            BackColor = cardBackground,
            Padding = new Padding(20)
        };
        Controls.Add(bottomCard);

        bottomCard.Controls.Add(new Label
        {
            Text = "Atualizador de Banco",
            ForeColor = blue,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Left = 0,
            Top = 0
        });

        _rbSomenteCarregar.Text = "Somente carregar arquivos";
        _rbSomenteCarregar.Left = 0;
        _rbSomenteCarregar.Top = 30;
        _rbSomenteCarregar.Width = 260;
        _rbSomenteCarregar.ForeColor = Color.White;
        _rbSomenteCarregar.Checked = true;
        bottomCard.Controls.Add(_rbSomenteCarregar);

        _rbCarregarProcessar.Text = "Carregar e processar arquivos";
        _rbCarregarProcessar.Left = 300;
        _rbCarregarProcessar.Top = 30;
        _rbCarregarProcessar.Width = 260;
        _rbCarregarProcessar.ForeColor = Color.White;
        bottomCard.Controls.Add(_rbCarregarProcessar);

        _lblStatus.Text = "Carregando lista de módulos...";
        _lblStatus.ForeColor = Color.White;
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        _lblStatus.Left = 0;
        _lblStatus.Top = 70;
        _lblStatus.Width = 580;
        _lblStatus.Height = 24;
        bottomCard.Controls.Add(_lblStatus);

        _progress.Left = 0;
        _progress.Top = 100;
        _progress.Width = 700;
        _progress.Height = 18;
        _progress.Minimum = 0;
        _progress.Maximum = 100;
        bottomCard.Controls.Add(_progress);

        _lblPercent.Text = "0%";
        _lblPercent.ForeColor = blue;
        _lblPercent.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _lblPercent.Left = 710;
        _lblPercent.Top = 98;
        _lblPercent.Width = 70;
        bottomCard.Controls.Add(_lblPercent);

        ConfigureButton(_btnLog, "Ver Log", 600, 70, 100, 32, panelBackground, Color.White, bottomCard);
        ConfigureButton(_btnClose, "Fechar", 710, 70, 100, 32, panelBackground, Color.White, bottomCard);
        ConfigureButton(_btnUpdate, "IMPLANTAR AGORA", 600, 102, 220, 32, blue, Color.FromArgb(15, 23, 42), bottomCard);

        _btnUpdate.Enabled = false;

        _btnAll.Click += (_, _) =>
        {
            for (int i = 0; i < _list.Items.Count; i++) _list.SetItemChecked(i, true);
            UpdateSelectedCount();
        };

        _btnNone.Click += (_, _) =>
        {
            for (int i = 0; i < _list.Items.Count; i++) _list.SetItemChecked(i, false);
            UpdateSelectedCount();
        };

        _btnClose.Click += (_, _) => Close();

        _btnLog.Click += (_, _) =>
        {
            if (File.Exists(_log.LogFile))
                System.Diagnostics.Process.Start("notepad.exe", _log.LogFile);
            else
                MessageBox.Show("Nenhum log foi gerado ainda.", "Data Smart Enterprise");
        };

        _btnUpdate.Click += async (_, _) => await RunUpdateAsync();
    }

    private void ConfigureButton(Button btn, string text, int left, int top, int width, int height, Color back, Color fore, Control? parent = null)
    {
        var target = parent?.Controls ?? Controls;
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
        var originalBack = back;

        btn.MouseEnter += (_, _) =>
        {
            if (btn.Enabled)
                btn.BackColor = ControlPaint.Light(originalBack);
        };

        btn.MouseLeave += (_, _) =>
        {
            btn.BackColor = btn.Enabled ? originalBack : Color.FromArgb(80, 86, 102);
        };

        btn.EnabledChanged += (_, _) =>
        {
            btn.BackColor = btn.Enabled ? originalBack : Color.FromArgb(80, 86, 102);
        };

        target.Add(btn);
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
        return LoadImage(baseDir, "logo-datasmart_ext.png")
            ?? LoadImage(baseDir, "logo-datasmart.png");
    }

    private async Task LoadManifestAsync()
    {
        try
        {
            _manifest = await _manifestService.LoadAsync(_config.ManifestUrl);
            _list.Items.Clear();

            foreach (var item in _manifest.Arquivos)
            {
                var local = FindLocalModulePath(item.Nome);
                var status = local != null ? $"ENCONTRADO em {(Path.GetDirectoryName(local)?.EndsWith("EXE", StringComparison.OrdinalIgnoreCase) == true ? "EXE" : "raiz")} - será substituído com backup" : "NÃO INSTALADO - nova instalação";
                _list.Items.Add($"{item.Nome}  |  {item.Descricao}  |  {status}", false);
            }

            _lblStatus.Text = "Selecione os módulos que deseja atualizar.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Falha ao carregar manifest.json.\n\n{ex.Message}", "Data Smart Enterprise", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateSelectedCount()
    {
        var count = _list.CheckedItems.Count;
        _lblSelected.Text = count == 0 ? "Nenhum selecionado" : count == 1 ? "1 selecionado" : $"{count} selecionados";
        _btnUpdate.Enabled = count > 0;
    }

    private List<ManifestItem> GetSelectedItems()
    {
        var selected = new List<ManifestItem>();

        for (int i = 0; i < _list.Items.Count; i++)
        {
            if (_list.GetItemChecked(i))
                selected.Add(_manifest.Arquivos[i]);
        }

        return selected;
    }

    private async Task RunUpdateAsync()
    {
        var selected = GetSelectedItems();

        if (selected.Count == 0)
        {
            MessageBox.Show("Selecione pelo menos um módulo.", "Data Smart Enterprise");
            return;
        }

        foreach (var item in selected)
        {
            var proc = Path.GetFileNameWithoutExtension(item.Nome);
            if (System.Diagnostics.Process.GetProcessesByName(proc).Length > 0)
            {
                MessageBox.Show($"Feche o módulo {item.Nome} antes de continuar.", "Data Smart Enterprise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        SetControls(false);

        var session = _backupService.CreateSessionBackup();
        var date = DateTime.Now.ToString("yyyyMMdd");

        try
        {
            SetProgress(5, "Preparando ambiente...");
            _backupService.BackupDatabase(session);

            SetProgress(15, "Criando backup do banco...");

            int total = selected.Count;
            int current = 0;

            foreach (var item in selected)
            {
                current++;

                SetProgress((int)(((current - 1) / (double)total) * 100), $"Preparando ambiente para {item.Nome} ({current}/{total})...");

                var finalPath = GetModuleDestinationPath(item.Nome);
                var tempPath = finalPath + ".tmp";

                SetProgress((int)(((current - 1) / (double)total) * 100) + 5, "Criando backup dos módulos...");
                _backupService.BackupExe(item.Nome, session);

                var downloadProgress = new Progress<int>(p =>
                {
                    var totalPercent = (int)((((current - 1) / (double)total) * 100) + (p / (double)total));
                    SetProgress(totalPercent, $"Baixando arquivo {item.Nome}... {p}% | Total {totalPercent}%");
                });

                await _downloadService.DownloadAsync(item.Url, tempPath, downloadProgress);

                if (File.Exists(finalPath))
                {
                    SetProgress((int)(((current - 1) / (double)total) * 100) + 80, $"Substituindo arquivo {item.Nome}...");
                    File.Delete(finalPath);
                }

                File.Move(tempPath, finalPath, true);

                _log.Info($"Arquivo atualizado: {finalPath}");
            }

            SetProgress(95, "Abrindo atualizador de banco...");
            var processar = _rbCarregarProcessar.Checked;
            var bancoResult = _bancoUpdaterService.OpenAndRun(_dataDir, processar);

            SetProgress(100, "Processo finalizado com sucesso.");

            MessageBox.Show(
                $"Atualização concluída com sucesso.\n\nBackup:\n{session}\n\nAtualizador de banco:\n{bancoResult}\n\nLog:\n{_log.LogFile}",
                "Data Smart Enterprise",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Close();
        }
        catch (Exception ex)
        {
            _log.Error(ex.Message);
            MessageBox.Show($"Falha na atualização.\n\nErro:\n{ex.Message}\n\nBackup:\n{session}\n\nLog:\n{_log.LogFile}", "Data Smart Enterprise", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetControls(true);
        }
    }

    private string GetModuleDestinationPath(string moduleName)
    {
        return Path.Combine(_exeDir, moduleName);
    }

    private string? FindLocalModulePath(string moduleName)
    {
        var exePath = Path.Combine(_exeDir, moduleName);
        if (File.Exists(exePath))
            return exePath;

        var rootPath = Path.Combine(_dataDir, moduleName);
        if (File.Exists(rootPath))
            return rootPath;

        return null;
    }

    private void SetControls(bool enabled)
    {
        _btnUpdate.Enabled = enabled;
        _btnClose.Enabled = enabled;
        _btnAll.Enabled = enabled;
        _btnNone.Enabled = enabled;
        _list.Enabled = enabled;
    }

    private void SetProgress(int value, string message)
    {
        value = Math.Clamp(value, 0, 100);

        _progress.Value = value;
        _lblPercent.Text = $"{value}%";
        _lblStatus.Text = message;
        Application.DoEvents();
    }
}

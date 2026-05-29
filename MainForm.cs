using DataSmartUpdater.Models;
using DataSmartUpdater.Services;

namespace DataSmartUpdater;

public sealed class MainForm : Form
{
    private readonly AppConfig _config;
    private readonly string _dataDir;
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

    private ManifestFile _manifest = new();

    public MainForm(AppConfig config)
    {
        _config = config;
        _dataDir = PathService.GetDataSmartPath();
        _backupDir = Path.Combine(_dataDir, "Backup");
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
        Width = 940;
        Height = 820;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(15, 23, 42);
        Font = new Font("Segoe UI", 9);

        var blue = Color.FromArgb(56, 189, 248);
        var panel = Color.FromArgb(30, 41, 59);
        var gray = Color.FromArgb(148, 163, 184);

        var title = new Label
        {
            Text = "DATA SMART ENTERPRISE",
            ForeColor = blue,
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            AutoSize = true,
            Left = 30,
            Top = 25
        };
        Controls.Add(title);

        var subtitle = new Label
        {
            Text = "Atualizador seguro com backup automático, download online e carregamento do atualizador de banco",
            ForeColor = Color.White,
            AutoSize = true,
            Left = 32,
            Top = 72
        };
        Controls.Add(subtitle);

        var dir = new Label
        {
            Text = $"Diretório detectado: {_dataDir}",
            ForeColor = gray,
            AutoSize = true,
            Left = 32,
            Top = 100
        };
        Controls.Add(dir);

        var aviso = new Panel
        {
            Left = 30,
            Top = 130,
            Width = 850,
            Height = 82,
            BackColor = Color.FromArgb(23, 32, 51)
        };
        Controls.Add(aviso);

        aviso.Controls.Add(new Label
        {
            Text = "Processo: 1) cria backup do COMERCIAL.DAT, 2) cria backup dos executáveis selecionados, 3) baixa os arquivos novos, 4) substitui os módulos, 5) abre o Atualizador de Banco e clica apenas em Carregar arquivos.",
            ForeColor = Color.White,
            Left = 15,
            Top = 15,
            Width = 815,
            Height = 55
        });

        _list.Left = 30;
        _list.Top = 230;
        _list.Width = 850;
        _list.Height = 310;
        _list.BackColor = panel;
        _list.ForeColor = Color.White;
        _list.CheckOnClick = true;
        _list.BorderStyle = BorderStyle.FixedSingle;
        _list.ItemCheck += (_, _) => BeginInvoke(new Action(UpdateSelectedCount));
        Controls.Add(_list);

        _lblSelected.Text = "Nenhum selecionado";
        _lblSelected.ForeColor = gray;
        _lblSelected.Left = 30;
        _lblSelected.Top = 555;
        _lblSelected.Width = 250;
        Controls.Add(_lblSelected);

        _rbSomenteCarregar.Text = "Somente carregar arquivos";
        _rbSomenteCarregar.Left = 30;
        _rbSomenteCarregar.Top = 590;
        _rbSomenteCarregar.Width = 260;
        _rbSomenteCarregar.ForeColor = Color.White;
        _rbSomenteCarregar.Checked = true;
        Controls.Add(_rbSomenteCarregar);

        _rbCarregarProcessar.Text = "Carregar e processar arquivos";
        _rbCarregarProcessar.Left = 310;
        _rbCarregarProcessar.Top = 590;
        _rbCarregarProcessar.Width = 260;
        _rbCarregarProcessar.ForeColor = Color.White;
        Controls.Add(_rbCarregarProcessar);

        ConfigureButton(_btnAll, "Marcar todos", 610, 550, 120, 34, panel, Color.White);
        ConfigureButton(_btnNone, "Desmarcar todos", 745, 550, 135, 34, panel, Color.White);

        _lblStatus.Text = "Carregando lista de módulos...";
        _lblStatus.ForeColor = Color.White;
        _lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        _lblStatus.Left = 30;
        _lblStatus.Top = 635;
        _lblStatus.Width = 850;
        _lblStatus.Height = 28;
        Controls.Add(_lblStatus);

        _progress.Left = 30;
        _progress.Top = 665;
        _progress.Width = 740;
        _progress.Height = 22;
        _progress.Minimum = 0;
        _progress.Maximum = 100;
        Controls.Add(_progress);

        _lblPercent.Text = "0%";
        _lblPercent.ForeColor = blue;
        _lblPercent.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _lblPercent.Left = 790;
        _lblPercent.Top = 663;
        _lblPercent.Width = 90;
        Controls.Add(_lblPercent);

        ConfigureButton(_btnLog, "Ver Log", 525, 700, 100, 38, panel, Color.White);
        ConfigureButton(_btnClose, "Fechar", 640, 700, 100, 38, panel, Color.White);
        ConfigureButton(_btnUpdate, "IMPLANTAR AGORA", 755, 700, 125, 38, blue, Color.FromArgb(15, 23, 42));

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

    private void ConfigureButton(Button btn, string text, int left, int top, int width, int height, Color back, Color fore)
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
        Controls.Add(btn);
    }

    private async Task LoadManifestAsync()
    {
        try
        {
            _manifest = await _manifestService.LoadAsync(_config.ManifestUrl);
            _list.Items.Clear();

            foreach (var item in _manifest.Arquivos)
            {
                var local = Path.Combine(_dataDir, item.Nome);
                var status = File.Exists(local) ? "ENCONTRADO - será substituído com backup" : "NÃO INSTALADO - nova instalação";
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
            _backupService.BackupDatabase(session);

            int total = selected.Count;
            int current = 0;

            foreach (var item in selected)
            {
                current++;

                SetProgress((int)(((current - 1) / (double)total) * 100), $"Preparando {item.Nome} ({current} de {total})...");

                var finalPath = Path.Combine(_dataDir, item.Nome);
                var tempPath = finalPath + ".tmp";

                _backupService.BackupExe(item.Nome, session);

                var downloadProgress = new Progress<int>(p =>
                {
                    var totalPercent = (int)((((current - 1) / (double)total) * 100) + (p / (double)total));
                    SetProgress(totalPercent, $"Baixando {item.Nome}... {p}% | Total {totalPercent}%");
                });

                await _downloadService.DownloadAsync(item.Url, tempPath, downloadProgress);

                if (File.Exists(finalPath))
                    File.Delete(finalPath);

                File.Move(tempPath, finalPath, true);

                _log.Info($"Arquivo atualizado: {finalPath}");
            }

            var processar = _rbCarregarProcessar.Checked;
            SetProgress(98, processar ? "Abrindo atualizador de banco, carregando e processando arquivos..." : "Abrindo atualizador de banco e clicando em Carregar arquivos...");
            var bancoResult = _bancoUpdaterService.OpenAndRun(_dataDir, processar);

            SetProgress(100, "Atualização concluída com sucesso.");

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

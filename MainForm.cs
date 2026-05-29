using DataSmartUpdater.Models;
using DataSmartUpdater.Services;

namespace DataSmartUpdater;

public sealed class MainForm : Form
{
    private readonly AppConfig _config;
    private readonly string _installDirectory;
    private readonly string _backupDirectory;
    private readonly LogService _log;
    private readonly ManifestService _manifestService;

    private readonly Label _titleLabel = new();
    private readonly Label _subtitleLabel = new();
    private readonly Label _pathLabel = new();
    private readonly Label _infoLabel = new();
    private readonly CheckedListBox _moduleList = new();
    private readonly Label _selectedCountLabel = new();
    private readonly Label _statusLabel = new();
    private readonly ProgressBar _progressBar = new();
    private readonly Label _percentLabel = new();
    private readonly Button _selectAllButton = new();
    private readonly Button _unselectAllButton = new();
    private readonly Button _logButton = new();
    private readonly Button _closeButton = new();
    private readonly Button _updateButton = new();

    private readonly List<ManifestFile> _files = [];
    private string _currentLogPath = string.Empty;

    public MainForm(AppConfig config)
    {
        _config = config;
        _installDirectory = InstallPathService.ResolveInstallPath(config.DefaultInstallPath);
        _backupDirectory = Path.Combine(_installDirectory, "Backup");
        Directory.CreateDirectory(_backupDirectory);

        _log = new LogService(_backupDirectory);
        _currentLogPath = _log.LogFilePath;
        _manifestService = new ManifestService(config.DownloadTimeoutSeconds);

        ConfigureForm();
        BuildInterface();

        Shown += async (_, _) => await LoadManifestAsync();
    }

    private void ConfigureForm()
    {
        Text = "Data Smart Enterprise - Atualizador";
        Width = 960;
        Height = 760;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(15, 23, 42);
        Font = new Font("Segoe UI", 9);
    }

    private void BuildInterface()
    {
        var blue = Color.FromArgb(56, 189, 248);
        var panel = Color.FromArgb(30, 41, 59);
        var gray = Color.FromArgb(148, 163, 184);

        _titleLabel.Text = "DATA SMART ENTERPRISE";
        _titleLabel.ForeColor = blue;
        _titleLabel.Font = new Font("Segoe UI", 22, FontStyle.Bold);
        _titleLabel.AutoSize = true;
        _titleLabel.Left = 30;
        _titleLabel.Top = 25;
        Controls.Add(_titleLabel);

        _subtitleLabel.Text = "Atualizador seguro com backup automático, restauração e carregamento do atualizador de banco";
        _subtitleLabel.ForeColor = Color.White;
        _subtitleLabel.AutoSize = true;
        _subtitleLabel.Left = 32;
        _subtitleLabel.Top = 72;
        Controls.Add(_subtitleLabel);

        _pathLabel.Text = $"Diretório detectado: {_installDirectory}";
        _pathLabel.ForeColor = gray;
        _pathLabel.AutoSize = true;
        _pathLabel.Left = 32;
        _pathLabel.Top = 98;
        Controls.Add(_pathLabel);

        var warningPanel = new Panel
        {
            Left = 30,
            Top = 130,
            Width = 880,
            Height = 82,
            BackColor = Color.FromArgb(23, 32, 51)
        };
        Controls.Add(warningPanel);

        _infoLabel.Text =
            "Processo: 1) baixa a lista online do GitHub, 2) cria backup do COMERCIAL.DAT, " +
            "3) cria backup dos executáveis selecionados, 4) baixa os arquivos novos, " +
            "5) substitui os módulos, 6) abre o Atualizador de Banco e clica apenas em Carregar arquivos.";
        _infoLabel.ForeColor = Color.White;
        _infoLabel.Left = 15;
        _infoLabel.Top = 14;
        _infoLabel.Width = 850;
        _infoLabel.Height = 60;
        warningPanel.Controls.Add(_infoLabel);

        _moduleList.Left = 30;
        _moduleList.Top = 230;
        _moduleList.Width = 880;
        _moduleList.Height = 310;
        _moduleList.BackColor = panel;
        _moduleList.ForeColor = Color.White;
        _moduleList.CheckOnClick = true;
        _moduleList.BorderStyle = BorderStyle.FixedSingle;
        _moduleList.Font = new Font("Segoe UI", 9);
        _moduleList.ItemCheck += (_, _) => BeginInvoke(UpdateSelectedCount);
        Controls.Add(_moduleList);

        _selectedCountLabel.Text = "Nenhum selecionado";
        _selectedCountLabel.ForeColor = gray;
        _selectedCountLabel.Left = 30;
        _selectedCountLabel.Top = 552;
        _selectedCountLabel.Width = 260;
        Controls.Add(_selectedCountLabel);

        ConfigureSecondaryButton(_selectAllButton, "Marcar todos", 650, 546, 125);
        _selectAllButton.Click += (_, _) =>
        {
            for (var i = 0; i < _moduleList.Items.Count; i++)
            {
                _moduleList.SetItemChecked(i, true);
            }

            UpdateSelectedCount();
        };
        Controls.Add(_selectAllButton);

        ConfigureSecondaryButton(_unselectAllButton, "Desmarcar todos", 785, 546, 125);
        _unselectAllButton.Click += (_, _) =>
        {
            for (var i = 0; i < _moduleList.Items.Count; i++)
            {
                _moduleList.SetItemChecked(i, false);
            }

            UpdateSelectedCount();
        };
        Controls.Add(_unselectAllButton);

        _statusLabel.Text = "Carregando lista online de módulos...";
        _statusLabel.ForeColor = Color.White;
        _statusLabel.TextAlign = ContentAlignment.MiddleCenter;
        _statusLabel.Left = 30;
        _statusLabel.Top = 592;
        _statusLabel.Width = 880;
        _statusLabel.Height = 28;
        Controls.Add(_statusLabel);

        _progressBar.Left = 30;
        _progressBar.Top = 628;
        _progressBar.Width = 760;
        _progressBar.Height = 22;
        _progressBar.Minimum = 0;
        _progressBar.Maximum = 100;
        _progressBar.Value = 0;
        Controls.Add(_progressBar);

        _percentLabel.Text = "0%";
        _percentLabel.ForeColor = blue;
        _percentLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _percentLabel.Left = 810;
        _percentLabel.Top = 626;
        _percentLabel.Width = 100;
        _percentLabel.Height = 25;
        Controls.Add(_percentLabel);

        ConfigureSecondaryButton(_logButton, "Ver Log", 555, 672, 105);
        _logButton.Click += (_, _) =>
        {
            if (File.Exists(_currentLogPath))
            {
                System.Diagnostics.Process.Start("notepad.exe", _currentLogPath);
            }
            else
            {
                MessageBox.Show("Nenhum log gerado ainda.", "Data Smart Enterprise");
            }
        };
        Controls.Add(_logButton);

        ConfigureSecondaryButton(_closeButton, "Fechar", 675, 672, 105);
        _closeButton.Click += (_, _) => Close();
        Controls.Add(_closeButton);

        _updateButton.Text = "IMPLANTAR AGORA";
        _updateButton.Left = 795;
        _updateButton.Top = 672;
        _updateButton.Width = 115;
        _updateButton.Height = 38;
        _updateButton.Enabled = false;
        _updateButton.BackColor = blue;
        _updateButton.ForeColor = Color.FromArgb(15, 23, 42);
        _updateButton.FlatStyle = FlatStyle.Flat;
        _updateButton.FlatAppearance.BorderColor = blue;
        _updateButton.Click += async (_, _) => await ExecuteUpdateAsync();
        Controls.Add(_updateButton);
    }

    private static void ConfigureSecondaryButton(Button button, string text, int left, int top, int width)
    {
        button.Text = text;
        button.Left = left;
        button.Top = top;
        button.Width = width;
        button.Height = 38;
        button.BackColor = Color.FromArgb(30, 41, 59);
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(56, 189, 248);
    }

    private async Task LoadManifestAsync()
    {
        try
        {
            SetStatus(0, "Baixando manifest.json do GitHub...");

            var manifest = await _manifestService.LoadAsync(
                _config.ManifestUrl,
                CancellationToken.None);

            _files.Clear();
            _moduleList.Items.Clear();

            foreach (var file in manifest.Files)
            {
                var localPath = Path.Combine(_installDirectory, file.Name);
                file.LocalStatus = File.Exists(localPath)
                    ? "ENCONTRADO - será substituído com backup"
                    : "NÃO INSTALADO - nova instalação";

                _files.Add(file);
                _moduleList.Items.Add(file, false);
            }

            SetStatus(0, $"Lista carregada com sucesso. Versão do pacote: {manifest.Version}");
            _log.Info($"Manifest carregado. Versão: {manifest.Version}");
        }
        catch (Exception ex)
        {
            SetStatus(0, "Falha ao carregar manifest online. Usando lista local padrão.");

            _log.Warning($"Falha ao carregar manifest: {ex.Message}");

            var fallback = new List<ManifestFile>
            {
                new() { Name = "SmartNFe.exe", Description = "Módulo de NF-e", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/SmartNFe.exe" },
                new() { Name = "SmartNFSe.exe", Description = "Módulo de NFS-e", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/SmartNFSe.exe" },
                new() { Name = "SmartFood.exe", Description = "Módulo Food", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/SmartFood.exe" },
                new() { Name = "SmartCTE.exe", Description = "Módulo CT-e", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/SmartCTE.exe" },
                new() { Name = "SPED.exe", Description = "Módulo SPED", Url = "https://raw.githubusercontent.com/Hyroshido/Teste-do-atualizador-/main/SPED.exe" }
            };

            _files.Clear();
            _moduleList.Items.Clear();

            foreach (var file in fallback)
            {
                var localPath = Path.Combine(_installDirectory, file.Name);
                file.LocalStatus = File.Exists(localPath)
                    ? "ENCONTRADO - será substituído com backup"
                    : "NÃO INSTALADO - nova instalação";

                _files.Add(file);
                _moduleList.Items.Add(file, false);
            }
        }
    }

    private void UpdateSelectedCount()
    {
        var count = _moduleList.CheckedItems.Count;

        _selectedCountLabel.Text = count switch
        {
            0 => "Nenhum selecionado",
            1 => "1 selecionado",
            _ => $"{count} selecionados"
        };

        _updateButton.Enabled = count > 0;
    }

    private async Task ExecuteUpdateAsync()
    {
        var selected = new List<ManifestFile>();

        for (var i = 0; i < _moduleList.Items.Count; i++)
        {
            if (_moduleList.GetItemChecked(i))
            {
                selected.Add(_files[i]);
            }
        }

        if (selected.Count == 0)
        {
            MessageBox.Show("Selecione pelo menos um módulo para atualizar.", "Data Smart Enterprise");
            return;
        }

        var openModules = ProcessService.GetOpenModules(selected.Select(x => x.Name));
        if (openModules.Count > 0)
        {
            MessageBox.Show(
                "Feche os seguintes módulos antes de continuar:" + Environment.NewLine + Environment.NewLine +
                "- " + string.Join(Environment.NewLine + "- ", openModules),
                "Data Smart Enterprise",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        EnableControls(false);

        var backupService = new BackupService(_installDirectory, _backupDirectory, _log);
        var downloadService = new DownloadService(_config.DownloadTimeoutSeconds, _config.MinimumFileSizeBytes);
        var databaseUpdaterService = new DatabaseUpdaterService(_log);

        var updateService = new UpdateService(
            _installDirectory,
            _config,
            _log,
            backupService,
            downloadService,
            databaseUpdaterService);

        var progress = new Progress<(int percent, string message)>(p =>
        {
            SetStatus(p.percent, p.message);
        });

        var result = await updateService.ExecuteAsync(selected, progress, CancellationToken.None);

        _currentLogPath = result.LogPath;

        if (result.Success)
        {
            MessageBox.Show(
                "Atualização concluída com sucesso." + Environment.NewLine + Environment.NewLine +
                "Backup criado em:" + Environment.NewLine +
                result.BackupPath + Environment.NewLine + Environment.NewLine +
                "Atualizador de banco:" + Environment.NewLine +
                result.DatabaseUpdaterMessage + Environment.NewLine + Environment.NewLine +
                "Importante: o botão Processar arquivos NÃO foi acionado automaticamente." + Environment.NewLine + Environment.NewLine +
                "Log:" + Environment.NewLine +
                result.LogPath,
                "Data Smart Enterprise",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show(
                "A atualização falhou e os arquivos antigos foram restaurados quando havia backup." + Environment.NewLine + Environment.NewLine +
                "Erro:" + Environment.NewLine +
                result.ErrorMessage + Environment.NewLine + Environment.NewLine +
                "Backup:" + Environment.NewLine +
                result.BackupPath + Environment.NewLine + Environment.NewLine +
                "Log:" + Environment.NewLine +
                result.LogPath,
                "Data Smart Enterprise",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        EnableControls(true);
    }

    private void SetStatus(int percent, string message)
    {
        percent = Math.Clamp(percent, 0, 100);
        _progressBar.Value = percent;
        _percentLabel.Text = $"{percent}%";
        _statusLabel.Text = message;
    }

    private void EnableControls(bool enabled)
    {
        _moduleList.Enabled = enabled;
        _selectAllButton.Enabled = enabled;
        _unselectAllButton.Enabled = enabled;
        _updateButton.Enabled = enabled && _moduleList.CheckedItems.Count > 0;
        _closeButton.Enabled = enabled;
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace DataSmartUpdater;

public sealed class SplashForm : Form
{
    private readonly Label _statusLabel = new();
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly List<string> _messages = new()
    {
        "Inicializando ambiente...",
        "Validando configuracoes...",
        "Verificando atualizacoes...",
        "Conectando ao servidor..."
    };
    private int _currentMessage;

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        Width = 520;
        Height = 320;
        BackColor = Color.FromArgb(12, 16, 32);
        ShowInTaskbar = false;

        var logo = new Label
        {
            Text = "DATASMART DEPLOY CENTER",
            ForeColor = Color.FromArgb(56, 189, 248),
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Left = 30,
            Top = 30
        };
        Controls.Add(logo);

        var subtitle = new Label
        {
            Text = "Central Inteligente de Atualizacao e Implantacao",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            AutoSize = true,
            Left = 30,
            Top = 70
        };
        Controls.Add(subtitle);

        _statusLabel.Text = _messages[0];
        _statusLabel.ForeColor = Color.FromArgb(180, 200, 220);
        _statusLabel.Font = new Font("Segoe UI", 11, FontStyle.Regular);
        _statusLabel.AutoSize = true;
        _statusLabel.Left = 30;
        _statusLabel.Top = 130;
        Controls.Add(_statusLabel);

        var progress = new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Left = 30,
            Top = 180,
            Width = 460,
            Height = 20,
            Value = 0,
            ForeColor = Color.FromArgb(56, 189, 248)
        };
        Controls.Add(progress);

        var footer = new Label
        {
            Text = "Preparando a Central Inteligente de Implantacao...",
            ForeColor = Color.FromArgb(120, 140, 170),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = true,
            Left = 30,
            Top = 220
        };
        Controls.Add(footer);

        _timer.Interval = 850;
        _timer.Tick += (_, _) =>
        {
            _currentMessage++;
            if (_currentMessage >= _messages.Count)
            {
                _timer.Stop();
                Close();
                return;
            }

            _statusLabel.Text = _messages[_currentMessage];
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _timer.Start();
    }
}

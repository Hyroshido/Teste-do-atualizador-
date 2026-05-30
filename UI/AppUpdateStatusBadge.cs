namespace DataSmartUpdater.UI;

public enum UpdateStatus
{
    Checking,
    UpToDate,
    UpdateAvailable,
    Error
}

public class AppUpdateStatusBadge : Panel
{
    private Label _statusLabel = null!;
    private Label _dateLabel = null!;
    private Label _iconLabel = null!;
    private UpdateStatus _currentStatus = UpdateStatus.Checking;
    private DateTime _lastCheckTime = DateTime.Now;

    public AppUpdateStatusBadge()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        BackColor = ModernColors.BackgroundCard;
        BorderStyle = BorderStyle.None;
        Padding = new Padding(12);
        Height = 60;
        Dock = DockStyle.Top;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Margin = new Padding(0),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

        _iconLabel = new Label
        {
            Text = "⏳",
            Font = new Font("Segoe UI", 16),
            ForeColor = ModernColors.StatusInfo,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        layout.Controls.Add(_iconLabel, 0, 0);
        layout.SetRowSpan(_iconLabel, 2);

        _statusLabel = new Label
        {
            Text = "Verificando atualizações...",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = ModernColors.TextPrimary,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        };
        layout.Controls.Add(_statusLabel, 1, 0);

        _dateLabel = new Label
        {
            Text = "Última verificação: agora",
            Font = new Font("Segoe UI", 8),
            ForeColor = ModernColors.TextMuted,
            AutoSize = false,
            TextAlign = ContentAlignment.TopLeft,
            Dock = DockStyle.Fill
        };
        layout.Controls.Add(_dateLabel, 1, 1);

        var btnCheck = new ModernSecondaryButton
        {
            Text = "Verificar",
            Width = 110,
            Dock = DockStyle.Right,
            Margin = new Padding(0, 0, 0, 0)
        };
        layout.Controls.Add(btnCheck, 2, 0);
        layout.SetRowSpan(btnCheck, 2);

        Controls.Add(layout);
    }

    public void SetStatus(UpdateStatus status)
    {
        _currentStatus = status;
        _lastCheckTime = DateTime.Now;

        switch (status)
        {
            case UpdateStatus.Checking:
                _iconLabel.Text = "⏳";
                _statusLabel.Text = "Verificando atualizações...";
                _iconLabel.ForeColor = ModernColors.StatusInfo;
                break;

            case UpdateStatus.UpToDate:
                _iconLabel.Text = "✓";
                _statusLabel.Text = "Aplicação atualizada";
                _iconLabel.ForeColor = ModernColors.StatusSuccess;
                break;

            case UpdateStatus.UpdateAvailable:
                _iconLabel.Text = "⬆";
                _statusLabel.Text = "Atualização disponível";
                _iconLabel.ForeColor = ModernColors.StatusWarning;
                break;

            case UpdateStatus.Error:
                _iconLabel.Text = "✕";
                _statusLabel.Text = "Erro ao verificar";
                _iconLabel.ForeColor = ModernColors.StatusError;
                break;
        }

        _dateLabel.Text = $"Última verificação: {_lastCheckTime:dd/MM/yyyy HH:mm:ss}";
    }

    public UpdateStatus CurrentStatus => _currentStatus;
    public DateTime LastCheckTime => _lastCheckTime;
}

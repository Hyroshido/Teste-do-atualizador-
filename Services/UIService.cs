namespace DataSmartUpdater.Services;

using System.Diagnostics;

public sealed class UIService
{
    private readonly LogService _log;

    public UIService(LogService log)
    {
        _log = log;
    }

    public void ShowSuccessMessage(string title, string message)
    {
        _log.Info($"[SUCCESS] {title}: {message}");
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ShowErrorMessage(string title, string message, Exception? exception = null)
    {
        var fullMessage = exception != null ? $"{message}\n\nDetalhes:\n{exception.Message}" : message;
        _log.Error($"[ERROR] {title}: {fullMessage}");
        MessageBox.Show(fullMessage, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ShowWarningMessage(string title, string message)
    {
        _log.Warn($"[WARNING] {title}: {message}");
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public DialogResult ShowConfirmation(string title, string message)
    {
        _log.Info($"[CONFIRMATION] {title}: {message}");
        return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    }

    public void DisableControlsDuring(Control control, Func<Task> operation, string statusMessage = "Processando...")
    {
        var originalControls = GetAllControls(control).ToList();
        try
        {
            foreach (var ctrl in originalControls)
                ctrl.Enabled = false;

            operation().Wait();
        }
        catch (Exception ex)
        {
            _log.Error($"Erro durante operação: {ex.Message}");
            ShowErrorMessage("Erro", "Ocorreu um erro durante a operação.", ex);
        }
        finally
        {
            foreach (var ctrl in originalControls)
                ctrl.Enabled = true;
        }
    }

    public void SetButtonLoadingState(Button button, bool isLoading)
    {
        button.Enabled = !isLoading;
        button.Text = isLoading ? "⏳ Carregando..." : button.Tag?.ToString() ?? button.Text;
        button.Cursor = isLoading ? Cursors.WaitCursor : Cursors.Hand;
    }

    private static IEnumerable<Control> GetAllControls(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            yield return control;
            foreach (var child in GetAllControls(control))
                yield return child;
        }
    }
}

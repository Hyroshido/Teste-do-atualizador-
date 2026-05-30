using DataSmartUpdater.Services;

namespace DataSmartUpdater;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var config = AppConfig.Load();

            // Mostrar splash antes da janela principal para melhor UX
            using (var splash = new SplashForm())
            {
                splash.ShowDialog();
            }

            Application.Run(new MainForm(config));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao iniciar aplicacao: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

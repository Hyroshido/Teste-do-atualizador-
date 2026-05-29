using DataSmartUpdater.Services;

namespace DataSmartUpdater;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var config = AppConfig.Load();
        using var form = new MainForm(config);

        Application.Run(form);
    }
}

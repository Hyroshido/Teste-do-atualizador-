using DataSmartUpdater.Services;

namespace DataSmartUpdater;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var config = AppConfig.Load();
        Application.Run(new MainForm(config));
    }
}

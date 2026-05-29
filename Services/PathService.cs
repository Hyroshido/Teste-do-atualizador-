namespace DataSmartUpdater.Services;

public static class PathService
{
    public static string GetDataSmartPath()
    {
        try
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var links = Directory.GetFiles(desktop, "*.lnk");

            Type shellType = Type.GetTypeFromProgID("WScript.Shell")!;
            dynamic shell = Activator.CreateInstance(shellType)!;

            foreach (var link in links)
            {
                try
                {
                    dynamic shortcut = shell.CreateShortcut(link);
                    string target = shortcut.TargetPath;

                    if (!string.IsNullOrWhiteSpace(target) && target.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        var dir = Path.GetDirectoryName(target);
                        if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                            return dir;
                    }
                }
                catch { }
            }
        }
        catch { }

        var fallback = @"C:\DataSmart";
        Directory.CreateDirectory(fallback);
        return fallback;
    }
}

using System.Windows.Forms;

namespace DataSmartUpdater.Services;

public static class PathService
{
    private static readonly string[] KnownDataSmartFolders =
    {
        @"C:\DataSmart",
        @"D:\DataSmart",
        @"C:\Sistema\DataSmart",
        @"C:\DataSmart Sistemas"
    };

    private static readonly string[] KnownModuleFiles =
    {
        "SmartNFe.exe",
        "SmartNFSe.exe",
        "SmartFood.exe",
        "SmartCTE.exe",
        "SPED.exe",
        "SPED_Fiscal.exe",
        "SmartBackup.exe",
        "SmartBackup_SmartImoveis.exe",
        "SmartContador.exe",
        "SmartTools.exe",
        "SmartNFSe_260513.exe",
        "SmartNFe_Incopal.exe",
        "Atualizador de Banco de Dados.exe",
        "atualizador de banco de dados.exe"
    };

    public static bool IsValidDataSmartPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
                return false;

            if (KnownModuleFiles.Any(name => File.Exists(Path.Combine(fullPath, name))))
                return true;

            var exeFolder = Path.Combine(fullPath, "EXE");
            if (Directory.Exists(exeFolder) && KnownModuleFiles.Any(name => File.Exists(Path.Combine(exeFolder, name))))
                return true;
        }
        catch
        {
        }

        return false;
    }

    public static string FindDataSmartPath(string? configuredPath)
    {
        if (TryLocatePath(configuredPath, out var path))
            return path;

        foreach (var candidate in KnownDataSmartFolders)
        {
            if (TryLocatePath(candidate, out path))
                return path;
        }

        if (TryFindFromDesktopShortcut(out var shortcutPath))
            return shortcutPath;

        return AskUserForPath(configuredPath);
    }

    private static bool TryLocatePath(string? path, out string validPath)
    {
        validPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            var normalized = Path.GetFullPath(path);
            if (IsValidDataSmartPath(normalized))
            {
                validPath = normalized;
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    private static bool TryFindFromDesktopShortcut(out string path)
    {
        path = string.Empty;

        try
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var links = Directory.GetFiles(desktop, "*.lnk");

            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
                return false;

            dynamic shell = Activator.CreateInstance(shellType);
            foreach (var link in links)
            {
                try
                {
                    dynamic shortcut = shell.CreateShortcut(link);
                    string target = shortcut.TargetPath;

                    if (string.IsNullOrWhiteSpace(target) || !target.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var dir = Path.GetDirectoryName(target);
                    if (!string.IsNullOrWhiteSpace(dir) && IsValidDataSmartPath(dir))
                    {
                        path = dir;
                        return true;
                    }
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        return false;
    }

    private static string AskUserForPath(string? fallbackPath)
    {
        try
        {
            using var browser = new FolderBrowserDialog
            {
                Description = "Select the folder where DataSmart is installed.",
                SelectedPath = !string.IsNullOrWhiteSpace(fallbackPath) ? fallbackPath : KnownDataSmartFolders[0],
                ShowNewFolderButton = false
            };

            var result = browser.ShowDialog();
            if (result == DialogResult.OK && TryLocatePath(browser.SelectedPath, out var selectedPath))
                return selectedPath;
        }
        catch
        {
        }

        var fallback = KnownDataSmartFolders[0];
        Directory.CreateDirectory(fallback);
        return fallback;
    }
}

using System.Diagnostics;

namespace DataSmartUpdater.Services;

public static class VersionService
{
    public static string GetFileVersion(string path)
    {
        try
        {
            var info = FileVersionInfo.GetVersionInfo(path);
            var version = info.FileVersion;
            if (string.IsNullOrWhiteSpace(version))
                version = info.ProductVersion;

            return string.IsNullOrWhiteSpace(version) ? "Desconhecida" : version;
        }
        catch
        {
            return "Desconhecida";
        }
    }

    public static int CompareVersions(string a, string b)
    {
        if (Version.TryParse(a, out var va) && Version.TryParse(b, out var vb))
            return va.CompareTo(vb);

        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsUpdateAvailable(string localVersion, string remoteVersion)
    {
        if (string.IsNullOrWhiteSpace(remoteVersion))
            return false;

        if (string.Equals(localVersion, "Desconhecida", StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            return CompareVersions(localVersion, remoteVersion) < 0;
        }
        catch
        {
            return false;
        }
    }
}

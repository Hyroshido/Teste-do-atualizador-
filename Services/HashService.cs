using System.Security.Cryptography;

namespace DataSmartUpdater.Services;

public static class HashService
{
    public static string ComputeSha256(string path)
    {
        using var file = File.OpenRead(path);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(file);
        return Convert.ToHexString(hash);
    }

    public static bool MatchesSha256(string path, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(expectedHash))
            return true;

        try
        {
            var actualHash = ComputeSha256(path);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

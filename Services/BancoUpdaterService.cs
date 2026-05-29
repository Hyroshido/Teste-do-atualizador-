using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DataSmartUpdater.Services;

public sealed class BancoUpdaterService
{
    private readonly LogService _log;

    public BancoUpdaterService(LogService log)
    {
        _log = log;
    }

    public string OpenAndClickCarregar(string dataDir)
    {
        return OpenAndRun(dataDir, false, false);
    }

    public string OpenAndRun(string dataDir, bool processarArquivos, bool fecharAutomaticamente)
    {
        var exe = Path.Combine(dataDir, "Atualizador de Banco de dados.exe");

        if (!File.Exists(exe))
        {
            exe = Path.Combine(dataDir, "atualizador de banco de dados.exe");
        }

        if (!File.Exists(exe))
        {
            _log.Warn("Atualizador de banco de dados não encontrado.");
            return "Atualizador de banco não encontrado.";
        }

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                WorkingDirectory = dataDir,
                UseShellExecute = true
            });

            if (process == null)
                return "Atualizador de banco não abriu.";

            for (int i = 0; i < 40; i++)
            {
                Thread.Sleep(500);
                process.Refresh();

                if (process.MainWindowHandle != IntPtr.Zero)
                    break;
            }

            if (process.MainWindowHandle == IntPtr.Zero)
                return "Atualizador abriu, mas a janela não foi localizada.";

            Thread.Sleep(1000);

            var clicked = ClickButtonByText(process.MainWindowHandle, "Carregar");

            if (!clicked)
            {
                _log.Warn("Botão Carregar arquivos não encontrado.");
                return "Atualizador aberto, mas o botão Carregar arquivos não foi encontrado. Clique manualmente.";
            }

            _log.Info("Clique automático realizado no botão Carregar arquivos.");
            var result = "Atualizador aberto e botão Carregar arquivos acionado.";

            if (processarArquivos)
            {
                Thread.Sleep(1500);
                var clickedProcessar = ClickButtonByText(process.MainWindowHandle, "Processar");

                if (clickedProcessar)
                {
                    _log.Info("Clique automático realizado no botão Processar.");
                    result = "Atualizador aberto, Carregar acionado e Processar acionado.";
                }
                else
                {
                    _log.Warn("Botão Processar não encontrado.");
                    result = "Atualizador aberto e Carregar acionado, mas o botão Processar não foi encontrado. Clique manualmente.";
                }
            }

            if (fecharAutomaticamente)
            {
                Thread.Sleep(1500);
                ClickButtonByText(process.MainWindowHandle, "Fechar");
                process.Refresh();
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                }
                _log.Info("Tentativa automática de fechamento do atualizador de banco.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _log.Warn($"Falha ao abrir/clicar no atualizador de banco: {ex.Message}");
            return "Atualizador de banco não pôde ser acionado automaticamente.";
        }
    }

    private static bool ClickButtonByText(IntPtr parent, string text)
    {
        bool clicked = false;

        EnumChildWindows(parent, (hWnd, lParam) =>
        {
            var sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            var title = sb.ToString();

            if (title.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                SendMessage(hWnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                clicked = true;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return clicked;
    }

    private const int BM_CLICK = 0x00F5;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}

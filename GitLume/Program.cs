using System;
using System.IO;
using System.Windows.Forms;

namespace GitLume;

static class Program
{
    // 崩溃日志写到软件所在目录（不往系统盘写任何文件）
    private static readonly string CrashLog =
        Path.Combine(AppContext.BaseDirectory, "crash.log");

    /// <summary>应用程序主入口。</summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // 全局异常兜底：记录崩溃日志，避免程序无声退出
        AppDomain.CurrentDomain.UnhandledException += (_, e) => LogCrash("UnhandledException", e.ExceptionObject as Exception);
        Application.ThreadException += (_, e) => LogCrash("ThreadException", e.Exception);

        try
        {
            Application.Run(new UI.MainForm());
        }
        catch (Exception ex)
        {
            LogCrash("Main", ex);
            throw;
        }
    }

    private static void LogCrash(string stage, Exception? ex)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CrashLog)!);
            File.AppendAllText(CrashLog,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {stage}\n{ex}\n--------------------------------\n");
        }
        catch
        {
            // 日志写入失败忽略
        }
    }
}

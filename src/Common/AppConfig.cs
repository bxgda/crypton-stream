using System;
using System.IO;

namespace src.Common;

public class AppConfig
{
    public static string LogsDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    public static string ReceivedFilesDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "received_files");

    public static int DefaultPort { get; set; } = 9000;

    public static void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(LogsDirectory)) Directory.CreateDirectory(LogsDirectory);
        if (!Directory.Exists(ReceivedFilesDirectory)) Directory.CreateDirectory(ReceivedFilesDirectory);
    }
}

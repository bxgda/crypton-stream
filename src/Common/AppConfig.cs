using System;
using System.IO;
using System.Linq;

namespace src.Common;

public class AppConfig
{
    public static string SecretWord { get; set; } = GenerateRandomSecret();

    public static string LogsDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    public static string ReceivedFilesDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "received_files");

    public static int DefaultPort { get; set; } = 9000;

    private static string GenerateRandomSecret(int length = 32)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        var rnd = Random.Shared;

        return new string(
            Enumerable.Range(0, length)
                      .Select(_ => chars[rnd.Next(chars.Length)])
                      .ToArray()
        );
    }

    public static void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(LogsDirectory)) Directory.CreateDirectory(LogsDirectory);
        if (!Directory.Exists(ReceivedFilesDirectory)) Directory.CreateDirectory(ReceivedFilesDirectory);
    }
}

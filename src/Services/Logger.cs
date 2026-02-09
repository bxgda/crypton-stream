using System;
using System.IO;
using src.Common;

namespace src.Services;

public static class Logger
{
    private static readonly object _lock = new object();

    private static string GetCurrentLogPath()
    {
        string fileName = $"log_{DateTime.Now:yyyy-MM-dd_HH}.txt";
        return Path.Combine(AppConfig.LogsDirectory, fileName);
    }

    public static void Log(string message, FileMetadata? metadata = null)
    {
        lock (_lock)
        {
            try
            {
                if (!Directory.Exists(AppConfig.LogsDirectory))
                    Directory.CreateDirectory(AppConfig.LogsDirectory);

                string logPath = GetCurrentLogPath();
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string logEntry = $"[{timestamp}] {message}";

                if (metadata != null)
                {
                    logEntry += Environment.NewLine +
                                $"      |-- Fajl: {metadata.FileName}" + Environment.NewLine +
                                $"      |-- Velicina: {FormatSize(metadata.FileSize)}" + Environment.NewLine +
                                $"      |-- Algo: {metadata.EncryptingAlgorithm}" + Environment.NewLine +
                                $"      |-- Hash (Cipher): {metadata.HashValue}" + Environment.NewLine +
                                $"      |-- Nonce: {(metadata.Nonce.HasValue ? metadata.Nonce.Value.ToString() : "N/A")}";
                }

                using var stream = new FileStream(
                    logPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite
                );

                using var writer = new StreamWriter(stream);

                writer.WriteLine(logEntry);
                writer.WriteLine(new string('-', 50));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger error: {ex.Message}");
            }
        }
    }

    private static string FormatSize(long bytes)
    {
        string[] suffix = { "B", "KB", "MB", "GB", "TB" };
        int i;
        double dblSByte = bytes;

        for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            dblSByte = bytes / 1024.0;

        return $"{dblSByte:0.##} {suffix[i]}";
    }
}

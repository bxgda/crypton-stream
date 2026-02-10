using System;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using src.Common;

namespace src.ViewModels;

public partial class LogsViewModel : ViewModelBase
{
    private DispatcherTimer? _timer;

    [ObservableProperty]
    private string _logContent = "No log entries yet.";

    [ObservableProperty]
    private bool _isRefreshing;

    public void StartRefreshing()
    {
        if (_timer != null) return;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += (_, _) => LoadLogs();
        _timer.Start();
        IsRefreshing = true;

        // Immediate first load
        LoadLogs();
    }

    public void StopRefreshing()
    {
        _timer?.Stop();
        _timer = null;
        IsRefreshing = false;
    }

    private void LoadLogs()
    {
        try
        {
            string logsDir = AppConfig.LogsDirectory;
            if (!Directory.Exists(logsDir))
            {
                LogContent = "Logs directory does not exist yet.";
                return;
            }

            var logFiles = Directory.GetFiles(logsDir, "log_*.txt")
                                    .OrderBy(f => f)
                                    .ToArray();

            if (logFiles.Length == 0)
            {
                LogContent = "No log files found.";
                return;
            }

            var sb = new StringBuilder();
            foreach (var file in logFiles)
            {
                try
                {
                    using var stream = new FileStream(
                        file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    sb.Append(reader.ReadToEnd());
                }
                catch
                {
                    // Skip files that can't be read
                }
            }

            var content = sb.ToString().TrimEnd();
            LogContent = string.IsNullOrEmpty(content)
                ? "Log files are empty."
                : content;
        }
        catch (Exception ex)
        {
            LogContent = $"Error reading logs: {ex.Message}";
        }
    }
}

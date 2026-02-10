using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using src.Common;

namespace src.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    // ── Secret Key ──
    [ObservableProperty]
    private string _secretKey = AppConfig.SecretWord;

    [ObservableProperty]
    private bool _showSecretKey;

    public string EyeIcon => ShowSecretKey ? "👁" : "👁‍🗨";

    partial void OnShowSecretKeyChanged(bool value) => OnPropertyChanged(nameof(EyeIcon));

    // ── Logs section ──
    [ObservableProperty]
    private string _logsDirectory = AppConfig.LogsDirectory;

    [ObservableProperty]
    private bool _useDefaultLogsDir = true;

    partial void OnUseDefaultLogsDirChanged(bool value)
    {
        if (value)
            LogsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
    }

    // ── Network section ──
    [ObservableProperty]
    private string _port = AppConfig.DefaultPort.ToString();

    [ObservableProperty]
    private string _receivedFilesDirectory = AppConfig.ReceivedFilesDirectory;

    [ObservableProperty]
    private bool _useDefaultReceivedDir = true;

    partial void OnUseDefaultReceivedDirChanged(bool value)
    {
        if (value)
            ReceivedFilesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "received_files");
    }

    // ── File System Watcher section ──
    [ObservableProperty]
    private string _watcherTargetDirectory = AppConfig.WatcherTargetDirectory;

    [ObservableProperty]
    private bool _useDefaultWatcherTarget = true;

    partial void OnUseDefaultWatcherTargetChanged(bool value)
    {
        if (value)
            WatcherTargetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "watcher_target");
    }

    [ObservableProperty]
    private string _watcherOutputDirectory = AppConfig.WatcherOutputDirectory;

    [ObservableProperty]
    private bool _useDefaultWatcherOutput = true;

    partial void OnUseDefaultWatcherOutputChanged(bool value)
    {
        if (value)
            WatcherOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "watcher_output_x");
    }

    // ── Status ──
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    // ── Commands ──

    [RelayCommand]
    private void ToggleShowSecret()
    {
        ShowSecretKey = !ShowSecretKey;
    }

    [RelayCommand]
    private async Task BrowseLogsDirectory()
    {
        var folder = await PickFolder("Select Logs Directory");
        if (folder != null)
        {
            LogsDirectory = folder;
            UseDefaultLogsDir = false;
        }
    }

    [RelayCommand]
    private async Task BrowseReceivedDirectory()
    {
        var folder = await PickFolder("Select Received Files Directory");
        if (folder != null)
        {
            ReceivedFilesDirectory = folder;
            UseDefaultReceivedDir = false;
        }
    }

    [RelayCommand]
    private async Task BrowseWatcherTarget()
    {
        var folder = await PickFolder("Select Watcher Target Directory");
        if (folder != null)
        {
            WatcherTargetDirectory = folder;
            UseDefaultWatcherTarget = false;
        }
    }

    [RelayCommand]
    private async Task BrowseWatcherOutput()
    {
        var folder = await PickFolder("Select Watcher Output Directory");
        if (folder != null)
        {
            WatcherOutputDirectory = folder;
            UseDefaultWatcherOutput = false;
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            // Secret key
            if (!string.IsNullOrWhiteSpace(SecretKey))
                AppConfig.SecretWord = SecretKey;

            // Logs
            AppConfig.LogsDirectory = LogsDirectory;

            // Network
            if (int.TryParse(Port, out int portNum) && portNum > 0 && portNum <= 65535)
                AppConfig.DefaultPort = portNum;
            else
            {
                StatusMessage = "Invalid port number (1-65535).";
                IsSuccess = false;
                return;
            }
            AppConfig.ReceivedFilesDirectory = ReceivedFilesDirectory;

            // File System Watcher
            AppConfig.WatcherTargetDirectory = WatcherTargetDirectory;
            AppConfig.WatcherOutputDirectory = WatcherOutputDirectory;

            AppConfig.EnsureDirectoriesExist();

            StatusMessage = "Settings saved successfully!";
            IsSuccess = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
            IsSuccess = false;
        }
    }

    // ── Helpers ──

    private static async Task<string?> PickFolder(string title)
    {
        var topLevel = GetTopLevel();
        if (topLevel == null) return null;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    private static TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}

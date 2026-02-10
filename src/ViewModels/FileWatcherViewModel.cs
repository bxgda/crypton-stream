using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using src.Common;
using src.Services;

namespace src.ViewModels;

public partial class FileWatcherViewModel : ViewModelBase
{
    private FileSystemWatcherService? _watcherService;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusMessage = "Watcher is stopped.";

    [ObservableProperty]
    private bool _isStatusSuccess;

    // Algorithm selection
    [ObservableProperty]
    private bool _isA52Selected = true;

    [ObservableProperty]
    private bool _isSubstitutionSelected;

    public string ToggleText => IsRunning ? "⏹  Stop Watcher" : "▶  Start Watcher";

    partial void OnIsRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(ToggleText));
    }

    partial void OnIsA52SelectedChanged(bool value)
    {
        if (value) IsSubstitutionSelected = false;
    }

    partial void OnIsSubstitutionSelectedChanged(bool value)
    {
        if (value) IsA52Selected = false;
    }

    [RelayCommand]
    private void ToggleWatcher()
    {
        if (IsRunning)
        {
            _watcherService?.Stop();
            _watcherService = null;
            IsRunning = false;
            StatusMessage = "Watcher stopped.";
            IsStatusSuccess = false;
        }
        else
        {
            try
            {
                AppConfig.EnsureDirectoriesExist();

                _watcherService = new FileSystemWatcherService(
                    AppConfig.WatcherTargetDirectory,
                    AppConfig.WatcherOutputDirectory,
                    AppConfig.SecretWord);

                var algorithm = IsA52Selected
                    ? EncryptionAlgorithm.A5_2
                    : EncryptionAlgorithm.SimpleSubstitution;

                _watcherService.Start(algorithm);
                IsRunning = true;
                var folderName = System.IO.Path.GetFileName(AppConfig.WatcherTargetDirectory.TrimEnd(
                    System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
                StatusMessage = $"Watching: {folderName}/";
                IsStatusSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                IsStatusSuccess = false;
            }
        }
    }
}

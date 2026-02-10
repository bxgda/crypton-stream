using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace src.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private bool _isLocalActive = true;

    [ObservableProperty]
    private bool _isNetworkActive;

    [ObservableProperty]
    private bool _isFileWatcherActive;

    [ObservableProperty]
    private bool _isLogsActive;

    [ObservableProperty]
    private bool _isSettingsActive;

    public LocalViewModel LocalVm { get; }
    public NetworkViewModel NetworkVm { get; }
    public FileWatcherViewModel FileWatcherVm { get; }
    public LogsViewModel LogsVm { get; }
    public SettingsViewModel SettingsVm { get; }

    public MainWindowViewModel()
    {
        LocalVm = new LocalViewModel();
        NetworkVm = new NetworkViewModel();
        FileWatcherVm = new FileWatcherViewModel();
        LogsVm = new LogsViewModel();
        SettingsVm = new SettingsViewModel();
        _currentPage = LocalVm;
    }

    private void ClearActiveFlags()
    {
        IsLocalActive = false;
        IsNetworkActive = false;
        IsFileWatcherActive = false;
        IsLogsActive = false;
        IsSettingsActive = false;
    }

    [RelayCommand]
    private void NavigateTo(string page)
    {
        // Stop logs timer when navigating away
        if (CurrentPage == LogsVm && page != "Logs")
            LogsVm.StopRefreshing();

        ClearActiveFlags();
        switch (page)
        {
            case "Local":
                CurrentPage = LocalVm;
                IsLocalActive = true;
                break;
            case "Network":
                CurrentPage = NetworkVm;
                IsNetworkActive = true;
                break;
            case "FileWatcher":
                CurrentPage = FileWatcherVm;
                IsFileWatcherActive = true;
                break;
            case "Logs":
                CurrentPage = LogsVm;
                IsLogsActive = true;
                LogsVm.StartRefreshing();
                break;
            case "Settings":
                CurrentPage = SettingsVm;
                IsSettingsActive = true;
                break;
        }
    }
}

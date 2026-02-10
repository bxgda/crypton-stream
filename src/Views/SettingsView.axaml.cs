using Avalonia.Controls;
using Avalonia.Media;
using src.ViewModels;
using System.ComponentModel;

namespace src.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if ((e.PropertyName == nameof(SettingsViewModel.StatusMessage) ||
             e.PropertyName == nameof(SettingsViewModel.IsSuccess)) &&
            sender is SettingsViewModel vm && StatusText != null)
        {
            StatusText.Foreground = vm.IsSuccess
                ? new SolidColorBrush(Color.Parse("#22C55E"))
                : new SolidColorBrush(Color.Parse("#EF4444"));
        }
    }
}

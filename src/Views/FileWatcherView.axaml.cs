using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using src.ViewModels;
using System.ComponentModel;

namespace src.Views;

public partial class FileWatcherView : UserControl
{
    public FileWatcherView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is FileWatcherViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not FileWatcherViewModel vm) return;

        if (e.PropertyName == nameof(FileWatcherViewModel.IsRunning) && StatusDot != null)
        {
            StatusDot.Fill = vm.IsRunning
                ? new SolidColorBrush(Color.Parse("#22C55E"))
                : new SolidColorBrush(Color.Parse("#71717A"));
        }
    }
}

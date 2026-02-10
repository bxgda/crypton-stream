using Avalonia.Controls;
using src.ViewModels;
using System.ComponentModel;

namespace src.Views;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is LogsViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LogsViewModel.LogContent) && LogScrollViewer != null)
        {
            // Auto-scroll to bottom when new content loads
            LogScrollViewer.ScrollToEnd();
        }
    }
}

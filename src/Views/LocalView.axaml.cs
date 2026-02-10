using Avalonia.Controls;
using Avalonia.Media;
using src.ViewModels;
using System.ComponentModel;

namespace src.Views;

public partial class LocalView : UserControl
{
    public LocalView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is LocalViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocalViewModel.StatusMessage) ||
            e.PropertyName == nameof(LocalViewModel.IsSuccess))
        {
            if (sender is LocalViewModel vm && StatusText != null)
            {
                StatusText.Foreground = vm.IsSuccess
                    ? new SolidColorBrush(Color.Parse("#22C55E"))
                    : new SolidColorBrush(Color.Parse("#EF4444"));
            }
        }
    }
}

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using src.ViewModels;
using System.ComponentModel;

namespace src.Views;

public partial class NetworkView : UserControl
{
    public NetworkView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is NetworkViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not NetworkViewModel vm) return;

        if ((e.PropertyName == nameof(NetworkViewModel.SendStatus) ||
             e.PropertyName == nameof(NetworkViewModel.IsSendSuccess)) && SendStatusText != null)
        {
            SendStatusText.Foreground = vm.IsSendSuccess
                ? new SolidColorBrush(Color.Parse("#22C55E"))
                : new SolidColorBrush(Color.Parse("#EF4444"));
        }

        if ((e.PropertyName == nameof(NetworkViewModel.IsListening) ||
             e.PropertyName == nameof(NetworkViewModel.IsListenSuccess)) && StatusDot != null)
        {
            StatusDot.Fill = vm.IsListening
                ? new SolidColorBrush(Color.Parse("#22C55E"))
                : new SolidColorBrush(Color.Parse("#71717A"));
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using src.Common;
using src.Services;

namespace src.ViewModels;

public partial class NetworkViewModel : ViewModelBase
{
    public string PageTitle { get; } = "Network";
    public string Description { get; } = "Send and receive encrypted files over the network.";

    private readonly NetworkService _networkService = new();

    // ── Send section ──
    [ObservableProperty]
    private string _sendFilePath = string.Empty;

    [ObservableProperty]
    private string _targetIpAddress = string.Empty;

    [ObservableProperty]
    private int _selectedAlgorithmIndex;

    [ObservableProperty]
    private bool _isA52Selected = true;

    [ObservableProperty]
    private bool _isSubstitutionSelected;

    [ObservableProperty]
    private bool _isSending;

    [ObservableProperty]
    private string _sendStatus = string.Empty;

    [ObservableProperty]
    private bool _isSendSuccess;

    // ── Listen section ──
    [ObservableProperty]
    private bool _isListening;

    [ObservableProperty]
    private string _listenStatus = "Listener is off.";

    [ObservableProperty]
    private bool _isListenSuccess;

    // ── My IP ──
    public string MyIpAddress { get; }

    public string ListenToggleText => IsListening ? "⏹  Stop Listening" : "▶  Start Listening";

    public NetworkViewModel()
    {
        MyIpAddress = GetLocalIpAddress();
    }

    partial void OnIsListeningChanged(bool value)
    {
        OnPropertyChanged(nameof(ListenToggleText));
    }

    partial void OnIsA52SelectedChanged(bool value)
    {
        if (value)
        {
            IsSubstitutionSelected = false;
            SelectedAlgorithmIndex = 0;
        }
    }

    partial void OnIsSubstitutionSelectedChanged(bool value)
    {
        if (value)
        {
            IsA52Selected = false;
            SelectedAlgorithmIndex = 1;
        }
    }

    [RelayCommand]
    private async Task BrowseSendFile()
    {
        var topLevel = GetTopLevel();
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select File to Send",
            AllowMultiple = false
        });

        if (files.Count > 0)
            SendFilePath = files[0].Path.LocalPath;
    }

    [RelayCommand]
    private async Task SendFile()
    {
        SendStatus = string.Empty;

        if (string.IsNullOrWhiteSpace(SendFilePath) || !File.Exists(SendFilePath))
        {
            SendStatus = "Please select a valid file to send.";
            IsSendSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetIpAddress))
        {
            SendStatus = "Please enter the target IP address.";
            IsSendSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(AppConfig.SecretWord))
        {
            SendStatus = "Secret key not set. Go to Settings first.";
            IsSendSuccess = false;
            return;
        }

        IsSending = true;

        try
        {
            var algorithm = SelectedAlgorithmIndex == 0
                ? EncryptionAlgorithm.A5_2
                : EncryptionAlgorithm.SimpleSubstitution;

            await _networkService.SendFileAsync(
                SendFilePath,
                TargetIpAddress.Trim(),
                AppConfig.SecretWord,
                algorithm);

            SendStatus = $"File sent successfully to {TargetIpAddress}!";
            IsSendSuccess = true;
        }
        catch (Exception ex)
        {
            SendStatus = $"Error: {ex.Message}";
            IsSendSuccess = false;
        }
        finally
        {
            IsSending = false;
        }
    }

    [RelayCommand]
    private void ToggleListening()
    {
        if (IsListening)
        {
            _networkService.StopListening();
            IsListening = false;
            ListenStatus = "Listener stopped.";
            IsListenSuccess = false;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(AppConfig.SecretWord))
            {
                ListenStatus = "Secret key not set. Go to Settings first.";
                IsListenSuccess = false;
                return;
            }

            try
            {
                _networkService.StartListening(
                    AppConfig.ReceivedFilesDirectory,
                    AppConfig.SecretWord);

                IsListening = true;
                ListenStatus = $"Listening on port {AppConfig.DefaultPort}...";
                IsListenSuccess = true;
            }
            catch (Exception ex)
            {
                ListenStatus = $"Error: {ex.Message}";
                IsListenSuccess = false;
            }
        }
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up
                         && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var ni in networkInterfaces)
            {
                var ipProps = ni.GetIPProperties();
                var addr = ipProps.UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                if (addr != null)
                    return addr.Address.ToString();
            }

            // Fallback
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            if (socket.LocalEndPoint is IPEndPoint endPoint)
                return endPoint.Address.ToString();
        }
        catch { }

        return "Unknown";
    }

    private static TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}

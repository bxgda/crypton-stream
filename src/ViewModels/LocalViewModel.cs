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
using src.Services;

namespace src.ViewModels;

public partial class LocalViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _inputFilePath = string.Empty;

    [ObservableProperty]
    private string _outputFolderPath = string.Empty;

    [ObservableProperty]
    private bool _isEncryptMode = true;

    [ObservableProperty]
    private bool _isDecryptMode;

    [ObservableProperty]
    private int _selectedAlgorithmIndex;

    [ObservableProperty]
    private bool _isA52Selected = true;

    [ObservableProperty]
    private bool _isSubstitutionSelected;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private bool _isProcessing;

    public string ExecuteButtonText => IsEncryptMode ? "🔐  Encrypt File" : "🔓  Decrypt File";

    public string[] AvailableAlgorithms { get; } = { "A5/2 (Stream Cipher)", "Simple Substitution" };

    partial void OnIsEncryptModeChanged(bool value)
    {
        if (value) IsDecryptMode = false;
        OnPropertyChanged(nameof(ExecuteButtonText));
    }

    partial void OnIsDecryptModeChanged(bool value)
    {
        if (value) IsEncryptMode = false;
        OnPropertyChanged(nameof(ExecuteButtonText));
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
    private async Task BrowseInputFile()
    {
        var topLevel = GetTopLevel();
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Input File",
            AllowMultiple = false
        });

        if (files.Count > 0)
        {
            InputFilePath = files[0].Path.LocalPath;
        }
    }

    [RelayCommand]
    private async Task BrowseOutputFolder()
    {
        var topLevel = GetTopLevel();
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Output Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            OutputFolderPath = folders[0].Path.LocalPath;
        }
    }

    [RelayCommand]
    private async Task Execute()
    {
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(InputFilePath) || !File.Exists(InputFilePath))
        {
            StatusMessage = "Please select a valid input file.";
            IsSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputFolderPath) || !Directory.Exists(OutputFolderPath))
        {
            StatusMessage = "Please select a valid output folder.";
            IsSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(AppConfig.SecretWord))
        {
            StatusMessage = "Secret key not set. Go to Settings first.";
            IsSuccess = false;
            return;
        }

        IsProcessing = true;

        try
        {
            await Task.Run(() =>
            {
                var algorithm = SelectedAlgorithmIndex == 0
                    ? EncryptionAlgorithm.A5_2
                    : EncryptionAlgorithm.SimpleSubstitution;

                if (IsEncryptMode)
                {
                    SystemFile.EncryptFile(InputFilePath, OutputFolderPath, AppConfig.SecretWord, algorithm);
                }
                else
                {
                    SystemFile.DecryptFile(InputFilePath, OutputFolderPath, AppConfig.SecretWord);
                }
            });

            StatusMessage = IsEncryptMode
                ? "File encrypted successfully!"
                : "File decrypted successfully!";
            IsSuccess = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private static TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}

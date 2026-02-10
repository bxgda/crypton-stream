using System;
using System.IO;
using System.Threading;
using src.Common;
using src.Core;

namespace src.Services;

public class FileSystemWatcherService
{
    private FileSystemWatcher? _watcher;
    private readonly string _targetPath;
    private readonly string _outputPath;
    private readonly string _key;
    private EncryptionAlgorithm _selectedAlgorithm;

    public FileSystemWatcherService(string targetPath, string outputPath, string key)
    {
        _targetPath = targetPath;
        _outputPath = outputPath;
        _key = key;
    }

    public void Start(EncryptionAlgorithm algorithm)
    {
        if (_watcher != null) Stop();

        _selectedAlgorithm = algorithm;
        _watcher = new FileSystemWatcher(_targetPath);

        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;

        _watcher.IncludeSubdirectories = false;
        _watcher.EnableRaisingEvents = true;

        Logger.Log($"File system watcher started on: {_targetPath}, with algorithm: {_selectedAlgorithm}");
    }

    public void Stop()
    {
        if (_watcher == null) return;

        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;

        Logger.Log("File system watcher stopped.");
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        Logger.Log($"File system watcher detected new file: '{e.Name}'");

        ThreadPool.QueueUserWorkItem(_ =>
        {
            WaitForFile(e.FullPath);
            try
            {
                SystemFile.EncryptFile(e.FullPath, _outputPath, _key, _selectedAlgorithm);
                Logger.Log($"File system watcher encrypted file: '{e.Name}'");
            }
            catch (Exception ex) { Logger.Log($"File system watcher error: Problem encrypting {e.Name}: {ex.Message}"); }
        });
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Logger.Log($"Fyle system watcher detected that file: '{e.Name}' is deleted");
    }

    private void WaitForFile(string filePath)
    {
        int retries = 10;
        while (retries > 0)
        {
            try
            {
                using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return;
            }
            catch (IOException)
            {
                retries--;
                Thread.Sleep(500);
            }
        }
    }
}

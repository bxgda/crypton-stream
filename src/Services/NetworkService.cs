using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using src.Common;
using src.Factories;
using src.Interfaces;

namespace src.Services;

public class NetworkService
{
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private bool _isListening;

    public async Task SendFileAsync(string filePath, string targetIp, string key, EncryptionAlgorithm algorithm)
    {
        try
        {
            using var client = new TcpClient();

            var connectTask = client.ConnectAsync(targetIp, AppConfig.DefaultPort);
            if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                throw new Exception("Connection timeout. Is the other student listening?");

            using var netStream = client.GetStream();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            ICryptoStrategy strategy = CryptoStrategyFactory.CreateForEncryption(algorithm, key);

            await Task.Run(() =>
            {
                CryptoManager.PackAndEncrypt(fileStream, netStream, strategy, Path.GetFileName(filePath));
            });

            Logger.Log($"Network: File '{filePath}' sent to: {targetIp}", null);
        }
        catch (Exception ex)
        {
            throw new Exception($"Network send error: {ex.Message}");
        }
    }

    public void StartListening(string downloadDirectory, string key)
    {
        if (_isListening) return;

        _isListening = true;
        _cts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            _listener = new TcpListener(IPAddress.Any, AppConfig.DefaultPort);
            _listener.Start();
            Logger.Log("Network: Listening started...", null);

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // cekako asinhrono na konekciju - ne blokiramo glavnu nit i možemo primati vise konekcija
                    var client = await _listener.AcceptTcpClientAsync();

                    // cim dobijemo klijenta, obradjujemo ga u posebnom pod-zadatku 
                    // da ne bismo blokirali ostale koji mozda zele da salju
                    _ = Task.Run(async () => HandleConnection(client, downloadDirectory, key));
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            { Logger.Log($"Network critical exception: {ex.Message}", null); }
            finally { _isListening = false; _listener.Stop(); }
        });
    }

    public void StopListening()
    {
        if (!_isListening) return;

        _cts?.Cancel();
        _listener?.Stop();
        _isListening = false;
        Logger.Log("Network: Listening stopped.", null);
    }

    private async Task HandleConnection(TcpClient client, string downloadDir, string key)
    {
        string fullDestPath = null;
        bool success = false;

        try
        {
            using (client)
            using (var netStream = client.GetStream())
            {
                var metadata = CryptoManager.ReadMetadata(netStream);

                Logger.Log($"Network: Receiving file '{metadata.FileName}'...", null);

                if (!Directory.Exists(downloadDir)) Directory.CreateDirectory(downloadDir);

                fullDestPath = Path.Combine(downloadDir, "DECRYPTED_" + metadata.FileName);

                using (var outputFs = new FileStream(fullDestPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536))
                {
                    CryptoManager.DecryptAndVerify(netStream, outputFs, metadata, key);
                }

                success = true;
                Logger.Log($"Network: Successfully received and decrypted '{metadata.FileName}'", metadata);
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Network Receive Error: {ex.Message}", null);

            // ako ne valja onda brisemo fajl
            if (!success && fullDestPath != null && File.Exists(fullDestPath))
            {
                try { File.Delete(fullDestPath); } catch { }
                Logger.Log($"Network: Corrupted file deleted '{fullDestPath}'", null);
            }
        }
    }
}

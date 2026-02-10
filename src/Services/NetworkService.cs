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
    private int Port = AppConfig.DefaultPort;
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private bool _isListening;

    public NetworkService()
    {
        _listener = new TcpListener(IPAddress.Any, Port);
    }

    public async Task SendFileAsync(string filePath, string targetIp, string key, EncryptionAlgorithm algorithm)
    {
        try
        {
            using var client = new TcpClient();

            var connectTask = client.ConnectAsync(targetIp, Port);
            if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                throw new Exception("Connection timeout. Is the other student listening?");

            using var netStream = client.GetStream();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            ICryptoStrategy strategy = CryptoStrategyFactory.CreateForEncryption(algorithm, key);

            CryptoManager.PackAndEncrypt(fileStream, netStream, strategy, Path.GetFileName(filePath));

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
            _listener.Start();
            Logger.Log("Network: Listening started...", null);

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // cekako asinhrono na konekciju - ne blokiramo glavnu nit i možemo primati vise konekcija
                    using var client = await _listener.AcceptTcpClientAsync();

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

    private async Task HandleConnection(TcpClient client, string downloadDir, string key)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".incoming");
        try
        {
            using (var netStream = client.GetStream())
            using (var tempStream = new FileStream(tempPath, FileMode.Create))
            {
                await netStream.CopyToAsync(tempStream);
            }

            SystemFile.DecryptFile(tempPath, downloadDir, key);
        }
        catch (Exception ex) { Logger.Log("Receive Error: " + ex.Message); }
        finally { if (File.Exists(tempPath)) File.Delete(tempPath); }
    }
}

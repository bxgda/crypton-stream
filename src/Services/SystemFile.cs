using System;
using System.IO;
using src.Common;
using src.Factories;
using src.Interfaces;

namespace src.Services;

public static class SystemFile
{
    public static void EncryptFile(string srcPath, string destDirectory, string key, EncryptionAlgorithm algorithm)
    {
        try
        {
            if (!Directory.Exists(destDirectory)) Directory.CreateDirectory(destDirectory);

            string destFileName = Path.GetFileNameWithoutExtension(srcPath) + ".crypto";
            string destPath = Path.Combine(destDirectory, destFileName);

            using var inputStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read);
            using var outputStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);

            ICryptoStrategy strategy = CryptoStrategyFactory.CreateForEncryption(algorithm, key);
            var meta = CryptoManager.PackAndEncrypt(inputStream, outputStream, strategy, Path.GetFileName(srcPath));

            Logger.Log($"Encrypted file '{srcPath}' to '{destPath}'", meta);
        }
        catch (Exception ex) { throw new Exception($"System file error encrypting file: {ex.Message}"); }
    }

    public static void DecryptFile(string srcPath, string destDirectory, string key)
    {
        string fullDestPath = null;
        bool success = false;

        try
        {
            if (!Directory.Exists(destDirectory)) Directory.CreateDirectory(destDirectory);

            using var input = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);

            var metadata = CryptoManager.ReadMetadata(input);

            fullDestPath = Path.Combine(destDirectory, "DECRYPTED_" + metadata.FileName);

            using var output = new FileStream(fullDestPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536);

            CryptoManager.DecryptAndVerify(input, output, metadata, key);

            success = true;
            Logger.Log($"Decrypted: {metadata.FileName}", metadata);
        }
        catch (Exception ex)
        {
            // brisemo fajl ukoliko ne valja
            if (!success && fullDestPath != null && File.Exists(fullDestPath))
            {
                try { File.Delete(fullDestPath); } catch { }
            }
            throw new Exception($"System file decrypt Error: {ex.Message}");
        }
    }
}

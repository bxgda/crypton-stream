using System;
using System.IO;
using System.Text;
using src.Common;
using src.Core;
using src.Factories;
using src.Interfaces;
using src.StreamWrappers;

namespace src.Services;

public static class CryptoManager
{
    public static FileMetadata PackAndEncrypt(Stream inputStram, Stream outputStream, ICryptoStrategy strategy, string originalFileName)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");

        try
        {
            using var tempStream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                65536, // 64KB buffer
                FileOptions.DeleteOnClose
            );

            var md5 = new MD5_Hasher();

            using (var wrapperStream = new MD5StreamWrapper(tempStream, md5, true))
            {
                strategy.Encrypt(inputStram, wrapperStream);
                wrapperStream.Flush();
            }

            string encryptedHash = md5.FinalizeHash();

            var metadata = new FileMetadata
            {
                FileName = originalFileName,
                FileSize = tempStream.Length,
                Timestamp = DateTime.Now,
                EncryptingAlgorithm = strategy.AlgorithmName,
                Nonce = strategy.InitialNonce,
                HashValue = encryptedHash
            };

            byte[] headerBytes = Encoding.UTF8.GetBytes(metadata.ToJson());
            byte[] lengthBytes = BitConverter.GetBytes(headerBytes.Length);

            outputStream.Write(lengthBytes, 0, lengthBytes.Length);
            outputStream.Write(headerBytes, 0, headerBytes.Length);

            tempStream.Seek(0, SeekOrigin.Begin);
            tempStream.CopyTo(outputStream);

            return metadata;
        }
        catch (Exception ex)
        {
            throw new Exception($"CryptoEngine Error: {ex.Message}", ex);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    public static FileMetadata ReadMetadata(Stream inputStream)
    {
        byte[] lenBytes = new byte[4];
        if (inputStream.Read(lenBytes, 0, 4) < 4)
            throw new Exception("Invalid header length.");

        int headerLength = BitConverter.ToInt32(lenBytes, 0);
        byte[] headerBytes = new byte[headerLength];

        if (inputStream.Read(headerBytes, 0, headerLength) < headerLength)
            throw new Exception("Incomplete metadata header.");

        var metadata = FileMetadata.FromJson(Encoding.UTF8.GetString(headerBytes));

        if (metadata == null)
            throw new Exception("Metadata corruption.");

        return metadata;
    }

    public static void DecryptAndVerify(Stream inputStream, Stream outputStream, FileMetadata metadata, string key)
    {
        ICryptoStrategy strategy = CryptoStrategyFactory.CreateForDecryption(metadata.EncryptingAlgorithm, key, metadata.Nonce);

        var md5 = new MD5_Hasher();

        using (var hashStream = new MD5StreamWrapper(inputStream, md5, true))
        {
            strategy.Decrypt(hashStream, outputStream);
        }

        string calculatedHash = md5.FinalizeHash();

        if (calculatedHash != metadata.HashValue)
        {
            throw new Exception("Security Alert: Hash mismatch! File is corrupted or intercepted.");
        }
    }
}

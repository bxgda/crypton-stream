using System;
using System.IO;
using System.Text;
using src.Common;
using src.Core;

namespace src.Services;

public static class CryptoManager
{
    public static FileMetadata PackAndEncrypt(Stream inputStram, Stream outputStream, string secretWord, EncryptionAlgorithm algorithm, string originalFileName)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");

        try
        {
            using var tempStream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                4096,
                FileOptions.DeleteOnClose
            );

            ushort? nonce = null;

            switch (algorithm)
            {
                case EncryptionAlgorithm.A5_2:
                    var a52 = new A52CtrCipher(secretWord);
                    nonce = a52.InitialNonce;
                    a52.Process(inputStram, tempStream);
                    break;

                case EncryptionAlgorithm.SimpleSubstitution:
                    var sub = new SimpleSubstitution(secretWord);
                    sub.Encrypt(inputStram, tempStream);
                    break;

                default:
                    throw new NotSupportedException($"Encryption algorithm {algorithm} is not supported.");
            }

            tempStream.Seek(0, SeekOrigin.Begin);
            string encryptedHash = MD5_Hasher.CalculateMD5Stream(tempStream);

            var metadata = new FileMetadata
            {
                FileName = originalFileName,
                FileSize = tempStream.Length,
                Timestamp = DateTime.Now,
                EncryptingAlgorithm = algorithm.ToString(),
                Nonce = nonce,
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
    }

    public static FileMetadata UnpackAndDecrypt(Stream inputStram, Stream outputStream, string secretWord)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");

        try
        {
            byte[] lenBytes = new byte[4];

            if (inputStram.Read(lenBytes, 0, 4) < 4)
                throw new Exception("Invalid header length.");

            int headerLength = BitConverter.ToInt32(lenBytes, 0);
            byte[] headerBytes = new byte[headerLength];

            inputStram.Read(headerBytes, 0, headerLength);

            var metadata = FileMetadata.FromJson(Encoding.UTF8.GetString(headerBytes));

            if (metadata == null)
                throw new Exception("Invalid metadata.");

            using var tempStream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                4096,
                FileOptions.DeleteOnClose
            );

            inputStram.CopyTo(tempStream);
            tempStream.Seek(0, SeekOrigin.Begin);

            string currentHash = MD5_Hasher.CalculateMD5Stream(tempStream);
            if (currentHash != metadata.HashValue)
                throw new Exception("Integritet narušen - MD5 Hash mismatch na kriptovanim podacima!");

            tempStream.Seek(0, SeekOrigin.Begin);

            switch (metadata.EncryptingAlgorithm)
            {
                case "A5_2":
                    var a52 = new A52CtrCipher(secretWord, metadata.Nonce);
                    a52.Process(tempStream, outputStream);
                    break;

                case "SimpleSubstitution":
                    var ss = new SimpleSubstitution(secretWord);
                    ss.Decrypt(tempStream, outputStream);
                    break;

                default:
                    throw new NotSupportedException($"Encryption algorithm {metadata.EncryptingAlgorithm} is not supported.");
            }

            return metadata;
        }
        catch (Exception ex)
        {
            throw new Exception($"Decryption Error: {ex.Message}", ex);
        }
    }
}

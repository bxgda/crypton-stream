using System;
using System.IO;
using System.Text;
using src.Core;
using src.Common;

namespace src.FileSystem;

public class FileHandler
{
    private const string EncryptedExtension = ".crypt";

    public byte[] Pack(string sourceFilePath, string secretWord, EncryptionAlgorithm algorithm)
    {
        byte[] originalData = File.ReadAllBytes(sourceFilePath);
        string originalFileName = Path.GetFileName(sourceFilePath);

        byte[] encryptedData;

        // sifrovanje na osnovu izabranog algoritma
        switch (algorithm)
        {
            case EncryptionAlgorithm.SimpleSubstitution:
                var cipher_SS = new SimpleSubstitution(secretWord);
                encryptedData = cipher_SS.Encrypt(originalData);
                break;
            case EncryptionAlgorithm.A5_2:
                var cipher_A5_2 = new A5_2(secretWord);
                encryptedData = cipher_A5_2.Process(originalData);
                break;

            default:
                throw new NotSupportedException($"Encryption algorithm '{algorithm}' is not supported.");
        }

        // generisanje metapodataka
        var metadata = new FileMetadata
        {
            FileName = originalFileName,
            FileSize = originalData.Length,
            Timestamp = DateTime.Now,
            EncryptingAlgorithm = algorithm.ToString(),
            HashValue = MD5_Hasher.CalculateMD5(originalData)
        };

        // pakovanje u niz bajtova
        string jsonHeader = metadata.ToJson();
        byte[] headerBytes = Encoding.UTF8.GetBytes(jsonHeader);
        byte[] headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);

        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(headerLengthBytes, 0, 4);
            memoryStream.Write(headerBytes, 0, headerBytes.Length);
            memoryStream.Write(encryptedData, 0, encryptedData.Length);

            return memoryStream.ToArray();
        }
    }

    public void EncryptAndSave(string sourceFilePath, string destinationFolder, string secretWord, EncryptionAlgorithm algorithm)
    {
        byte[] packedDate = Pack(sourceFilePath, secretWord, algorithm);
        string originalFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        string outputFilePath = Path.Combine(destinationFolder, originalFileName + EncryptedExtension);

        File.WriteAllBytes(outputFilePath, packedDate);
        Console.WriteLine($"Fajl '{originalFileName}' je uspešno šifrovan i sačuvan na putanji: '{outputFilePath}'");
    }

    public (byte[] Data, FileMetadata Metadata) Unpack(byte[] packedData, string secretWord)
    {
        using (var memoryStream = new MemoryStream(packedData))
        {
            // citanje duzine zaglavlja
            byte[] headerLengthBytes = new byte[4];
            memoryStream.Read(headerLengthBytes, 0, 4);
            int headerLength = BitConverter.ToInt32(headerLengthBytes, 0);

            // citanje zaglavlja
            byte[] headerBytes = new byte[headerLength];
            memoryStream.Read(headerBytes, 0, headerLength);
            var metadata = FileMetadata.FromJson(Encoding.UTF8.GetString(headerBytes));

            if (metadata == null)
                throw new Exception("Greška pri čitanju zaglavlja: Metapodaci su nevalidni.");


            // citanje sifrovanih podataka
            byte[] encryptedData = new byte[memoryStream.Length - memoryStream.Position];
            memoryStream.Read(encryptedData, 0, encryptedData.Length);

            if (!Enum.TryParse(metadata.EncryptingAlgorithm, out EncryptionAlgorithm algorithm))
                throw new Exception($"Algoritam {metadata.EncryptingAlgorithm} nije podržan!");

            byte[] decryptedData;
            switch (algorithm)
            {
                case EncryptionAlgorithm.SimpleSubstitution:
                    var cipher_SS = new SimpleSubstitution(secretWord);
                    decryptedData = cipher_SS.Decrypt(encryptedData);
                    break;
                case EncryptionAlgorithm.A5_2:
                    var cipher_A5_2 = new A5_2(secretWord);
                    decryptedData = cipher_A5_2.Process(encryptedData);
                    break;
                default:
                    throw new NotSupportedException($"Encryption algorithm '{algorithm}' is not supported.");
            }

            string currentHash = MD5_Hasher.CalculateMD5(decryptedData);
            if (currentHash != metadata.HashValue)
                throw new Exception("Hash vrednost se ne poklapa! Desifrovanje nije uspelo.");

            return (decryptedData, metadata);
        }
    }

    public void UnpackAndSave(string encryptedFilePath, string destinationFolder, string secretWord)
    {
        if (!File.Exists(encryptedFilePath)) throw new FileNotFoundException(encryptedFilePath);

        byte[] packedData = File.ReadAllBytes(encryptedFilePath);

        var (decryptedData, metadata) = Unpack(packedData, secretWord);

        string outputPath = Path.Combine(destinationFolder, "DECRYPTED_" + metadata.FileName);
        File.WriteAllBytes(outputPath, decryptedData);

        string metaData = Path.GetFileNameWithoutExtension(metadata.FileName);
        string metaDataPath = Path.Combine(destinationFolder, "DECRYPTED_" + metaData + ".json");
        File.WriteAllText(metaDataPath, metadata.ToJson());

        Console.WriteLine($"\n--- USPEH ---");
        Console.WriteLine($"Fajl: {metadata.FileName}");
        Console.WriteLine($"Metadata sačuvan: {Path.GetFileName(metaData)}");
        Console.WriteLine($"Putanja: {destinationFolder}");

        string currentHash = MD5_Hasher.CalculateMD5(decryptedData);
        if (currentHash == metadata.HashValue)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Integritet (MD5): VERIFIKOVAN");
            Console.ResetColor();
        }
    }
}
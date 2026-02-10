using Avalonia;
using System;
using System.IO;
using System.Text;

using src.Core;
using src.Common;
using src.Services;

namespace src;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    // [STAThread]
    // public static void Main(string[] args) => BuildAvaloniaApp()
    //     .StartWithClassicDesktopLifetime(args);

    // // Avalonia configuration, don't remove; also used by visual designer.
    // public static AppBuilder BuildAvaloniaApp()
    //     => AppBuilder.Configure<App>()
    //         .UsePlatformDetect()
    //         .WithInterFont()
    //         .LogToTrace();

    static void Main(string[] args)
    {
        // GetCurrentDirectory() u VS Code-u (dotnet run) je koren projekta
        string projectRoot = Directory.GetCurrentDirectory();

        // Putanje ka folderima - kreiramo ih u root-u projekta
        AppConfig.LogsDirectory = Path.Combine(projectRoot, "logs");
        AppConfig.ReceivedFilesDirectory = Path.Combine(projectRoot, "decrypted_files");
        AppConfig.EnsureDirectoriesExist();

        string encryptedFolder = Path.Combine(projectRoot, "encrypted_files");
        if (!Directory.Exists(encryptedFolder)) Directory.CreateDirectory(encryptedFolder);

        // --- KONFIGURACIJA TESTA ---
        string tajnaSifra = "Sifra123!";
        string file1 = "slika.png";      // Fajl u root-u projekta
        string file2 = "dokument.pdf";   // Fajl u root-u projekta
        // ---------------------------

        Console.WriteLine($"Radni direktorijum: {projectRoot}");
        Console.WriteLine("=== Crypto Manager Test Start ===\n");

        try
        {
            // Test 1: A5/2
            RunTest(projectRoot, file1, encryptedFolder, AppConfig.ReceivedFilesDirectory, tajnaSifra, EncryptionAlgorithm.A5_2);

            Console.WriteLine("\n" + new string('-', 60) + "\n");

            // Test 2: Simple Substitution
            RunTest(projectRoot, file2, encryptedFolder, AppConfig.ReceivedFilesDirectory, tajnaSifra, EncryptionAlgorithm.SimpleSubstitution);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kritična greška: {ex.Message}");
        }

        Console.WriteLine("\nTestiranje završeno. Pritisni bilo koji taster za izlaz...");
    }

    static void RunTest(string root, string fileName, string encDir, string decDir, string key, EncryptionAlgorithm algo)
    {
        string srcPath = Path.Combine(root, fileName);

        // Logika tvoje SystemFile.EncryptFile metode: 
        // skida ekstenziju i dodaje .crypto (npr. slika.png -> slika.crypto)
        string encFileName = Path.GetFileNameWithoutExtension(fileName) + ".crypto";
        string encPath = Path.Combine(encDir, encFileName);

        // Tvoja izmena u SystemFile.DecryptFile: prefiks DECRYPTED_
        string decPath = Path.Combine(decDir, "DECRYPTED_" + fileName);

        if (!File.Exists(srcPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[GRESKA]: Izvor nije pronađen: {srcPath}");
            Console.ResetColor();
            return;
        }

        Console.WriteLine($"[Algoritam: {algo}]");

        try
        {
            // 1. Kriptovanje
            SystemFile.EncryptFile(srcPath, encDir, key, algo);
            Console.WriteLine($" -> Kriptovan: encrypted_files/{encFileName}");

            // 2. Dekriptovanje
            SystemFile.DecryptFile(encPath, decDir, key);
            Console.WriteLine($" -> Dekriptovan: decrypted_files/DECRYPTED_{fileName}");

            // 3. Verifikacija (MD5 poredjenje originala i novog fajla)
            if (File.Exists(decPath))
            {
                using var fs1 = new FileStream(srcPath, FileMode.Open, FileAccess.Read);
                using var fs2 = new FileStream(decPath, FileMode.Open, FileAccess.Read);

                string h1 = MD5_Hasher.CalculateMD5Stream(fs1);
                string h2 = MD5_Hasher.CalculateMD5Stream(fs2);

                if (h1 == h2)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" [OK] MD5 podudaranje! Fajlovi su identični.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" [FAIL] MD5 mismatch! Proveri logiku algoritma.");
                }
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" [GRESKA tokom obrade]: {ex.Message}");
            Console.ResetColor();
        }
    }

}

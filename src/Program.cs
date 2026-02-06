using Avalonia;
using System;
using System.IO;
using System.Text;

using src.Core;
using src.Common;
using src.FileSystem;

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
        Console.WriteLine("=== FINALNI INTEGRACIONI TEST (FileHandler) ===\n");

        // Konfiguracija
        string sourceFile = "../test.png"; // Slika u root-u
        string outputFolder = "../TestOutput";
        string lozinka = "SigurnaLozinka123";
        EncryptionAlgorithm algoritam = EncryptionAlgorithm.A5_2;

        // Priprema foldera
        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        if (!File.Exists(sourceFile))
        {
            Console.WriteLine($"❌ Greška: Nedostaje {sourceFile}");
            return;
        }

        try
        {
            FileHandler handler = new FileHandler();

            // 1. KORAK: Šifrovanje i čuvanje (Koristimo tvoju metodu)
            Console.WriteLine("[KORAK 1] Pokrećem EncryptAndSave...");
            handler.EncryptAndSave(sourceFile, outputFolder, lozinka, algoritam);

            // 2. KORAK: Pronalaženje kreiranog .crypt fajla
            string fileNameOnly = Path.GetFileNameWithoutExtension(sourceFile);
            string encryptedFilePath = Path.Combine(outputFolder, fileNameOnly + ".crypt");

            // 3. KORAK: Dešifrovanje i čuvanje JSON-a (Koristimo tvoju novu metodu)
            Console.WriteLine("\n[KORAK 2] Pokrećem UnpackAndSave...");
            handler.UnpackAndSave(encryptedFilePath, outputFolder, lozinka);

            // 4. KORAK: Provera rezultata na disku
            Console.WriteLine("\n[KORAK 3] Provera fajlova u folderu:");
            string[] files = Directory.GetFiles(outputFolder);
            foreach (var file in files)
            {
                Console.WriteLine($" - Pronađen fajl: {Path.GetFileName(file)} ({new FileInfo(file).Length} bytes)");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Greška tokom izvršavanja: {ex.Message}");
        }

        Console.WriteLine("\n=== Test završen ===");
    }

}

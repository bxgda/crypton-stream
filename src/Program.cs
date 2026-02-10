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
        // 1. Inicijalizacija putanja u korenu projekta
        string projectRoot = Directory.GetCurrentDirectory();
        AppConfig.LogsDirectory = Path.Combine(projectRoot, "logs");
        AppConfig.WatcherTargetDirectory = Path.Combine(projectRoot, "Target");
        AppConfig.WatcherOutputDirectory = Path.Combine(projectRoot, "X");

        AppConfig.EnsureDirectoriesExist();

        string tajnaSifra = "Sifra123!";

        // 2. Kreiranje servisa
        var watcherService = new FileSystemWatcherService(
            AppConfig.WatcherTargetDirectory,
            AppConfig.WatcherOutputDirectory,
            tajnaSifra
        );

        Console.WriteLine("=== FileSystemWatcher (FSW) Test Console ===");
        Console.WriteLine($"Target folder (pratim): {AppConfig.WatcherTargetDirectory}");
        Console.WriteLine($"Output folder (X):      {AppConfig.WatcherOutputDirectory}");
        Console.WriteLine("--------------------------------------------");

        try
        {
            // SIMULACIJA VIEWMODEL-A: Korisnik bira A5/2 i pali FSW
            Console.WriteLine("\n[VM Simulacija]: Korisnik bira A5_2 i aktivira FSW...");
            watcherService.Start(EncryptionAlgorithm.A5_2);

            Console.WriteLine("\n>>> TEST 1: Ubaci neki fajl u 'Target' folder...");
            Console.WriteLine(">>> (Čekam detekciju... Pritisni bilo koji taster za promenu algoritma)");
            Console.ReadKey(true);

            // SIMULACIJA VIEWMODEL-A: Korisnik menja algoritam na SimpleSubstitution
            Console.WriteLine("\n[VM Simulacija]: Korisnik menja algoritam na SimpleSubstitution...");
            watcherService.Start(EncryptionAlgorithm.SimpleSubstitution);

            Console.WriteLine("\n>>> TEST 2: Ubaci NOVI fajl u 'Target' folder...");
            Console.WriteLine(">>> (Sada će biti korišćen SimpleSubstitution. Pritisni 'Q' za kraj)");

            // Drži program budnim dok ne pritisneš Q
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q) break;
            }

            watcherService.Stop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kritična greška: {ex.Message}");
        }

        Console.WriteLine("\n=== Program završen. Proveri logove u 'logs' folderu. ===");
    }

}

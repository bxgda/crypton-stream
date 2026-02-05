using Avalonia;
using System;
using System.IO;

using System.Text;
using src.Core;

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
        Console.WriteLine("=== MD5 Integrity Test (Simple Substitution) ===");

        string putanjaDoSlike = "../poster.pdf";
        string tajnaRec = "MojaLozinka123";

        if (!File.Exists(putanjaDoSlike))
        {
            Console.WriteLine("❌ Greška: Prvo ubaci 'test.jpg' u koren projekta!");
            return;
        }

        // 1. Učitaj originalni fajl
        byte[] originalniBajtovi = File.ReadAllBytes(putanjaDoSlike);

        // 2. Izračunaj MD5 originala
        string hashOriginala = MD5_Hasher.CalculateMD5(originalniBajtovi);
        Console.WriteLine($"[1] MD5 Originala:    {hashOriginala}");

        // 3. Šifruj pomoću SimpleSubstitution
        SimpleSubstitution cipher = new SimpleSubstitution(tajnaRec);
        byte[] sifrovaniBajtovi = cipher.Encrypt(originalniBajtovi);

        // Opciono: Izračunaj hash šifrovanog fajla (čisto da vidiš da je drugačiji)
        string hashSifrata = MD5_Hasher.CalculateMD5(sifrovaniBajtovi);
        Console.WriteLine($"[2] MD5 Šifrata:     {hashSifrata} (Mora biti drugačiji)");

        // 4. Dešifruj
        byte[] desifrovaniBajtovi = cipher.Decrypt(sifrovaniBajtovi);

        // 5. Izračunaj MD5 dešifrovanog fajla
        string hashDesifrovanog = MD5_Hasher.CalculateMD5(desifrovaniBajtovi);
        Console.WriteLine($"[3] MD5 Dešifrovanog: {hashDesifrovanog}");

        // 6. Finalna provera
        Console.WriteLine("\n------------------------------------------------");
        if (hashOriginala == hashDesifrovanog)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ USPEH: Heševi su identični! Integritet je očuvan.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ GREŠKA: Heševi se ne poklapaju! Algoritam menja podatke.");
        }
        Console.ResetColor();
        Console.WriteLine("------------------------------------------------");
    }

}

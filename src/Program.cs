using Avalonia;
using System;

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
        Console.WriteLine("--- Testiranje Simple Substitution Algoritma ---");

        // 1. Test sa tajnom rečju (tvoja ideja sa pomerajem)
        string tajnaRec = "ELFAK";
        var cipherRec = new SimpleSubstitution(tajnaRec);

        string originalniTekst = "Pozdrav sa elektronskog!";
        byte[] podaci = Encoding.UTF8.GetBytes(originalniTekst);

        Console.WriteLine($"\nOriginalni tekst: {originalniTekst}");

        // Šifrovanje
        byte[] sifrovano = cipherRec.Encrypt(podaci);
        Console.WriteLine($"Šifrovano (bajtovi): {BitConverter.ToString(sifrovano)}");

        // Dešifrovanje
        byte[] desifrovano = cipherRec.Decrypt(sifrovano);
        string rezultat = Encoding.UTF8.GetString(desifrovano);
        Console.WriteLine($"Dešifrovano tekst: {rezultat}");

        // Provera
        if (originalniTekst == rezultat)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("USPEH: Tekstovi se podudaraju!");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("GREŠKA: Tekstovi nisu isti!");
            Console.ResetColor();
        }

        Console.WriteLine("\n------------------------------------------------");
    }

}

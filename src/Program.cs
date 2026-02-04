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
        string tajnaRec = "elfak2026";

        // --- TEST 1: STRING ---
        Console.WriteLine("=== Test 1: String šifrovanje ===");
        string originalniTekst = "Ovo je tajna poruka za A5/2!";
        byte[] tekstBajtovi = Encoding.UTF8.GetBytes(originalniTekst);

        A5_2 cipherString = new A5_2(tajnaRec);
        byte[] sifrovaniTekst = cipherString.Process(tekstBajtovi);

        A5_2 decipherString = new A5_2(tajnaRec); // Resetujemo registre za dešifrovanje
        byte[] desifrovaniTekstBajtovi = decipherString.Process(sifrovaniTekst);
        string konacniTekst = Encoding.UTF8.GetString(desifrovaniTekstBajtovi);

        Console.WriteLine($"Original: {originalniTekst}");
        Console.WriteLine($"Rezultat: {konacniTekst}");
        Console.WriteLine(originalniTekst == konacniTekst ? "✅ STRING USPEŠNO DEŠIFROVAN" : "❌ GREŠKA U STRINGU");


        // --- TEST 2: FAJL (SLIKA) ---
        Console.WriteLine("\n=== Test 2: Fajl (slika) šifrovanje ===");

        // Putanja je dva nivoa iznad 'bin/Debug/net10.0' ako pokrećeš sa dotnet run
        // Ili samo stavi apsolutnu putanju radi testa
        string putanjaDoSlike = "../test.png";
        string putanjaSifrovana = "../test_sifrovano.bin";
        string putanjaDesifrovana = "../test_povraceno.png";

        if (!File.Exists(putanjaDoSlike))
        {
            Console.WriteLine($"❌ Greška: Fajl {putanjaDoSlike} nije pronađen u korenu projekta!");
            return;
        }

        // 1. Učitaj bajtove slike
        byte[] originalnaSlika = File.ReadAllBytes(putanjaDoSlike);
        Console.WriteLine($"Fajl učitan. Veličina: {originalnaSlika.Length} bajtova.");

        // 2. Šifruj
        A5_2 cipherFile = new A5_2(tajnaRec);
        byte[] sifrovanaSlika = cipherFile.Process(originalnaSlika);
        File.WriteAllBytes(putanjaSifrovana, sifrovanaSlika);
        Console.WriteLine("Fajl šifrovan i sačuvan kao 'test_sifrovano.bin'.");

        // 3. Dešifruj
        A5_2 decipherFile = new A5_2(tajnaRec);
        byte[] desifrovanaSlika = decipherFile.Process(sifrovanaSlika);
        File.WriteAllBytes(putanjaDesifrovana, desifrovanaSlika);
        Console.WriteLine("Fajl dešifrovan i sačuvan kao 'test_povraceno.jpg'.");

        // 4. Provera integriteta
        bool uspeh = true;
        for (int i = 0; i < originalnaSlika.Length; i++)
        {
            if (originalnaSlika[i] != desifrovanaSlika[i])
            {
                uspeh = false;
                break;
            }
        }
        Console.WriteLine(uspeh ? "✅ SLIKA IDENTIČNA ORIGINALU!" : "❌ SLIKA JE OŠTEĆENA!");
    }

}

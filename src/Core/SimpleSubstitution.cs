using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace src.Core;

public class SimpleSubstitution
{
    private readonly byte[] _key;
    private readonly byte[] _inverseKey;
    private const int SegmentSize = 4096;   // 4KB

    public SimpleSubstitution(string secretWord)
    {
        if (string.IsNullOrEmpty(secretWord))
            throw new ArgumentException("secretWord must not be null or empty");

        _key = GenerateKeyFromSecretWord(secretWord);
        _inverseKey = GenerateInverseKey(_key);
    }

    public byte[] GenerateKeyFromSecretWord(string secretWord)
    {
        byte[] key = new byte[256];

        int sum = secretWord.Sum(c => (int)c);

        for (int i = 0; i < 256; i++)
            key[i] = (byte)((i + sum) % 256);

        return key;
    }

    public byte[] GenerateInverseKey(byte[] key)
    {
        byte[] inverseKey = new byte[256];

        for (int i = 0; i < 256; i++)
            inverseKey[key[i]] = (byte)i;

        return inverseKey;
    }

    public void Encrypt(Stream input, Stream output) => ProcessStream(input, output, _key);


    public void Decrypt(Stream input, Stream output) => ProcessStream(input, output, _inverseKey);

    private void ProcessStream(Stream input, Stream output, byte[] table)
    {
        int maxParallelism = Environment.ProcessorCount;
        int batchSegments = 1024;          // procesiramo 1024 segmenata po paralelnom pozivu (optimalno za balans izmedju paralelizma i kesa)
        int batchBufferSize = batchSegments * SegmentSize;  // ukupna veličina bafera za čitanje i obradu u jednom paralelnom pozivu 

        byte[] buffer = new byte[batchBufferSize];
        byte[] resultBuffer = new byte[batchBufferSize];

        int bytesRead;

        while (true)
        {
            bytesRead = ReadStream(input, buffer, batchBufferSize);

            if (bytesRead == 0)
                break;

            int actualSegments = (bytesRead + SegmentSize - 1) / SegmentSize;
            int totalBytes = bytesRead;

            Parallel.For(0, actualSegments, new ParallelOptions { MaxDegreeOfParallelism = maxParallelism },
                segmentIndex =>
                {
                    int offset = segmentIndex * SegmentSize;
                    int length = Math.Min(SegmentSize, totalBytes - offset);

                    for (int i = 0; i < length; i++)
                        resultBuffer[offset + i] = table[buffer[offset + i]];
                });

            output.Write(resultBuffer, 0, bytesRead);
        }
    }

    private static int ReadStream(Stream stream, byte[] buffer, int count)
    {
        int totalRead = 0;

        while (totalRead < count)
        {
            int bytesRead = stream.Read(buffer, totalRead, count - totalRead);

            if (bytesRead == 0)
                break;

            totalRead += bytesRead;
        }

        return totalRead;
    }
}

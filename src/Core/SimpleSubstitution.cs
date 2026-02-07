using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;

namespace src.Core;

public class SimpleSubstitution
{
    private readonly byte[] _key;
    private readonly byte[] _inverseKey;
    private const int SegmentSize = 1024;   // 1KB

    public SimpleSubstitution(byte[] key)
    {
        if (key.Length != 256)
            throw new ArgumentException("key must be 256 bytes long");

        _key = key;
        _inverseKey = new byte[256];

        for (int i = 0; i < 256; i++)
            _inverseKey[_key[i]] = (byte)i;
    }

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

    public static byte[] GenerateRandomKey()
    {
        byte[] key = new byte[256];

        for (int i = 0; i < 256; i++)
            key[i] = (byte)i;

        Random rnd = new Random();

        int n = key.Length;

        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            byte value = key[k];
            key[k] = key[n];
            key[n] = value;
        }

        return key;
    }

    public byte[] Encrypt(byte[] data)
    {
        byte[] result = new byte[data.Length];

        int totalSegments = (data.Length + SegmentSize - 1) / SegmentSize;

        Parallel.For(0, totalSegments, segmentIndex =>
        {
            int offset = segmentIndex * SegmentSize;
            int length = Math.Min(SegmentSize, data.Length - offset);

            for (int i = 0; i < length; i++)
                result[offset + i] = _key[data[offset + i]];
        });

        return result;
    }

    public byte[] Decrypt(byte[] data)
    {
        byte[] result = new byte[data.Length];

        int totalSegments = (data.Length + SegmentSize - 1) / SegmentSize;

        Parallel.For(0, totalSegments, segmentIndex =>
        {
            int offset = segmentIndex * SegmentSize;
            int length = Math.Min(SegmentSize, data.Length - offset);

            for (int i = 0; i < length; i++)
                result[offset + i] = _inverseKey[data[offset + i]];
        });

        return result;
    }

    public void EncryptStream(Stream input, Stream output)
    {
        ProcessStream(input, output, _key);
    }

    public void DecryptStream(Stream input, Stream output)
    {
        ProcessStream(input, output, _inverseKey);
    }

    private void ProcessStream(Stream input, Stream output, byte[] table)
    {
        int maxParallelism = Environment.ProcessorCount;
        int batchSegments = maxParallelism * 64;
        int batchBufferSize = batchSegments * SegmentSize;

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

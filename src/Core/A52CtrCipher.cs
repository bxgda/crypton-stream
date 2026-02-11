using System;
using System.IO;
using System.Threading.Tasks;
using src.Interfaces;

namespace src.Core;

public class A52CtrCipher : ICryptoStrategy
{
    public const int SegmentSize = 256 * 1024; // 256KB po segmentu

    private readonly ulong _key;
    private readonly ushort _initialNonce;
    private readonly int _maxDegreeOfParallelism;

    public A52CtrCipher(string secretWord, ushort? nonce = null)
    {
        _key = DeriveKey(secretWord);

        _maxDegreeOfParallelism = Environment.ProcessorCount;

        if (nonce != null)
            _initialNonce = (ushort)nonce;
        else
            _initialNonce = new CtrMode().InitialNonce;
    }

    public void Encrypt(Stream input, Stream output) => Process(input, output);

    public void Decrypt(Stream input, Stream output) => Process(input, output);

    public string AlgorithmName => "A5_2";

    public ushort? InitialNonce => _initialNonce;

    private void ProcessSegment(byte[] input, byte[] result, int localSegmentIndex, int globalSegmentIndex)
    {
        // offset u odnosu na pocetak trenutnog bafera
        int offset = localSegmentIndex * SegmentSize;
        int segmentLength = Math.Min(SegmentSize, input.Length - offset);

        if (segmentLength <= 0) return;

        var ctr = new CtrMode(_initialNonce);
        ctr.SeekTo(globalSegmentIndex);             // sek na globalni segment koji se trenutno procesira
        uint frameNumber = ctr.NextFrameNumber();

        var a52 = new A5_2(_key, frameNumber);
        a52.GenerateKeystream(segmentLength, offset, result);

        for (int i = 0; i < segmentLength; i++)
            result[offset + i] ^= input[offset + i];
    }

    public void Process(Stream input, Stream output)
    {
        // int batchSegments = 4096; // 16MB po segmentu, i tacno se uklapa da jedan poziv paralelne obrade bude 16MB odnosno jedan nonce
        int batchSegments = Environment.ProcessorCount * 4;
        int batchBufferSize = batchSegments * SegmentSize;

        byte[] buffer = new byte[batchBufferSize];
        byte[] resultBuffer = new byte[batchBufferSize];

        int bytesRead;
        int totalSegmentsProcessed = 0;

        while (true)
        {
            // bytesRead = input.Read(buffer, 0, batchBufferSize);
            bytesRead = ReadFullStream(input, buffer, batchBufferSize);

            if (bytesRead == 0)
                break;

            int actualSegmentsInBatch = (bytesRead + SegmentSize - 1) / SegmentSize;

            Parallel.For(0, actualSegmentsInBatch, new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
            localSegmentIndex =>
            {
                ProcessSegment(buffer, resultBuffer, localSegmentIndex, totalSegmentsProcessed + localSegmentIndex);
            });

            output.Write(resultBuffer, 0, bytesRead);
            totalSegmentsProcessed += actualSegmentsInBatch;
        }
    }

    private static ulong DeriveKey(string secretWord)
    {
        ulong key = 0;

        foreach (char c in secretWord)
            key = (key << 5) ^ (key >> 3) ^ (uint)c;

        key ^= key >> 33;
        key *= 0xFF51AFD7ED558CCD;
        key ^= key >> 33;
        key *= 0xC4CEB9FE1A85EC53;
        key ^= key >> 33;

        return key;
    }

    private int ReadFullStream(Stream stream, byte[] buffer, int count)
    {
        int totalRead = 0;
        int read;
        while (count > 0 && (read = stream.Read(buffer, totalRead, count)) > 0)
        {
            totalRead += read;
            count -= read;
        }
        return totalRead;
    }
}

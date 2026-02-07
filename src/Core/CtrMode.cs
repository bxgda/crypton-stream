using System;
using System.Security.Cryptography;

namespace src.Core;

public class CtrMode
{
    private readonly ushort initialNonce;
    private ushort currentNonce;
    private uint counter;
    private uint nonceGeneration;

    public const int MaxCounter = 4096;
    public const int NonceBits = 10;
    public const int CounerBits = 12;

    public CtrMode()
    {
        initialNonce = GenerateRandomNonce();
        currentNonce = initialNonce;
        counter = 0;
        nonceGeneration = 0;
    }

    public CtrMode(ushort initialNonce)
    {
        this.initialNonce = initialNonce;
        currentNonce = initialNonce;
        counter = 0;
        nonceGeneration = 0;
    }

    public ushort InitialNonce => initialNonce;

    public uint NextFrameNumber()
    {
        if (counter >= MaxCounter)
        {
            nonceGeneration++;
            currentNonce = DeriveNextNonce(initialNonce, nonceGeneration);
            counter = 0;
        }

        uint frameNumber = ((uint)currentNonce << CounerBits) | (counter & 0xFFF);
        counter++;

        return frameNumber;
    }

    public void SeekTo(long segmentIndex)
    {
        nonceGeneration = (uint)(segmentIndex / MaxCounter);
        counter = (uint)(segmentIndex % MaxCounter);

        if (nonceGeneration == 0)
            currentNonce = initialNonce;
        else
            currentNonce = DeriveNextNonce(initialNonce, nonceGeneration);
    }

    private static ushort DeriveNextNonce(ushort initialNonce, uint generation)
    {
        uint combined = ((uint)initialNonce << 16) | (generation & 0xFFFF);

        uint hash = 2166136261;
        hash ^= combined & 0xFF;
        hash *= 16777619;
        hash ^= (combined >> 8) & 0xFF;
        hash *= 16777619;
        hash ^= (combined >> 16) & 0xFF;
        hash *= 16777619;
        hash ^= (combined >> 24) & 0xFF;
        hash *= 16777619;

        return (ushort)(hash & 0x3FF); // 10 bita
    }

    private static ushort GenerateRandomNonce()
    {
        byte[] bytes = new byte[2];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return (ushort)(BitConverter.ToUInt16(bytes, 0) & 0x3FF);   // 10 bita
    }
}

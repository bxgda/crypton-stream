using System;
using System.Linq;

namespace src.Core;

public class A5_2
{
    private uint _r1, _r2, _r3, _r4;

    private const uint Mask1 = 0x7FFFF;     // 19 bita
    private const uint Mask2 = 0x3FFFFF;    // 22 bita
    private const uint Mask3 = 0x7FFFFF;    // 23 bita
    private const uint Mask4 = 0x1FFFF;     // 17 bita

    public A5_2(string secretWord)
    {
        InitializeRegisters(secretWord);
    }

    private void InitializeRegisters(string secretWord)
    {
        ulong key = 0;
        foreach (char c in secretWord)
            key = (key << 5) ^ (key >> 3) ^ (uint)c;

        _r1 = (uint)(key & Mask1);
        _r2 = (uint)((key >> 19) & Mask2);
        _r3 = (uint)((key >> 41) & Mask3);
        _r4 = (uint)((key >> 10) & Mask4);

        // osiguravamo da nijedan registar nije nula
        if (_r1 == 0) _r1 = 0x12345;
        if (_r2 == 0) _r2 = 0x23456;
        if (_r3 == 0) _r3 = 0x34567;
        if (_r4 == 0) _r4 = 0x45678;
    }

    private uint Clock(ref uint reg, uint mask, uint taps)
    {
        uint feedback = BitCount(reg & taps) % 2;

        uint outBit = (reg >> (int)GetMsbPos(mask)) & 1;

        reg = ((reg << 1) | feedback) & mask;

        return outBit;
    }

    private uint BitCount(uint n)
    {
        uint count = 0;

        while (n > 0)
        {
            count += n & 1;
            n >>= 1;
        }

        return count;
    }

    private int GetMsbPos(uint mask)
    {
        if (mask == Mask1)
            return 18;
        else if (mask == Mask2)
            return 21;
        else if (mask == Mask3)
            return 22;
        else
            return 16;
    }

    private byte GenerateByte()
    {
        byte result = 0;

        for (int i = 0; i < 8; i++)
        {
            uint p1 = (_r4 >> 10) & 1;
            uint p2 = (_r4 >> 3) & 1;
            uint p3 = (_r4 >> 7) & 1;

            uint majority = (p1 & p2) | (p1 & p3) | (p2 & p3);

            if (p1 == majority)
                Clock(ref _r1, Mask1, 0x072000);    // taps za R1: 18,17,16,13
            if (p2 == majority)
                Clock(ref _r2, Mask2, 0x000300);    // taps za R2: 21,20
            if (p3 == majority)
                Clock(ref _r3, Mask3, 0x700080);    // taps za R3: 22,21,20, 7

            Clock(ref _r4, Mask4, 0x010800);        // taps za R4: 16,11

            uint outBit = (_r1 >> 18) ^ (_r2 >> 21) ^ (_r3 >> 22);
            result = (byte)((result << 1) | outBit & 1);
        }

        return result;
    }

    public byte[] Process(byte[] data)
    {
        byte[] result = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
            result[i] = (byte)(data[i] ^ GenerateByte());

        return result;
    }
}

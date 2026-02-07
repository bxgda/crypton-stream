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

    private const uint Taps1 = 0x072000;    // R1: 18,17,16,13
    private const uint Taps2 = 0x300000;    // R2: 21,20
    private const uint Taps3 = 0x700080;    // R3: 22,21,20,7
    private const uint Taps4 = 0x010800;    // R4: 16,11

    private const int R4ClockBit1 = 10;
    private const int R4ClockBit2 = 3;
    private const int R4ClockBit3 = 7;

    public A5_2(ulong key, uint frameNumber)
    {
        InitializeRegisters(key, frameNumber);
    }

    private void InitializeRegisters(ulong key, uint frameNumber)
    {
        _r1 = _r2 = _r3 = _r4 = 0;

        // ucitavanje kljuva
        for (int i = 0; i < 64; i++)
        {
            uint keyBit = (uint)((key >> i) & 1);
            ClockAllRegisters();
            _r1 ^= keyBit;
            _r2 ^= keyBit;
            _r3 ^= keyBit;
            _r4 ^= keyBit;
        }

        // ucitavanje framenumber-a
        for (int i = 0; i < 22; i++)
        {
            uint frameBit = (frameNumber >> i) & 1;
            ClockAllRegisters();
            _r1 ^= frameBit;
            _r2 ^= frameBit;
            _r3 ^= frameBit;
            _r4 ^= frameBit;
        }

        // osiguravamo se da nijedan registar nije nula
        if (_r1 == 0) _r1 = 0x12345;
        if (_r2 == 0) _r2 = 0x23456;
        if (_r3 == 0) _r3 = 0x34567;
        if (_r4 == 0) _r4 = 0x45678;

        // 99 iteracija koje se odbacuju
        for (int i = 0; i < 99; i++)
        {
            ClockWithControl();
            GetOutputBit();
        }
    }

    private void ClockRegister(ref uint reg, uint mask, uint taps)
    {
        uint feedback = Parity(reg & taps);
        reg = ((reg << 1) | feedback) & mask;
    }

    private void ClockAllRegisters()
    {
        ClockRegister(ref _r1, Mask1, Taps1);
        ClockRegister(ref _r2, Mask2, Taps2);
        ClockRegister(ref _r3, Mask3, Taps3);
        ClockRegister(ref _r4, Mask4, Taps4);
    }

    private void ClockWithControl()
    {
        uint p1 = (_r4 >> R4ClockBit1) & 1;
        uint p2 = (_r4 >> R4ClockBit2) & 1;
        uint p3 = (_r4 >> R4ClockBit3) & 1;

        uint majority = (p1 & p2) | (p1 & p3) | (p2 & p3);

        if (p1 == majority)
            ClockRegister(ref _r1, Mask1, Taps1);
        if (p2 == majority)
            ClockRegister(ref _r2, Mask2, Taps2);
        if (p3 == majority)
            ClockRegister(ref _r3, Mask3, Taps3);

        ClockRegister(ref _r4, Mask4, Taps4);
    }

    private uint GetOutputBit()
    {
        return ((_r1 >> 18) ^ (_r2 >> 21) ^ (_r3 >> 22)) & 1;
    }

    private static uint Parity(uint n)
    {
        n ^= n >> 16;
        n ^= n >> 8;
        n ^= n >> 4;
        n ^= n >> 2;
        n ^= n >> 1;
        return n & 1;
    }

    public byte[] GenerateKeystream(int length)
    {
        byte[] keystream = new byte[length];

        for (int i = 0; i < length; i++)
        {
            byte b = 0;
            for (int bit = 0; bit < 8; bit++)
            {
                ClockWithControl();
                uint outBit = GetOutputBit();
                b = (byte)((b << 1) | outBit);
            }
            keystream[i] = b;
        }

        return keystream;
    }
}

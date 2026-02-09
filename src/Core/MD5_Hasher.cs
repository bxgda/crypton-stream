using System;
using System.IO;
using System.Linq;

namespace src.Core;

// implementirano pomocu https://gist.github.com/ameerkat/07a748c9b571289711ebaf61f4b596e9

// i dodatno implementirana podrska za stream "u letu"

public static class MD5_Hasher
{
    static int[] s = new int[64] {
        7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7,
        12, 17, 22, 5, 9, 14, 20, 5, 9, 14, 20, 5, 9, 14,
        20, 5, 9, 14, 20, 4, 11, 16, 23, 4, 11, 16, 23, 4,
        11, 16, 23, 4, 11, 16, 23, 6, 10, 15, 21, 6, 10, 15,
        21, 6, 10, 15, 21, 6, 10, 15, 21
    };

    static uint[] K = new uint[64] {
        0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
        0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
        0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
        0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
        0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
        0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
        0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
        0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
        0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
        0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
        0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
        0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
        0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
        0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
        0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
        0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
    };

    public class MD5Context
    {
        public uint A = 0x67452301;
        public uint B = 0xefcdab89;
        public uint C = 0x98badcfe;
        public uint D = 0x10325476;
        public byte[] Buffer = new byte[64];
        public int BufferLen = 0;
        public long TotalLen = 0;
    }

    public static void Update(MD5Context ctx, byte[] input, int length)
    {
        ctx.TotalLen += length;
        int offset = 0;

        // ako vec imamo nesto u baferu, dopunimo ga do 64
        if (ctx.BufferLen > 0)
        {
            int toCopy = Math.Min(length, 64 - ctx.BufferLen);
            Array.Copy(input, 0, ctx.Buffer, ctx.BufferLen, toCopy);
            ctx.BufferLen += toCopy;
            offset = toCopy;

            if (ctx.BufferLen == 64)
            {
                ProcessBlock(ctx.Buffer, ref ctx.A, ref ctx.B, ref ctx.C, ref ctx.D);
                ctx.BufferLen = 0;
            }
        }

        // procesiramo pune blokove od 64 bajta direktno iz inputa
        while (length - offset >= 64)
        {
            byte[] block = new byte[64];
            Array.Copy(input, offset, block, 0, 64);
            ProcessBlock(block, ref ctx.A, ref ctx.B, ref ctx.C, ref ctx.D);
            offset += 64;
        }

        // ostatak bacamo u bafer
        if (offset < length)
        {
            ctx.BufferLen = length - offset;
            Array.Copy(input, offset, ctx.Buffer, 0, ctx.BufferLen);
        }
    }

    public static string Finalize(MD5Context ctx)
    {
        long bitLength = ctx.TotalLen * 8;
        byte[] lastBlock = new byte[64];
        Array.Copy(ctx.Buffer, lastBlock, ctx.BufferLen);
        lastBlock[ctx.BufferLen] = 0x80;

        if (ctx.BufferLen <= 55)
        {
            Array.Copy(BitConverter.GetBytes(bitLength), 0, lastBlock, 56, 8);
            ProcessBlock(lastBlock, ref ctx.A, ref ctx.B, ref ctx.C, ref ctx.D);
        }
        else
        {
            ProcessBlock(lastBlock, ref ctx.A, ref ctx.B, ref ctx.C, ref ctx.D);
            byte[] finalBlock = new byte[64];
            Array.Copy(BitConverter.GetBytes(bitLength), 0, finalBlock, 56, 8);
            ProcessBlock(finalBlock, ref ctx.A, ref ctx.B, ref ctx.C, ref ctx.D);
        }

        return GetByteString(ctx.A) + GetByteString(ctx.B) + GetByteString(ctx.C) + GetByteString(ctx.D);
    }

    public static string CalculateMD5Stream(Stream input)
    {
        var ctx = new MD5Context();
        byte[] buffer = new byte[4096];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            Update(ctx, buffer, read);
        return Finalize(ctx);
    }

    private static void ProcessBlock(byte[] block, ref uint a0, ref uint b0, ref uint c0, ref uint d0)
    {
        uint[] M = new uint[16];
        for (int j = 0; j < 16; j++) M[j] = BitConverter.ToUInt32(block, j * 4);
        uint A = a0, B = b0, C = c0, D = d0, F = 0, g = 0;
        for (uint k = 0; k < 64; k++)
        {
            if (k <= 15) { F = (B & C) | (~B & D); g = k; }
            else if (k <= 31) { F = (D & B) | (~D & C); g = ((5 * k) + 1) % 16; }
            else if (k <= 47) { F = B ^ C ^ D; g = ((3 * k) + 5) % 16; }
            else { F = C ^ (B | ~D); g = (7 * k) % 16; }
            var dtemp = D; D = C; C = B;
            B = B + leftRotate((A + F + K[k] + M[g]), s[k]);
            A = dtemp;
        }
        a0 += A; b0 += B; c0 += C; d0 += D;
    }

    private static uint leftRotate(uint x, int c) => (x << c) | (x >> (32 - c));
    private static string GetByteString(uint x) => String.Join("", BitConverter.GetBytes(x).Select(y => y.ToString("x2")));
}

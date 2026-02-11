using System;

// implementirano pomocu https://gist.github.com/ameerkat/07a748c9b571289711ebaf61f4b596e9
// i dodatno implementirana podrska za streamove i hashovanje velikih fajlova bez ucitavanja u memoriju
// odnosno to znaci da racunamo md5 u letu a ne odjednom

namespace src.Core;

public class MD5_Hasher
{
    private static readonly int[] s = new int[64] {
        7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,
        5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,
        4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,
        6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21
    };

    private static readonly uint[] K = new uint[64] {
        0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
        0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
        0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
        0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
        0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
        0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
        0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
        0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
    };

    private uint _a, _b, _c, _d;
    private long _totalBytes;
    private byte[] _buffer;
    private int _bufferIdx;

    public MD5_Hasher() { Initialize(); }

    public void Initialize()
    {
        _a = 0x67452301; _b = 0xefcdab89; _c = 0x98badcfe; _d = 0x10325476;
        _totalBytes = 0;
        _buffer = new byte[64];
        _bufferIdx = 0;
    }

    public void Update(byte[] input, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            _buffer[_bufferIdx++] = input[offset + i];
            _totalBytes++;
            if (_bufferIdx == 64) { ProcessBlock(_buffer); _bufferIdx = 0; }
        }
    }

    public string FinalizeHash()
    {
        long bitLength = _totalBytes * 8;
        if (_bufferIdx < 64) _buffer[_bufferIdx++] = 0x80;
        else { ProcessBlock(_buffer); _bufferIdx = 0; _buffer[_bufferIdx++] = 0x80; }

        if (_bufferIdx > 56) { while (_bufferIdx < 64) _buffer[_bufferIdx++] = 0; ProcessBlock(_buffer); _bufferIdx = 0; }
        while (_bufferIdx < 56) _buffer[_bufferIdx++] = 0;

        byte[] lengthBytes = BitConverter.GetBytes(bitLength);
        Array.Copy(lengthBytes, 0, _buffer, 56, 8);
        ProcessBlock(_buffer);

        return GetByteString(_a) + GetByteString(_b) + GetByteString(_c) + GetByteString(_d);
    }

    private void ProcessBlock(byte[] block)
    {
        uint[] M = new uint[16];
        for (int j = 0; j < 16; j++) M[j] = BitConverter.ToUInt32(block, j * 4);
        uint A = _a, B = _b, C = _c, D = _d, F = 0, g = 0;

        for (uint k = 0; k < 64; ++k)
        {
            if (k <= 15) { F = (B & C) | (~B & D); g = k; }
            else if (k <= 31) { F = (D & B) | (~D & C); g = ((5 * k) + 1) % 16; }
            else if (k <= 47) { F = B ^ C ^ D; g = ((3 * k) + 5) % 16; }
            else { F = C ^ (B | ~D); g = (7 * k) % 16; }
            var dtemp = D; D = C; C = B; B = B + LeftRotate((A + F + K[k] + M[g]), s[k]); A = dtemp;
        }
        _a += A; _b += B; _c += C; _d += D;
    }

    private static uint LeftRotate(uint x, int c) => (x << c) | (x >> (32 - c));
    private static string GetByteString(uint x)
    {
        var bytes = BitConverter.GetBytes(x);
        return $"{bytes[0]:x2}{bytes[1]:x2}{bytes[2]:x2}{bytes[3]:x2}";
    }

    // ako zatreba jos negde mozda staticki
    public static string CalculateMD5Stream(System.IO.Stream stream)
    {
        var md5 = new MD5_Hasher();
        byte[] buf = new byte[8192];
        int read;
        while ((read = stream.Read(buf, 0, buf.Length)) > 0) md5.Update(buf, 0, read);
        return md5.FinalizeHash();
    }
}

using System;
using System.Linq;

namespace src.Core;

public class SimpleSubstitution
{
    private readonly byte[] _key;
    private readonly byte[] _inverseKey;

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

        for (int i = 0; i < data.Length; i++)
            result[i] = _key[data[i]];

        return result;
    }

    public byte[] Decrypt(byte[] data)
    {
        byte[] result = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
            result[i] = _inverseKey[data[i]];

        return result;
    }
}

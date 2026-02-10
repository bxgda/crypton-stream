using System;
using src.Common;
using src.Core;
using src.Interfaces;

namespace src.Factories;

public static class CryptoStrategyFactory
{
    public static ICryptoStrategy CreateForEncryption(EncryptionAlgorithm algorithm, string secretWord)
    {
        switch (algorithm)
        {
            case EncryptionAlgorithm.A5_2:
                return new A52CtrCipher(secretWord);

            case EncryptionAlgorithm.SimpleSubstitution:
                return new SimpleSubstitution(secretWord);

            default:
                throw new NotSupportedException("Not supported algorithm: " + algorithm);
        }
    }

    public static ICryptoStrategy CreateForDecryption(string algorithmName, string secretWord, ushort? nonce)
    {
        EncryptionAlgorithm algorithm;
        bool uspeh = Enum.TryParse(algorithmName, out algorithm);

        if (!uspeh)
            throw new NotSupportedException("Unknown algorithm in metadata: " + algorithmName);

        switch (algorithm)
        {
            case EncryptionAlgorithm.A5_2:
                return new A52CtrCipher(secretWord, nonce);

            case EncryptionAlgorithm.SimpleSubstitution:
                return new SimpleSubstitution(secretWord);

            default:
                throw new NotSupportedException("Not supported algorithm: " + algorithm);
        }
    }
}
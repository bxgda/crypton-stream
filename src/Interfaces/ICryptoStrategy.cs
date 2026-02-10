using System.IO;

namespace src.Interfaces;

public interface ICryptoStrategy
{
    void Encrypt(Stream input, Stream output);

    void Decrypt(Stream input, Stream output);

    string AlgorithmName { get; }

    ushort? InitialNonce { get; }
}
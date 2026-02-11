using System.IO;
using src.Core;

namespace src.StreamWrappers;

public class MD5StreamWrapper : Stream
{
    private readonly Stream _baseStream;
    private readonly MD5_Hasher _md5;

    public MD5StreamWrapper(Stream baseStream, MD5_Hasher md5)
    {
        _baseStream = baseStream;
        _md5 = md5;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        // citamo podatke sa streama
        int bytesRead = _baseStream.Read(buffer, offset, count);

        // ako smo procitali nesto azuriramo MD5 hash sa tim podacima
        if (bytesRead > 0) _md5.Update(buffer, offset, bytesRead);

        return bytesRead;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        // upisujemo podatke na stream
        _baseStream.Write(buffer, offset, count);

        // azuriramo MD5 hash sa tim podacima
        _md5.Update(buffer, offset, count);
    }


    // boilerplate delegiranje svih ostalih metoda na _baseStream
    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length => _baseStream.Length;

    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    public override void Flush() => _baseStream.Flush();

    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

    public override void SetLength(long value) => _baseStream.SetLength(value);

    // kad zatvorimo ovaj wrapper zatvoricemo i bazni stream
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _baseStream.Dispose();
        }
        base.Dispose(disposing);
    }
}


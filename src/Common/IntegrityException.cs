using System;

namespace src.Common;

public class IntegrityException : Exception
{
    public IntegrityException(string message) : base(message) { }
}

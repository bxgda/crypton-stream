using System;
using System.Security.Cryptography;
using System.Text;

namespace src.Core;

public static class MD5_Hasher
{
    public static string CalculateMD5(byte[] data)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(data);

            StringBuilder sb = new StringBuilder();

            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}

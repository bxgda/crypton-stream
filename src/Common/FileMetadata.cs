using System;

namespace src.Common;

public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime Timestamp { get; set; }

    public string EncryptingAlgorithm { get; set; } = string.Empty;

    public string? Nonce { get; set; } = string.Empty;

    public string HashValue { get; set; } = string.Empty;

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public static FileMetadata? FromJson(string json)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(json);
        }
        catch
        {
            return null;
        }
    }
}

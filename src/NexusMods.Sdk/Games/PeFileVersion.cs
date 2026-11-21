using System.Buffers.Binary;
using NexusMods.Paths;

namespace NexusMods.Sdk.Games;

/// <summary>
/// Reads the file version embedded in a Windows executable (the VS_FIXEDFILEINFO resource).
/// <see cref="System.Diagnostics.FileVersionInfo"/> can't read native PE files on Linux, so this
/// scans for the structure directly.
/// </summary>
public static class PeFileVersion
{
    private const uint FixedFileInfoSignature = 0xFEEF04BD;
    private const uint FixedFileInfoStrucVersion = 0x00010000;

    public static async Task<string?> TryReadAsync(AbsolutePath path, CancellationToken cancellationToken = default)
    {
        if (!path.FileExists) return null;

        byte[] bytes;
        try
        {
            bytes = await File.ReadAllBytesAsync(path.ToString(), cancellationToken);
        }
        catch (IOException)
        {
            return null;
        }

        return TryRead(bytes);
    }

    public static string? TryRead(ReadOnlySpan<byte> bytes)
    {
        // "MZ" header: only Windows executables carry this resource
        if (bytes.Length < 2 || bytes[0] != 'M' || bytes[1] != 'Z') return null;

        Span<byte> signature = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(signature, FixedFileInfoSignature);

        var offset = 0;
        while (true)
        {
            var index = bytes[offset..].IndexOf(signature);
            if (index < 0) return null;
            var start = offset + index;
            offset = start + 4;

            // dwSignature, dwStrucVersion, dwFileVersionMS, dwFileVersionLS
            if (start + 16 > bytes.Length) return null;
            if (BinaryPrimitives.ReadUInt32LittleEndian(bytes[(start + 4)..]) != FixedFileInfoStrucVersion) continue;

            var ms = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(start + 8)..]);
            var ls = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(start + 12)..]);
            if (ms == 0 && ls == 0) return null;

            return $"{ms >> 16}.{ms & 0xFFFF}.{ls >> 16}.{ls & 0xFFFF}";
        }
    }
}

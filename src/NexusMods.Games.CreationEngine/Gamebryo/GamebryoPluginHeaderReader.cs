using System.Buffers.Binary;
using System.Text;
using Mutagen.Bethesda.Plugins;
using NexusMods.Games.CreationEngine.Abstractions;

namespace NexusMods.Games.CreationEngine.Gamebryo;

/// <summary>
/// Reads the TES4 header record of a Fallout 3 or New Vegas plugin. Mutagen doesn't support these games,
/// and the only things needed are the masters, which live in the header record.
/// </summary>
/// <remarks>
/// Layout: a 24 byte record header ("TES4", data size, flags, form id, version control, form version, unknown)
/// followed by subrecords, each a 4 byte type, a 2 byte size and the data. Masters are "MAST" subrecords
/// holding a null-terminated file name.
/// </remarks>
public static class GamebryoPluginHeaderReader
{
    private const int RecordHeaderSize = 24;
    private const int SubrecordHeaderSize = 6;

    // Plugin headers are small; anything bigger than this is not a plugin we can read
    private const int MaxHeaderDataSize = 1024 * 1024;

    private static readonly Encoding Windows1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252) ?? Encoding.Latin1;

    /// <summary>
    /// Reads the header of a plugin, or returns null if the stream doesn't hold a valid plugin header.
    /// </summary>
    public static async ValueTask<PluginHeader?> ReadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        if (!ModKey.TryFromFileName(fileName, out var modKey)) return null;

        var recordHeader = new byte[RecordHeaderSize];
        if (!await TryReadExactly(stream, recordHeader, cancellationToken)) return null;
        if (!recordHeader.AsSpan(0, 4).SequenceEqual("TES4"u8)) return null;

        var dataSize = BinaryPrimitives.ReadUInt32LittleEndian(recordHeader.AsSpan(4, 4));
        if (dataSize > MaxHeaderDataSize) return null;

        var data = new byte[dataSize];
        if (!await TryReadExactly(stream, data, cancellationToken)) return null;

        var masters = ReadMasters(data);
        return masters is null ? null : new PluginHeader(modKey, masters);
    }

    /// <summary>
    /// Reads the master list from the TES4 record data, or returns null if the data is malformed.
    /// </summary>
    internal static List<ModKey>? ReadMasters(ReadOnlySpan<byte> data)
    {
        var masters = new List<ModKey>();
        var offset = 0;
        while (offset + SubrecordHeaderSize <= data.Length)
        {
            var type = data.Slice(offset, 4);
            var size = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset + 4, 2));
            offset += SubrecordHeaderSize;
            if (offset + size > data.Length) return null;

            if (type.SequenceEqual("MAST"u8))
            {
                var value = data.Slice(offset, size);
                var terminator = value.IndexOf((byte)0);
                if (terminator >= 0) value = value[..terminator];

                var name = Windows1252.GetString(value);
                if (ModKey.TryFromFileName(name, out var master))
                    masters.Add(master);
            }

            offset += size;
        }

        return masters;
    }

    private static async ValueTask<bool> TryReadExactly(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        try
        {
            await stream.ReadExactlyAsync(buffer, cancellationToken);
            return true;
        }
        catch (EndOfStreamException)
        {
            return false;
        }
    }
}

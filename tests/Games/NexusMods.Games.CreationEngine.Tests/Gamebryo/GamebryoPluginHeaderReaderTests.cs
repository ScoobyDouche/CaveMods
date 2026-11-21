using System.Buffers.Binary;
using System.Text;
using FluentAssertions;
using NexusMods.Games.CreationEngine.Gamebryo;

namespace NexusMods.Games.CreationEngine.Tests.Gamebryo;

public class GamebryoPluginHeaderReaderTests
{
    private static byte[] Subrecord(string type, byte[] data)
    {
        var buffer = new byte[6 + data.Length];
        Encoding.ASCII.GetBytes(type).CopyTo(buffer, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(4), (ushort)data.Length);
        data.CopyTo(buffer, 6);
        return buffer;
    }

    private static byte[] ZString(string value) => [..Encoding.ASCII.GetBytes(value), 0];

    private static byte[] Plugin(string recordType, params byte[][] subrecords)
    {
        var data = subrecords.SelectMany(x => x).ToArray();
        var header = new byte[24];
        Encoding.ASCII.GetBytes(recordType).CopyTo(header, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4), (uint)data.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8), 1); // ESM flag
        // A plugin has more records after the header, which the reader must not touch
        byte[] trailing = [..Encoding.ASCII.GetBytes("GRUP"), 0xFF, 0xFF, 0xFF, 0xFF];
        return [..header, ..data, ..trailing];
    }

    [Fact]
    public async Task ReadsMastersInOrder()
    {
        var bytes = Plugin("TES4",
            Subrecord("HEDR", new byte[12]),
            Subrecord("CNAM", ZString("author")),
            Subrecord("MAST", ZString("FalloutNV.esm")),
            Subrecord("DATA", new byte[8]),
            Subrecord("MAST", ZString("DeadMoney.esm")),
            Subrecord("DATA", new byte[8]),
            Subrecord("MAST", ZString("my mod.esp")));

        var header = await GamebryoPluginHeaderReader.ReadAsync(new MemoryStream(bytes), "Test.esp");

        header.Should().NotBeNull();
        header!.ModKey.FileName.ToString().Should().Be("Test.esp");
        header.Masters.Select(m => m.FileName.ToString()).Should().Equal("FalloutNV.esm", "DeadMoney.esm", "my mod.esp");
    }

    [Fact]
    public async Task ReadsPluginWithoutMasters()
    {
        var bytes = Plugin("TES4", Subrecord("HEDR", new byte[12]));

        var header = await GamebryoPluginHeaderReader.ReadAsync(new MemoryStream(bytes), "Fallout3.esm");

        header.Should().NotBeNull();
        header!.Masters.Should().BeEmpty();
    }

    [Fact]
    public async Task RejectsNonPluginData()
    {
        var bytes = Plugin("TES3", Subrecord("MAST", ZString("Morrowind.esm")));
        (await GamebryoPluginHeaderReader.ReadAsync(new MemoryStream(bytes), "Test.esp")).Should().BeNull();
    }

    [Fact]
    public async Task RejectsTruncatedFile()
    {
        var bytes = Plugin("TES4", Subrecord("MAST", ZString("FalloutNV.esm")));
        (await GamebryoPluginHeaderReader.ReadAsync(new MemoryStream(bytes[..30]), "Test.esp")).Should().BeNull();
        (await GamebryoPluginHeaderReader.ReadAsync(new MemoryStream(bytes[..10]), "Test.esp")).Should().BeNull();
    }

    [Fact]
    public async Task RejectsSubrecordOverrunningHeader()
    {
        // MAST claims 200 bytes but the header data is much shorter
        var bad = Subrecord("MAST", ZString("FalloutNV.esm"));
        BinaryPrimitives.WriteUInt16LittleEndian(bad.AsSpan(4), 200);
        var bytes = Plugin("TES4", bad);

        (await GamebryoPluginHeaderReader.ReadAsync(new MemoryStream(bytes), "Test.esp")).Should().BeNull();
    }

    [Fact]
    public async Task RejectsNonPluginFileName()
    {
        var bytes = Plugin("TES4", Subrecord("HEDR", new byte[12]));
        (await GamebryoPluginHeaderReader.ReadAsync(new MemoryStream(bytes), "readme.txt")).Should().BeNull();
    }

    /// <summary>
    /// Sanity check against real game files, only runs when the Steam versions are installed locally.
    /// </summary>
    [Theory]
    [InlineData("Fallout 3 goty", "ThePitt.esm", new[] { "Fallout3.esm" })]
    [InlineData("Fallout 3 goty", "Fallout3.esm", new string[0])]
    [InlineData("Fallout New Vegas", "DeadMoney.esm", new[] { "FalloutNV.esm" })]
    [InlineData("Fallout New Vegas", "FalloutNV.esm", new string[0])]
    public async Task ReadsInstalledGamePlugins(string gameFolder, string plugin, string[] expectedMasters)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local/share/Steam/steamapps/common", gameFolder, "Data", plugin);
        if (!File.Exists(path)) return;

        await using var stream = File.OpenRead(path);
        var header = await GamebryoPluginHeaderReader.ReadAsync(stream, plugin);

        header.Should().NotBeNull();
        header!.Masters.Select(m => m.FileName.ToString()).Should().Equal(expectedMasters);
    }
}

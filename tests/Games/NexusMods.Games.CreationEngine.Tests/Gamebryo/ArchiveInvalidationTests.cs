using FluentAssertions;
using NexusMods.Games.CreationEngine.Emitters;

namespace NexusMods.Games.CreationEngine.Tests.Gamebryo;

public class ArchiveInvalidationTests
{
    [Theory]
    [InlineData("[Archive]\r\nSInvalidationFile=ArchiveInvalidation.txt\r\nbInvalidateOlderFiles=1\r\n", true)]
    [InlineData("[Archive]\nbInvalidateOlderFiles = 1\n", true)]
    [InlineData("[archive]\nbinvalidateolderfiles=1\n", true)]
    [InlineData("[Archive]\nbInvalidateOlderFiles=0\n", false)]
    [InlineData("[Archive]\nSInvalidationFile=\n", false)]
    // The setting only counts in the [Archive] section
    [InlineData("[General]\nbInvalidateOlderFiles=1\n[Archive]\nbInvalidateOlderFiles=0\n", false)]
    [InlineData("[Archive]\nbInvalidateOlderFiles=0\n[General]\nbInvalidateOlderFiles=1\n", false)]
    [InlineData("", false)]
    public void ReadsInvalidationSetting(string ini, bool expected)
    {
        ArchiveInvalidationEmitter.IsInvalidationEnabled(ini).Should().Be(expected);
    }
}

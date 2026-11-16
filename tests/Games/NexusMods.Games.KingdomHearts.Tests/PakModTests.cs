using FluentAssertions;
using NexusMods.Games.KingdomHearts.Unreal;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using Xunit;

namespace NexusMods.Games.KingdomHearts.Tests;

public class PakModTests
{
    private static bool Managed(string path) => PakModPaths.KH3.IsManaged(new GamePath(LocationId.Game, path));

    [Theory]
    [InlineData("KINGDOM HEARTS III/Content/Paks/~mods/Sora_P.pak")]
    [InlineData("KINGDOM HEARTS III/Content/Paks/~mods/sub/Sora_P.pak")]
    public void ModFilesAreManaged(string path) => Managed(path).Should().BeTrue();

    [Theory]
    // the base game's own files must never be managed, or the synchronizer would delete them
    [InlineData("KINGDOM HEARTS III/Content/Paks/pakchunk0-WindowsNoEditor.pak")]
    [InlineData("KINGDOM HEARTS III/Content/Paks/~mods")]
    [InlineData("KINGDOM HEARTS III/Content/Paks/~mods2/x.pak")]
    [InlineData("KINGDOM HEARTS III/Binaries/Win64/KINGDOM HEARTS III.exe")]
    [InlineData("KINGDOM HEARTS III.exe")]
    public void GameFilesAreNotManaged(string path) => Managed(path).Should().BeFalse();

    private static bool KH02Managed(string path) => PakModPaths.KH02.IsManaged(new GamePath(LocationId.Game, path));

    [Fact]
    public void KH02ModFilesAreManaged() =>
        KH02Managed("KINGDOM HEARTS 0.2 Birth by Sleep/Content/Paks/~mods/x_P.pak").Should().BeTrue();

    [Theory]
    [InlineData("KINGDOM HEARTS 0.2 Birth by Sleep/Content/Paks/TresGame-WindowsNoEditor_0_P.pak")]
    [InlineData("Image/dt/kh3d_first.pkg")]
    [InlineData("KINGDOM HEARTS III/Content/Paks/~mods/x_P.pak")]
    public void KH02GameFilesAreNotManaged(string path) => KH02Managed(path).Should().BeFalse();

    [Fact]
    public void ExecutablesPointIntoTheProjectFolders()
    {
        PakModPaths.KH3.Executable.ToString().Should().Be("KINGDOM HEARTS III/Binaries/Win64/KINGDOM HEARTS III.exe");
        PakModPaths.KH02.Executable.ToString().Should().Be("KINGDOM HEARTS 0.2 Birth by Sleep/Binaries/Win64/KINGDOM HEARTS 0.2 Birth by Sleep.exe");
    }

    private static PakModInstaller.Selection Select(params string[] paths) =>
        PakModInstaller.SelectFiles(paths.Select(p => (RelativePath)p));

    [Fact]
    public void PicksPaksAndTheirCompanions()
    {
        var result = Select("Mod/Sora_P.pak", "Mod/Sora_P.sig", "Mod/Other.sig", "readme.txt", "Mod/preview.png");
        result.Error.Should().BeNull();
        result.Files.Select(f => f.ToString()).Should().BeEquivalentTo("Mod/Sora_P.pak", "Mod/Sora_P.sig");
    }

    [Fact]
    public void RejectsArchivesWithoutPaks() => Select("readme.txt").Error.Should().NotBeNull();

    [Fact]
    public void RejectsSeveralVersionsOfTheSamePak() =>
        Select("Red/Sora_P.pak", "Blue/Sora_P.pak").Error.Should().Contain("Sora_P.pak");
}

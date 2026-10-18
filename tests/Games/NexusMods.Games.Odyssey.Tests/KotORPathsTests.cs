using FluentAssertions;
using NexusMods.Games.Odyssey.KotOR;
using NexusMods.Sdk.Games;
using Xunit;

namespace NexusMods.Games.Odyssey.Tests;

public class KotORPathsTests
{
    [Theory]
    [InlineData("Override/n_darthmalak.tga")]
    [InlineData("override/appearance.2da")]
    [InlineData("Override/subfolder/file.tpc")]
    [InlineData("modules/ekun.mod")]
    [InlineData("Modules/ekun.mod")]
    public void ModFilesAreManaged(string path)
    {
        KotORPaths.IsManaged(new GamePath(LocationId.Game, path)).Should().BeTrue();
    }

    [Theory]
    // the base game's own files must never be managed, or the synchronizer would delete them
    [InlineData("swkotor.exe")]
    [InlineData("chitin.key")]
    [InlineData("dialog.tlk")]
    [InlineData("data/templates.bif")]
    [InlineData("modules/danm13.rim")]
    [InlineData("movies/legal.bik")]
    [InlineData("streamwaves/n_bastila_01.wav")]
    [InlineData("lips/danm13_loc.mod")]
    [InlineData("Saves/000002 - Game1/savegame.sav")]
    [InlineData("modules/sub/x.mod")]
    [InlineData("Override")]
    [InlineData("640x480/legal.bik")]
    public void GameFilesAreNotManaged(string path)
    {
        KotORPaths.IsManaged(new GamePath(LocationId.Game, path)).Should().BeFalse();
    }

    [Theory]
    [InlineData("override/a.tga", "Override/a.tga")]
    [InlineData("OVERRIDE/sub/a.tga", "Override/sub/a.tga")]
    [InlineData("Modules/ekun.mod", "modules/ekun.mod")]
    [InlineData("Source/k_pend.nss", "Source/k_pend.nss")]
    [InlineData("a.tga", "a.tga")]
    public void TopFolderIsNormalized(string input, string expected)
    {
        KotORPaths.NormalizeTopFolder(input).ToString().Should().Be(expected);
    }
}

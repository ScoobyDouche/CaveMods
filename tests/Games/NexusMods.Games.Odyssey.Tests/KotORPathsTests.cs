using FluentAssertions;
using NexusMods.Games.Odyssey.KotOR;
using NexusMods.Sdk.Games;
using Xunit;

namespace NexusMods.Games.Odyssey.Tests;

public class KotORPathsTests
{
    private static bool K1Managed(string path) => KotORPaths.KotOR1.IsManaged(new GamePath(LocationId.Game, path));
    private static bool K2Managed(string path) => KotORPaths.KotOR2.IsManaged(new GamePath(LocationId.Game, path));

    [Theory]
    [InlineData("Override/n_darthmalak.tga")]
    [InlineData("override/appearance.2da")]
    [InlineData("Override/subfolder/file.tpc")]
    [InlineData("modules/ekun.mod")]
    [InlineData("Modules/ekun.mod")]
    public void KotOR1ModFilesAreManaged(string path) => K1Managed(path).Should().BeTrue();

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
    public void KotOR1GameFilesAreNotManaged(string path) => K1Managed(path).Should().BeFalse();

    [Theory]
    [InlineData("steamassets/override/n_darthmalak.tga")]
    [InlineData("steamassets/override/appearance.2da")]
    [InlineData("steamassets/modules/ekun.mod")]
    public void KotOR2ModFilesAreManaged(string path) => K2Managed(path).Should().BeTrue();

    [Theory]
    [InlineData("KOTOR2")]
    [InlineData("libMiles.so")]
    [InlineData("resources/en.lproj/x.strings")]
    [InlineData("steamassets/chitin.key")]
    [InlineData("steamassets/swkotor2.ini")]
    [InlineData("steamassets/modules/101per.rim")]
    [InlineData("steamassets/modules/101per_dlg.erf")]
    [InlineData("steamassets/lips/x.mod")]
    [InlineData("steamassets/override")]
    // a Windows-layout path is outside the native data folder
    [InlineData("Override/n_darthmalak.tga")]
    // files Aspyr ships in override
    [InlineData("steamassets/override/handmaiden.dlg")]
    [InlineData("steamassets/override/mainmenu8x6_p.gui")]
    [InlineData("steamassets/override/cus_gpad_bg.tga")]
    [InlineData("steamassets/override/HANDMAIDEN.DLG")]
    public void KotOR2GameFilesAreNotManaged(string path) => K2Managed(path).Should().BeFalse();

    [Theory]
    [InlineData("override/a.tga", "Override/a.tga")]
    [InlineData("OVERRIDE/sub/a.tga", "Override/sub/a.tga")]
    [InlineData("Modules/ekun.mod", "modules/ekun.mod")]
    [InlineData("Override/N_DarthMalak.tga", "Override/N_DarthMalak.tga")]
    [InlineData("Source/k_pend.nss", "Source/k_pend.nss")]
    [InlineData("a.tga", "a.tga")]
    public void KotOR1PathIsNormalized(string input, string expected)
    {
        KotORPaths.KotOR1.Normalize(input).ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("steamassets/Override/N_DarthMalak.tga", "steamassets/override/n_darthmalak.tga")]
    [InlineData("steamassets/override/Sub/A.TGA", "steamassets/override/sub/a.tga")]
    [InlineData("steamassets/Modules/EKUN.mod", "steamassets/modules/ekun.mod")]
    [InlineData("steamassets/Source/K_Pend.nss", "steamassets/Source/K_Pend.nss")]
    [InlineData("KOTOR2", "KOTOR2")]
    public void KotOR2PathIsNormalizedToLowerCase(string input, string expected)
    {
        KotORPaths.KotOR2.Normalize(input).ToString().Should().Be(expected);
    }
}

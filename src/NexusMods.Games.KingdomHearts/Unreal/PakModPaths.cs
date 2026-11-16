using NexusMods.Paths;
using NexusMods.Sdk.Games;

namespace NexusMods.Games.KingdomHearts.Unreal;

/// <summary>
/// Where an Unreal Engine 4 Kingdom Hearts game (Windows build, run through Proton) keeps its pak mods.
/// </summary>
/// <remarks>
/// None of these games have a Linux build. UE4 loads any extra .pak it finds under Content/Paks, and the
/// community convention is a "~mods" folder there (the tilde makes it load after the base paks).
/// The games aren't in the file hashes database, so the synchronizer has no list of their own files and
/// would treat them as stray files to delete. The app therefore only ever looks inside ~mods, where
/// the base game ships nothing.
/// </remarks>
internal sealed class PakModPaths
{
    /// <summary>
    /// Kingdom Hearts III: Steam install dir holds the "KINGDOM HEARTS III" Unreal project folder.
    /// </summary>
    internal static readonly PakModPaths KH3 = new("KINGDOM HEARTS III", "KINGDOM HEARTS III.exe");

    /// <summary>
    /// Kingdom Hearts 0.2 Birth by Sleep, shipped inside the HD 2.8 Final Chapter Prologue install
    /// (next to Dream Drop Distance, which uses a different engine and isn't handled).
    /// </summary>
    internal static readonly PakModPaths KH02 = new("KINGDOM HEARTS 0.2 Birth by Sleep", "KINGDOM HEARTS 0.2 Birth by Sleep.exe");

    internal static readonly Extension Pak = new(".pak");

    /// <summary>
    /// Files that belong to a .pak and have to sit next to it (signature, and IoStore containers in newer UE versions).
    /// </summary>
    internal static readonly Extension[] PakCompanions = [new(".sig"), new(".utoc"), new(".ucas")];

    private PakModPaths(RelativePath projectFolder, string executable)
    {
        ModsFolder = projectFolder.Join("Content/Paks/~mods");
        Executable = projectFolder.Join("Binaries/Win64").Join(executable);
    }

    internal RelativePath ModsFolder { get; }

    internal RelativePath Executable { get; }

    internal bool IsManaged(GamePath path)
    {
        if (path.LocationId != LocationId.Game) return false;
        if (!path.Path.StartsWith(ModsFolder)) return false;
        return path.Path.RelativeTo(ModsFolder).Parts.Any();
    }
}

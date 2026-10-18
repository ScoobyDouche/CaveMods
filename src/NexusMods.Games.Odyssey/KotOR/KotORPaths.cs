using NexusMods.Paths;
using NexusMods.Sdk.Games;

namespace NexusMods.Games.Odyssey.KotOR;

/// <summary>
/// The part of the KotOR folder the app is allowed to manage.
/// </summary>
/// <remarks>
/// KotOR isn't in the file hashes database, so the synchronizer has no list of the game's own files
/// and would treat them as stray files to delete. Instead the app only ever looks at places the
/// base game doesn't ship anything: the Override folder and .mod files in modules (vanilla only has .rim).
/// </remarks>
internal static class KotORPaths
{
    internal static readonly RelativePath OverrideFolder = "Override";
    internal static readonly RelativePath ModulesFolder = "modules";
    internal static readonly Extension ModuleExtension = new(".mod");

    private static readonly RelativePath[] TopFolders = [OverrideFolder, ModulesFolder];

    internal static bool IsManaged(GamePath path)
    {
        if (path.LocationId != LocationId.Game) return false;

        var parts = path.Path.Parts.ToArray();
        if (parts.Length < 2) return false;

        if (parts[0].Equals(OverrideFolder)) return true;
        return parts.Length == 2 && parts[0].Equals(ModulesFolder) && path.Path.Extension.Equals(ModuleExtension);
    }

    /// <summary>
    /// Rewrites the first folder of <paramref name="path"/> to the game's own spelling, since Linux
    /// file systems are case-sensitive and e.g. "Modules" would create a second folder next to "modules".
    /// </summary>
    internal static RelativePath NormalizeTopFolder(RelativePath path)
    {
        var parts = path.Parts.ToArray();
        if (parts.Length < 2) return path;

        foreach (var folder in TopFolders)
        {
            if (folder.Equals(parts[0])) return folder.Join(path.DropFirst(1));
        }

        return path;
    }
}

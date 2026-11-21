using System.Runtime.CompilerServices;
using NexusMods.Abstractions.Diagnostics;
using NexusMods.Abstractions.Diagnostics.Emitters;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.Loadouts.Extensions;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Loadouts;

namespace NexusMods.Games.CreationEngine.Emitters;

/// <summary>
/// Warns when mods install loose textures or meshes into Fallout 3 / New Vegas but archive invalidation is off,
/// in which case the game keeps using the copies packed in its BSA archives.
/// </summary>
public class ArchiveInvalidationEmitter : ILoadoutDiagnosticEmitter
{
    private static readonly GamePath UserIni = new(LocationId.Preferences, "Fallout.ini");
    private static readonly GamePath DefaultIni = new(LocationId.Game, "Fallout_default.ini");

    // Vanilla Steam installs have no loose files in these folders, so anything here comes from a mod
    private static readonly RelativePath[] LooseAssetFolders = ["Data/textures", "Data/meshes"];

    public async IAsyncEnumerable<Diagnostic> Diagnose(Loadout.ReadOnly loadout, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();

        var hasLooseAssets = LoadoutItem.FindByLoadout(loadout.Db, loadout)
            .OfTypeLoadoutItemWithTargetPath()
            .Any(item =>
            {
                var path = (GamePath)item.TargetPath;
                return path.LocationId == LocationId.Game
                       && LooseAssetFolders.Any(folder => path.Path.InFolder(folder))
                       && item.AsLoadoutItem().IsEnabled();
            });
        if (!hasLooseAssets) yield break;

        // The game reads the user ini, which it creates from the default one on first launch
        var locations = loadout.InstallationInstance.Locations;
        var userIni = locations.ToAbsolutePath(UserIni);
        var iniToCheck = userIni.FileExists ? userIni : locations.ToAbsolutePath(DefaultIni);

        if (iniToCheck.FileExists && IsInvalidationEnabled(await iniToCheck.ReadAllTextAsync(cancellationToken)))
            yield break;

        yield return Diagnostics.CreateArchiveInvalidationDisabled(userIni.ToString());
    }

    /// <summary>
    /// Returns true if the ini enables <c>bInvalidateOlderFiles</c> in its <c>[Archive]</c> section.
    /// </summary>
    public static bool IsInvalidationEnabled(string iniText)
    {
        var inArchive = false;
        foreach (var rawLine in iniText.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.StartsWith('['))
            {
                inArchive = line.Equals("[Archive]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inArchive) continue;

            var separator = line.IndexOf('=');
            if (separator < 0) continue;

            var key = line[..separator].Trim();
            if (!key.Equals("bInvalidateOlderFiles", StringComparison.OrdinalIgnoreCase)) continue;

            return line[(separator + 1)..].Trim() == "1";
        }

        return false;
    }
}

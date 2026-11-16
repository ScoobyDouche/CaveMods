using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.Library.Installers;
using NexusMods.Abstractions.Loadouts;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Paths;
using NexusMods.Paths.Trees.Traits;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Library;
using NexusMods.Sdk.Loadouts;

namespace NexusMods.Games.KingdomHearts.Unreal;

/// <summary>
/// Installs Unreal Engine pak mods: every .pak in the archive (plus its .sig/.utoc/.ucas)
/// goes flat into Content/Paks/~mods, wherever it sits in the archive.
/// </summary>
/// <remarks>
/// Archives that ship several versions of the same pak (same file name in different folders) are
/// rejected, because installing all of them would load every version at once.
/// UE4SS / LogicMods mods need a script loader injected into the game and aren't handled.
/// </remarks>
internal class PakModInstaller : ALibraryArchiveInstaller
{
    private readonly PakModPaths _paths;

    public PakModInstaller(IServiceProvider serviceProvider, PakModPaths paths)
        : base(serviceProvider, serviceProvider.GetRequiredService<ILogger<PakModInstaller>>())
    {
        _paths = paths;
    }

    public override ValueTask<InstallerResult> ExecuteAsync(
        LibraryArchive.ReadOnly libraryArchive,
        LoadoutItemGroup.New loadoutGroup,
        ITransaction tx,
        Loadout.ReadOnly loadout,
        CancellationToken cancellationToken)
    {
        var tree = LibraryArchiveTreeExtensions.GetTree(libraryArchive);
        var files = tree.EnumerateFilesBfs().Select(f => f.Value).ToArray();

        var selection = SelectFiles(files.Select(f => f.Item.Path));
        if (selection.Error is not null)
            return ValueTask.FromResult<InstallerResult>(new NotSupported(Reason: selection.Error));

        var wanted = selection.Files.ToHashSet();
        foreach (var file in files)
        {
            if (!wanted.Contains(file.Item.Path)) continue;
            file.ToLoadoutFile(loadout.Id, loadoutGroup.Id, tx, new GamePath(LocationId.Game, _paths.ModsFolder.Join(file.Item.Path.FileName)));
        }

        return ValueTask.FromResult<InstallerResult>(new Success());
    }

    internal record Selection(RelativePath[] Files, string? Error);

    /// <summary>
    /// Picks the archive files to install: each .pak and the companion files that share its folder and name.
    /// </summary>
    internal static Selection SelectFiles(IEnumerable<RelativePath> archivePaths)
    {
        var all = archivePaths.ToArray();
        var paks = all.Where(p => p.Extension.Equals(PakModPaths.Pak)).ToArray();
        if (paks.Length == 0)
            return new Selection([], "Archive contains no .pak files");

        var duplicate = paks.GroupBy(p => p.FileName.ToString(), StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            return new Selection([], $"Archive has several versions of {duplicate.Key}; install the one you want manually");

        var stems = paks.Select(StemKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var companions = all.Where(p => PakModPaths.PakCompanions.Any(e => p.Extension.Equals(e)) && stems.Contains(StemKey(p)));

        return new Selection(paks.Concat(companions).ToArray(), null);
    }

    private static string StemKey(RelativePath path) => path.Parent.Join(path.FileName.ToString()[..^path.Extension.ToString().Length]).ToString();
}

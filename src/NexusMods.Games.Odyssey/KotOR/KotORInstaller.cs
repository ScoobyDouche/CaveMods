using NexusMods.Abstractions.Library.Installers;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.Generic.Installers;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Paths;
using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Library;
using NexusMods.Sdk.Loadouts;
using LibraryArchiveTreeExtensions = NexusMods.Sdk.Library.LibraryArchiveTreeExtensions;

namespace NexusMods.Games.Odyssey.KotOR;

/// <summary>
/// Installer for loose-file KotOR mods: files dropped into Override, or archives
/// that mirror the game folder layout (Override, modules, movies, ...).
/// </summary>
/// <remarks>
/// TSLPatcher/HoloPatcher mods (with a "tslpatchdata" folder) edit existing game files
/// and can't be installed by copying, so they are rejected.
/// </remarks>
public class KotORInstaller : GenericPatternMatchInstaller
{
    private static readonly RelativePath TslPatchDataFolder = "tslpatchdata";

    /// <summary>
    /// Top-level game folders, spelled as the game ships them. Linux file systems are
    /// case-sensitive, so mod archives using e.g. "Modules" are rewritten to "modules".
    /// </summary>
    internal static readonly RelativePath[] GameFolders =
    [
        "Override", "modules", "movies", "streammusic", "streamsounds", "streamwaves", "lips", "TexturePacks",
    ];

    private static readonly Extension[] DocumentationExtensions =
    [
        new(".txt"), new(".rtf"), new(".pdf"), new(".md"), new(".doc"), new(".docx"),
        new(".html"), new(".htm"), new(".url"), new(".png"), new(".jpg"), new(".jpeg"),
    ];

    private static readonly Extension[] OverrideExtensions =
    [
        // textures
        new(".tga"), new(".tpc"), new(".txi"),
        // models and walkmeshes
        new(".mdl"), new(".mdx"), new(".wok"), new(".pwk"), new(".dwk"),
        // game data
        new(".2da"), new(".dlg"), new(".ncs"), new(".ssf"), new(".lip"), new(".gui"), new(".jrl"),
        // blueprints
        new(".uti"), new(".utc"), new(".utp"), new(".utd"), new(".ute"), new(".utm"), new(".uts"), new(".utt"), new(".utw"),
        // audio
        new(".wav"),
    ];

    public KotORInstaller(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        InstallFolderTargets =
        [
            // archives that mirror the game folder, e.g. "MyMod/Override/..." or "modules/..."
            new InstallFolderTarget
            {
                DestinationGamePath = new GamePath(LocationId.Game, ""),
                Names = GameFolders,
                FileExtensionsToDiscard = DocumentationExtensions,
            },

            // loose Override files, e.g. "MyMod/N_DarthMalak.tga"
            new InstallFolderTarget
            {
                DestinationGamePath = new GamePath(LocationId.Game, "Override"),
                KnownValidFileExtensions = OverrideExtensions,
                FileExtensionsToDiscard = DocumentationExtensions,
            },
        ];
    }

    public override ValueTask<InstallerResult> ExecuteAsync(
        LibraryArchive.ReadOnly libraryArchive,
        LoadoutItemGroup.New loadoutGroup,
        ITransaction transaction,
        Loadout.ReadOnly loadout,
        CancellationToken cancellationToken)
    {
        var tree = LibraryArchiveTreeExtensions.GetTree(libraryArchive);
        var isPatcherMod = tree.EnumerateChildrenBfs().Any(node => node.Value.IsDirectory() && node.Value.Key().Name.Equals(TslPatchDataFolder));
        if (isPatcherMod)
            return ValueTask.FromResult<InstallerResult>(new NotSupported(Reason: "TSLPatcher mods need to be installed with TSLPatcher or HoloPatcher"));

        return base.ExecuteAsync(libraryArchive, loadoutGroup, transaction, loadout, cancellationToken);
    }

    protected override void GenerateFileItem(
        InstallFolderTarget target,
        ITransaction transaction,
        Loadout.ReadOnly loadout,
        RelativePath relativePath,
        LoadoutItemGroup.New loadoutGroup,
        KeyedBox<RelativePath, LibraryArchiveTree> fileNode)
    {
        base.GenerateFileItem(target, transaction, loadout, NormalizeGameFolder(relativePath), loadoutGroup, fileNode);
    }

    /// <summary>
    /// Rewrites the first folder of <paramref name="path"/> to the game's own spelling.
    /// </summary>
    internal static RelativePath NormalizeGameFolder(RelativePath path)
    {
        var parts = path.Parts.ToArray();
        if (parts.Length < 2) return path;

        var topFolder = parts[0];
        foreach (var gameFolder in GameFolders)
        {
            if (!gameFolder.Equals(topFolder)) continue;
            return gameFolder.Join(path.DropFirst(1));
        }

        return path;
    }
}

using System.Collections.Immutable;
using DynamicData.Kernel;
using Microsoft.Extensions.DependencyInjection;
using Mutagen.Bethesda.Plugins;
using NexusMods.Abstractions.Diagnostics.Emitters;
using NexusMods.Abstractions.Games;
using NexusMods.Abstractions.Library.Installers;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Games.CreationEngine.Abstractions;
using NexusMods.Games.CreationEngine.Emitters;
using NexusMods.Games.CreationEngine.Installers;
using NexusMods.Games.FOMOD;
using NexusMods.Hashing.xxHash3;
using NexusMods.Paths;
using NexusMods.Sdk.FileStore;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.IO;

namespace NexusMods.Games.CreationEngine.Gamebryo;

/// <summary>
/// Shared logic for Fallout 3 and Fallout: New Vegas. Both run on the Gamebryo engine, which Mutagen
/// doesn't support, and both use a timestamp-ordered load order.
/// </summary>
public abstract class AGamebryoFalloutGame<TSelf> : ICreationEngineGame
    where TSelf : AGamebryoFalloutGame<TSelf>, IGameData<TSelf>
{
    GameId IGameData.GameId => TSelf.GameId;
    string IGameData.DisplayName => TSelf.DisplayName;
    Optional<Sdk.NexusModsApi.NexusModsGameId> IGameData.NexusModsGameId => TSelf.NexusModsGameId;

    private readonly IStreamSourceDispatcher _streamSource;
    private readonly Lazy<ILoadoutSynchronizer> _synchronizer;
    private readonly Lazy<ISortOrderManager> _sortOrderManager;

    protected AGamebryoFalloutGame(IServiceProvider provider)
    {
        _streamSource = provider.GetRequiredService<IStreamSourceDispatcher>();

        _synchronizer = new Lazy<ILoadoutSynchronizer>(() => new GamebryoFalloutSynchronizer(provider, this));
        _sortOrderManager = new Lazy<ISortOrderManager>(() =>
        {
            var sortOrderManager = provider.GetRequiredService<SortOrderManager>();
            sortOrderManager.RegisterSortOrderVarieties([], this);
            return sortOrderManager;
        });

        DiagnosticEmitters =
        [
            new MissingMasterEmitter(this),
            new ArchiveInvalidationEmitter(),
        ];

        LibraryItemInstallers =
        [
            FomodXmlInstaller.Create(provider, KnownPaths.Data),
            new StopPatternInstaller(provider)
            {
                GameId = TSelf.GameId,
                GameAliases = GameAliases,
                TopLevelDirs = [..KnownPaths.CommonTopLevelFolders, "config"],
                StopPatterns = [$"(^|/){ScriptExtenderFolder}(/|$)"],
                EngineFiles =
                [
                    $@"{ScriptExtenderFolder}_loader\.exe",
                    $@"{ScriptExtenderFolder}_.*\.dll",
                ],
            }.Build(),
        ];
    }

    /// <summary>
    /// Folder name used by the game under AppData/Local and Documents/My Games.
    /// </summary>
    protected abstract string ConfigFolderName { get; }

    /// <summary>
    /// Name of the game's executable.
    /// </summary>
    protected abstract RelativePath ExecutableName { get; }

    /// <summary>
    /// Script extender prefix and folder name, "fose" or "nvse".
    /// </summary>
    protected abstract string ScriptExtenderFolder { get; }

    /// <summary>
    /// Names mod authors use for the game folder inside archives.
    /// </summary>
    protected abstract string[] GameAliases { get; }

    public abstract StoreIdentifiers StoreIdentifiers { get; }
    public abstract IStreamFactory IconImage { get; }
    public abstract IStreamFactory TileImage { get; }
    public abstract ModKey[] OfficialPlugins { get; }

    public ILoadoutSynchronizer Synchronizer => _synchronizer.Value;
    public ISortOrderManager SortOrderManager => _sortOrderManager.Value;
    public ILibraryItemInstaller[] LibraryItemInstallers { get; }
    public IDiagnosticEmitter[] DiagnosticEmitters { get; }

    public PluginListFormat PluginListFormat => PluginListFormat.TimestampOrdered;

    public GamePath PluginsFile { get; } = new(LocationId.AppData, "plugins.txt");

    public ImmutableDictionary<LocationId, AbsolutePath> GetLocations(IFileSystem fileSystem, GameLocatorResult gameLocatorResult)
    {
        return new Dictionary<LocationId, AbsolutePath>()
        {
            { LocationId.Game, gameLocatorResult.Path },
            { LocationId.AppData, fileSystem.GetKnownPath(KnownPath.LocalApplicationDataDirectory) / ConfigFolderName },
            { LocationId.Preferences, fileSystem.GetKnownPath(KnownPath.MyGamesDirectory) / ConfigFolderName },
        }.ToImmutableDictionary();
    }

    public GamePath GetPrimaryFile(GameInstallation installation) => new(LocationId.Game, ExecutableName);

    public async ValueTask<PluginHeader?> ParsePlugin(Hash hash, RelativePath? name = null)
    {
        var fileName = name?.FileName.ToString() ?? "unknown.esm";
        await using var stream = await _streamSource.OpenAsync(hash);
        if (stream is null) return null;
        return await GamebryoPluginHeaderReader.ReadAsync(stream, fileName);
    }
}

/// <summary>
/// Synchronizer for Fallout 3 and New Vegas.
/// </summary>
public class GamebryoFalloutSynchronizer(IServiceProvider provider, ICreationEngineGame game) : ACreationEngineSynchronizer(provider, game);

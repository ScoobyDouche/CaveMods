using System.Collections.Immutable;
using DynamicData.Kernel;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Diagnostics.Emitters;
using NexusMods.Abstractions.Games;
using NexusMods.Abstractions.Library.Installers;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.IO;

namespace NexusMods.Games.KingdomHearts.Unreal;

/// <summary>
/// Kingdom Hearts 0.2 Birth by Sleep -A fragmentary passage- (Unreal Engine 4), from the HD 2.8 Final Chapter Prologue
/// collection. Only 0.2 is modded here: Dream Drop Distance in the same install packs its data in .hed/.pkg
/// archives that need OpenKH. Windows-only, so this targets the Steam build under Proton.
/// </summary>
public class KH02 : IGame, IGameData<KH02>
{
    public static GameId GameId { get; } = GameId.From("SquareEnix.KingdomHearts02");
    public static string DisplayName => "Kingdom Hearts 0.2 Birth by Sleep";
    public static Optional<Sdk.NexusModsApi.NexusModsGameId> NexusModsGameId => Sdk.NexusModsApi.NexusModsGameId.From(3836);

    public StoreIdentifiers StoreIdentifiers { get; } = new(GameId)
    {
        SteamAppIds = [2552440u],
    };

    public IStreamFactory IconImage { get; } = new EmbeddedResourceStreamFactory<KH02>("NexusMods.Games.KingdomHearts.Resources.KingdomHearts02.thumbnail.webp");
    public IStreamFactory TileImage { get; } = new EmbeddedResourceStreamFactory<KH02>("NexusMods.Games.KingdomHearts.Resources.KingdomHearts02.tile.webp");

    private readonly Lazy<ILoadoutSynchronizer> _synchronizer;
    public ILoadoutSynchronizer Synchronizer => _synchronizer.Value;
    public ILibraryItemInstaller[] LibraryItemInstallers { get; }
    private readonly Lazy<ISortOrderManager> _sortOrderManager;
    public ISortOrderManager SortOrderManager => _sortOrderManager.Value;
    public IDiagnosticEmitter[] DiagnosticEmitters { get; } = [];

    public KH02(IServiceProvider provider)
    {
        _synchronizer = new Lazy<ILoadoutSynchronizer>(() => new PakModSynchronizer(provider, PakModPaths.KH02));
        _sortOrderManager = new Lazy<ISortOrderManager>(() =>
        {
            var sortOrderManager = provider.GetRequiredService<SortOrderManager>();
            sortOrderManager.RegisterSortOrderVarieties([], this);
            return sortOrderManager;
        });

        LibraryItemInstallers = [new PakModInstaller(provider, PakModPaths.KH02)];
    }

    public ImmutableDictionary<LocationId, AbsolutePath> GetLocations(IFileSystem fileSystem, GameLocatorResult gameLocatorResult)
    {
        return new Dictionary<LocationId, AbsolutePath>()
        {
            { LocationId.Game, gameLocatorResult.Path },
        }.ToImmutableDictionary();
    }

    public GamePath GetPrimaryFile(GameInstallation installation) => new(LocationId.Game, PakModPaths.KH02.Executable);
}

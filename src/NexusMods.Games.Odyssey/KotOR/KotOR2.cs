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

namespace NexusMods.Games.Odyssey.KotOR;

/// <summary>
/// Star Wars: Knights of the Old Republic II - The Sith Lords (Obsidian, Odyssey engine), the native
/// Linux build by Aspyr. See <see cref="KotORPaths.KotOR2"/> for how its layout differs from KotOR 1.
/// </summary>
public class KotOR2 : IGame, IGameData<KotOR2>
{
    public static GameId GameId { get; } = GameId.From("BioWare.KotOR2");
    public static string DisplayName => "Star Wars: Knights of the Old Republic II: The Sith Lords";
    public static Optional<Sdk.NexusModsApi.NexusModsGameId> NexusModsGameId => Sdk.NexusModsApi.NexusModsGameId.From(198);

    public StoreIdentifiers StoreIdentifiers { get; } = new(GameId)
    {
        SteamAppIds = [208580u],
        GOGProductIds = [1421404581L],
    };

    public IStreamFactory IconImage { get; } = new EmbeddedResourceStreamFactory<KotOR2>("NexusMods.Games.Odyssey.Resources.KotOR2.thumbnail.webp");
    public IStreamFactory TileImage { get; } = new EmbeddedResourceStreamFactory<KotOR2>("NexusMods.Games.Odyssey.Resources.KotOR2.tile.webp");

    private readonly Lazy<ILoadoutSynchronizer> _synchronizer;
    public ILoadoutSynchronizer Synchronizer => _synchronizer.Value;
    public ILibraryItemInstaller[] LibraryItemInstallers { get; }
    private readonly Lazy<ISortOrderManager> _sortOrderManager;
    public ISortOrderManager SortOrderManager => _sortOrderManager.Value;
    public IDiagnosticEmitter[] DiagnosticEmitters { get; } = [];

    public KotOR2(IServiceProvider provider)
    {
        _synchronizer = new Lazy<ILoadoutSynchronizer>(() => new KotORSynchronizer(provider, KotORPaths.KotOR2));
        _sortOrderManager = new Lazy<ISortOrderManager>(() =>
        {
            var sortOrderManager = provider.GetRequiredService<SortOrderManager>();
            sortOrderManager.RegisterSortOrderVarieties([], this);
            return sortOrderManager;
        });

        LibraryItemInstallers = [new KotORInstaller(provider, KotORPaths.KotOR2)];
    }

    public ImmutableDictionary<LocationId, AbsolutePath> GetLocations(IFileSystem fileSystem, GameLocatorResult gameLocatorResult)
    {
        return new Dictionary<LocationId, AbsolutePath>()
        {
            { LocationId.Game, gameLocatorResult.Path },
        }.ToImmutableDictionary();
    }

    public GamePath GetPrimaryFile(GameInstallation installation) => new(LocationId.Game, "KOTOR2");
}

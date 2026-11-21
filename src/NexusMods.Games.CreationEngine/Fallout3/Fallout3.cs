using DynamicData.Kernel;
using Mutagen.Bethesda.Plugins;
using NexusMods.Abstractions.Games;
using NexusMods.Games.CreationEngine.Gamebryo;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.IO;

namespace NexusMods.Games.CreationEngine.Fallout3;

public class Fallout3 : AGamebryoFalloutGame<Fallout3>, IGameData<Fallout3>
{
    public static GameId GameId { get; } = GameId.From("CreationEngine.Fallout3");
    public static string DisplayName => "Fallout 3";
    public static Optional<Sdk.NexusModsApi.NexusModsGameId> NexusModsGameId => Sdk.NexusModsApi.NexusModsGameId.From(120);

    public static readonly GamePath ScriptExtenderLoader = new(LocationId.Game, "fose_loader.exe");

    public Fallout3(IServiceProvider provider) : base(provider) { }

    protected override string ConfigFolderName => "Fallout3";
    protected override RelativePath ExecutableName => "Fallout3.exe";
    protected override string ScriptExtenderFolder => "fose";
    protected override string[] GameAliases => ["Fallout 3", "Fallout3", "FO3"];

    public override StoreIdentifiers StoreIdentifiers { get; } = new(GameId)
    {
        // Fallout 3, Fallout 3: Game of the Year Edition
        SteamAppIds = [22300u, 22370u],
    };

    public override IStreamFactory IconImage { get; } = new EmbeddedResourceStreamFactory<Fallout3>("NexusMods.Games.CreationEngine.Resources.Fallout3.thumbnail.webp");
    public override IStreamFactory TileImage { get; } = new EmbeddedResourceStreamFactory<Fallout3>("NexusMods.Games.CreationEngine.Resources.Fallout3.tile.webp");

    public override ModKey[] OfficialPlugins { get; } =
    [
        ModKey.FromFileName("Fallout3.esm"),
        ModKey.FromFileName("Anchorage.esm"),
        ModKey.FromFileName("ThePitt.esm"),
        ModKey.FromFileName("BrokenSteel.esm"),
        ModKey.FromFileName("PointLookout.esm"),
        ModKey.FromFileName("Zeta.esm"),
    ];
}

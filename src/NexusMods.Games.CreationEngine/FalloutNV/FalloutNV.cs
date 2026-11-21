using DynamicData.Kernel;
using Mutagen.Bethesda.Plugins;
using NexusMods.Abstractions.Games;
using NexusMods.Games.CreationEngine.Gamebryo;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.IO;

namespace NexusMods.Games.CreationEngine.FalloutNV;

public class FalloutNV : AGamebryoFalloutGame<FalloutNV>, IGameData<FalloutNV>
{
    public static GameId GameId { get; } = GameId.From("CreationEngine.FalloutNV");
    public static string DisplayName => "Fallout: New Vegas";
    public static Optional<Sdk.NexusModsApi.NexusModsGameId> NexusModsGameId => Sdk.NexusModsApi.NexusModsGameId.From(130);

    public static readonly GamePath ScriptExtenderLoader = new(LocationId.Game, "nvse_loader.exe");

    public FalloutNV(IServiceProvider provider) : base(provider) { }

    protected override string ConfigFolderName => "FalloutNV";
    protected override RelativePath ExecutableName => "FalloutNV.exe";
    protected override string ScriptExtenderFolder => "nvse";
    protected override string[] GameAliases => ["Fallout New Vegas", "Fallout NV", "FalloutNV", "New Vegas", "FNV"];

    public override StoreIdentifiers StoreIdentifiers { get; } = new(GameId)
    {
        SteamAppIds = [22380u],
    };

    public override IStreamFactory IconImage { get; } = new EmbeddedResourceStreamFactory<FalloutNV>("NexusMods.Games.CreationEngine.Resources.FalloutNV.thumbnail.webp");
    public override IStreamFactory TileImage { get; } = new EmbeddedResourceStreamFactory<FalloutNV>("NexusMods.Games.CreationEngine.Resources.FalloutNV.tile.webp");

    public override ModKey[] OfficialPlugins { get; } =
    [
        ModKey.FromFileName("FalloutNV.esm"),
        ModKey.FromFileName("DeadMoney.esm"),
        ModKey.FromFileName("HonestHearts.esm"),
        ModKey.FromFileName("OldWorldBlues.esm"),
        ModKey.FromFileName("LonesomeRoad.esm"),
        ModKey.FromFileName("GunRunnersArsenal.esm"),
        ModKey.FromFileName("ClassicPack.esm"),
        ModKey.FromFileName("MercenaryPack.esm"),
        ModKey.FromFileName("TribalPack.esm"),
        ModKey.FromFileName("CaravanPack.esm"),
    ];
}

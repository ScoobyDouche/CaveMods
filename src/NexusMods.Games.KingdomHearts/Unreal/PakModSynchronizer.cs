using NexusMods.Abstractions.Loadouts.Synchronizers;

namespace NexusMods.Games.KingdomHearts.Unreal;

internal class PakModSynchronizer : ALoadoutSynchronizer
{
    public PakModSynchronizer(IServiceProvider provider, PakModPaths paths) : base(provider)
    {
        // Never index, back up or delete anything outside Content/Paks/~mods.
        GamePathFilter = GamePathFilters.Create(path => !paths.IsManaged(path));
    }

    protected override IGamePathFilter GamePathFilter { get; }
}

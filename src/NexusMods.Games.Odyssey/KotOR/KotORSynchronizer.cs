using NexusMods.Abstractions.Loadouts.Synchronizers;

namespace NexusMods.Games.Odyssey.KotOR;

internal class KotORSynchronizer : ALoadoutSynchronizer
{
    public KotORSynchronizer(IServiceProvider provider, KotORPaths paths) : base(provider)
    {
        // Never index, back up or delete anything outside the mod-only part of the game folder.
        GamePathFilter = GamePathFilters.Create(path => !paths.IsManaged(path));
    }

    protected override IGamePathFilter GamePathFilter { get; }
}

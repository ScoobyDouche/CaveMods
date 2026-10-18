using NexusMods.Abstractions.Loadouts.Synchronizers;

namespace NexusMods.Games.Odyssey.KotOR;

public class KotORSynchronizer : ALoadoutSynchronizer
{
    public KotORSynchronizer(IServiceProvider provider) : base(provider) { }

    // Never index, back up or delete anything outside the mod-only part of the game folder.
    protected override IGamePathFilter GamePathFilter { get; } = GamePathFilters.Create(path => !KotORPaths.IsManaged(path));
}

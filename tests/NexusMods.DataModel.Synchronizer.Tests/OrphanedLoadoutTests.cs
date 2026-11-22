using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Games.RedEngine;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Games.TestFramework;
using NexusMods.Sdk;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Loadouts;
using NexusMods.Sdk.NexusModsApi;
using Xunit.Abstractions;

namespace NexusMods.DataModel.Synchronizer.Tests;

/// <summary>
/// A loadout whose game isn't registered in this build (e.g. created by a dev build that
/// supported more games) must not break synchronizing every other loadout.
/// </summary>
public class OrphanedLoadoutTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<OrphanedLoadoutTests>(helper)
{
    [Fact]
    public async Task LoadoutForUnregisteredGameDoesNotBreakSync()
    {
        var loadout = await CreateLoadout();

        using (var tx = Connection.BeginTransaction())
        {
            var install = new GameInstallMetadata.New(tx)
            {
                GameId = NexusModsGameId.From(999_999),
                Name = "Unregistered Game",
                Store = GameStore.Steam,
                Path = "/nonexistent/unregistered",
            };
            _ = new Loadout.New(tx)
            {
                Name = "Orphaned",
                ShortName = "O",
                InstallationId = install,
                LoadoutKind = LoadoutKind.Default,
                Revision = 0,
                LocatorIds = [],
                GameVersion = VanityVersion.From("1.0"),
            };
            await tx.Commit();
        }

        var act = async () => await Synchronizer.Synchronize(loadout.Rebase());
        await act.Should().NotThrowAsync();
    }
}

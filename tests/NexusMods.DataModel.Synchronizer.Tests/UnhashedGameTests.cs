using System.Text;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Backend;
using NexusMods.CrossPlatform;
using NexusMods.Games.Generic;
using NexusMods.Games.RedEngine;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Games.TestFramework;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Loadouts;
using NexusMods.StandardGameLocators.TestHelpers;
using Xunit.Abstractions;

namespace NexusMods.DataModel.Synchronizer.Tests;

/// <summary>
/// Games missing from the file-hash database (FO3, FNV, KOTOR, ...) must not have their
/// whole install ingested as External Changes, and unmanaging must not delete the game.
/// </summary>
public class UnhashedGameTests(ITestOutputHelper helper) : AIsolatedGameTest<UnhashedGameTests, Cyberpunk2077Game>(helper)
{
    private static readonly RelativePath VanillaFile = "bin/vanilla.dat";
    private static readonly RelativePath VanillaFile2 = "data/vanilla.esm";

    protected override IServiceCollection AddServices(IServiceCollection services)
    {
        return base.AddServices(services)
            .AddOSInterop()
            .AddRuntimeDependencies()
            .AddGenericGameSupport()
            .AddUniversalGameLocator<Cyberpunk2077Game>(
                new Version("1.61"),
                gameFiles: new Dictionary<RelativePath, byte[]>
                {
                    [VanillaFile] = "vanilla binary"u8.ToArray(),
                    [VanillaFile2] = "vanilla master"u8.ToArray(),
                },
                locatorIds: [LocatorId.From("not-in-hash-db")])
            .AddRedEngineGames();
    }

    private LoadoutItemWithTargetPath.ReadOnly[] ExternalChanges(Loadout.ReadOnly loadout)
    {
        loadout = loadout.Rebase();
        var group = LoadoutOverridesGroup.FindByOverridesFor(loadout.Db, loadout.Id).FirstOrDefault();
        if (!group.IsValid()) return [];
        return group.AsLoadoutItemGroup().Children.OfTypeLoadoutItemWithTargetPath().ToArray();
    }

    [Fact]
    public async Task ExistingInstallIsTreatedAsVanilla()
    {
        var loadout = await CreateLoadout();

        ExternalChanges(loadout).Should().BeEmpty("the existing install is the vanilla baseline, not user changes");

        var recorded = LocalGameFile.FindByGame(Connection.Db, loadout.InstallationId)
            .Select(f => ((GamePath)f.Path).Path)
            .ToArray();
        recorded.Should().Contain([VanillaFile, VanillaFile2]);
    }

    [Fact]
    public async Task NewFilesStillBecomeExternalChanges()
    {
        var loadout = await CreateLoadout();

        var newFile = new GamePath(LocationId.Game, "bin/user_added.txt");
        var full = GameInstallation.Locations.ToAbsolutePath(newFile);
        full.Parent.CreateDirectory();
        await full.WriteAllTextAsync("added after managing");

        loadout = await Synchronizer.Synchronize(loadout);

        ExternalChanges(loadout)
            .Select(i => ((GamePath)i.TargetPath).Path)
            .Should().BeEquivalentTo([newFile.Path]);
    }

    [Fact]
    public async Task UnmanageKeepsVanillaFiles()
    {
        var loadout = await CreateLoadout();

        var newFile = new GamePath(LocationId.Game, "bin/user_added.txt");
        var full = GameInstallation.Locations.ToAbsolutePath(newFile);
        full.Parent.CreateDirectory();
        await full.WriteAllTextAsync("added after managing");
        await Synchronizer.Synchronize(loadout);

        await LoadoutManager.UnManage(GameInstallation);

        GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, VanillaFile)).FileExists.Should().BeTrue();
        GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, VanillaFile2)).FileExists.Should().BeTrue();
        full.FileExists.Should().BeFalse("files added while managed are removed when resetting to the original state");
    }

    [Fact]
    public async Task DiagnosticsCanReadVanillaFiles()
    {
        // e.g. the Gamebryo missing-master check reads vanilla .esm files through GameFileStreamSource
        var loadout = await CreateLoadout();

        var entry = DiskStateEntry.All(Connection.Db)
            .First(e => e.GameId == loadout.InstallationId && e.Path.Item3 == VanillaFile2);
        var source = ServiceProvider.GetServices<NexusMods.Sdk.FileStore.IReadOnlyStreamSource>()
            .OfType<GameFileStreamSource>().Single();

        source.Resolve(entry.Hash).Should().Be(
            GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, VanillaFile2)));
    }

    [Fact]
    public async Task DiagnosticsCanReadVanillaFilesBeforeFirstSync()
    {
        var loadout = await CreateLoadout();
        using (var tx = Connection.BeginTransaction())
        {
            // an install that was indexed but never synced has no LastSyncedLoadout
            var metadata = GameInstallMetadata.Load(Connection.Db, loadout.InstallationId);
            tx.Retract(metadata, GameInstallMetadata.LastSyncedLoadoutId, metadata.LastSyncedLoadoutId.Value);
            await tx.Commit();
        }

        var entry = DiskStateEntry.All(Connection.Db)
            .First(e => e.GameId == loadout.InstallationId && e.Path.Item3 == VanillaFile2);
        var source = ServiceProvider.GetServices<NexusMods.Sdk.FileStore.IReadOnlyStreamSource>()
            .OfType<GameFileStreamSource>().Single();

        source.Resolve(entry.Hash).Should().Be(
            GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, VanillaFile2)));
    }
}

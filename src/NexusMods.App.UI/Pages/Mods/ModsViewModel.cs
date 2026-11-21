using NexusMods.Abstractions.Loadouts.Extensions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.GC;
using NexusMods.Abstractions.Library;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.NexusModsLibrary;
using NexusMods.Abstractions.NexusModsLibrary.Models;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Abstractions.NexusWebApi;
using NexusMods.App.UI.Dialog;
using NexusMods.App.UI.Dialog.Enums;
using NexusMods.App.UI.Pages.LoadoutPage;
using NexusMods.App.UI.Windows;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Library;
using NexusMods.Sdk.Loadouts;
using NexusMods.UI.Sdk.Dialog;
using NexusMods.UI.Sdk.Dialog.Enums;
using NexusMods.UI.Sdk.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Pages.Mods;

public class ModsViewModel : APageViewModel<IModsViewModel>, IModsViewModel
{
    private const string NotInstalled = " (not installed)";
    private const string OtherFiles = "Other files";

    private readonly IConnection _conn;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWindowManager _windowManager;
    private readonly IGameRegistry _gameRegistry;

    // Collapsed by default; remembers what the user expanded for as long as the page lives
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> _expanded = new();

    [Reactive] public IReadOnlyList<ModGroup> Downloaded { get; private set; } = [];
    [Reactive] public IReadOnlyList<ModGroup> Installed { get; private set; } = [];

    public ModsViewModel(IWindowManager windowManager, IServiceProvider serviceProvider, IConnection conn, IGameRegistry gameRegistry) : base(windowManager)
    {
        _conn = conn;
        _serviceProvider = serviceProvider;
        _windowManager = windowManager;
        _gameRegistry = gameRegistry;

        TabTitle = "Mods";
        TabIcon = IconValues.ModLibrary;

        this.WhenActivated(d =>
        {
            _conn.Revisions
                .StartWith(_conn.Db)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(db => (Downloaded: BuildDownloaded(db), Installed: BuildInstalled(db)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(groups =>
                {
                    Downloaded = groups.Downloaded;
                    Installed = groups.Installed;
                })
                .DisposeWith(d);
        });
    }

    private IReadOnlyList<ModGroup> BuildDownloaded(IDb db)
    {
        var installedGameIds = _gameRegistry.LocateGameInstallations()
            .Select(i => i.Game.NexusModsGameId)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .ToHashSet();

        var gameNames = _serviceProvider.GetServices<IGameData>()
            .Where(g => g.NexusModsGameId.HasValue)
            .GroupBy(g => g.NexusModsGameId.Value)
            .ToDictionary(g => g.Key, g => g.First().DisplayName);

        var mappingCache = _serviceProvider.GetRequiredService<IGameDomainToGameIdMappingCache>();

        string GameTitle(LibraryItem.ReadOnly item)
        {
            var nexusItem = NexusModsLibraryItem.Load(db, item.Id);
            if (!nexusItem.IsValid()) return OtherFiles;

            var domain = nexusItem.ModPageMetadata.GameDomain;
            try
            {
                var gameId = mappingCache[domain];
                var name = gameNames.TryGetValue(gameId, out var n) ? n : domain.ToString();
                return installedGameIds.Contains(gameId) ? name : name + NotInstalled;
            }
            catch (Exception)
            {
                return domain.ToString();
            }
        }

        // Same items the Library page lists. Every file inside an archive is also a LibraryItem
        // (LibraryArchiveFileEntry), so LibraryItem.All would list thousands of extracted files.
        bool IsDownload(LibraryItem.ReadOnly item) =>
            NexusModsLibraryItem.Load(db, item.Id).IsValid()
            || LocalFile.Load(db, item.Id).IsValid()
            || ManuallyCreatedArchive.Load(db, item.Id).IsValid();

        return LibraryItem.All(db)
            .Where(IsDownload)
            .Select(item => (Item: item, Game: GameTitle(item)))
            .GroupBy(row => row.Game)
            .OrderBy(g => g.Key == OtherFiles).ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ModGroup(
                "downloaded:" + g.Key,
                g.Key,
                g.OrderBy(row => row.Item.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(row =>
                    {
                        var file = LibraryFile.Load(db, row.Item.Id);
                        var detail = file.IsValid() ? file.Size.ToString() : "";
                        var itemId = row.Item.LibraryItemId;
                        var itemName = row.Item.Name;
                        return new ModEntry(itemName, detail, "Delete", ReactiveCommand.CreateFromTask(() => DeleteDownload(itemId, itemName)));
                    })
                    .ToArray(),
                _expanded))
            .ToArray();
    }

    private IReadOnlyList<ModGroup> BuildInstalled(IDb db)
    {
        return Loadout.All(db)
            .Where(l => l.IsVisible())
            .Select(loadout =>
            {
                var isInstalled = _gameRegistry.TryGetGameInstallation(loadout, out _);
                var title = $"{loadout.Installation.Name}{(isInstalled ? "" : NotInstalled)} - {loadout.Name}";

                // top-level mods are the direct children of a collection group ("My Mods" or a collection)
                var mods = LoadoutItem.FindByLoadout(db, loadout.LoadoutId)
                    .Where(item => LoadoutItem.Parent.TryGetValue(item, out var parentId) && CollectionGroup.Load(db, parentId).IsValid())
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(item =>
                    {
                        var collection = item.Parent.AsLoadoutItem().Name;
                        var detail = (item.IsEnabled() ? "Enabled" : "Disabled") + (collection == "My Mods" ? "" : $" · {collection}");
                        if (!isInstalled) return new ModEntry(item.Name, detail);

                        var itemId = item.LoadoutItemId;
                        return new ModEntry(item.Name, detail, "Uninstall",
                            ReactiveCommand.CreateFromTask(() => LoadoutViewModel.HandleUninstallItem([itemId], _windowManager, _conn)));
                    })
                    .ToArray();

                var loadoutId = loadout.LoadoutId;
                var gameName = loadout.Installation.Name;
                var loadoutName = loadout.Name;
                var key = "installed:" + loadoutId.Value;
                return isInstalled
                    ? new ModGroup(key, title, mods, _expanded)
                    : new ModGroup(key, title, mods, _expanded, "Remove loadout", ReactiveCommand.CreateFromTask(() => RemoveUninstalledLoadout(loadoutId, gameName, loadoutName, mods.Length)));
            })
            // only loadouts that actually have mods installed
            .Where(g => g.Mods.Count > 0)
            .OrderBy(g => g.HasAction).ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<bool> Confirm(string title, string text, string confirmText)
    {
        var confirm = ButtonDefinitionId.From("confirm");
        var dialog = DialogFactory.CreateStandardDialog(
            title,
            new StandardDialogParameters { Text = text },
            [
                new DialogButtonDefinition("Cancel", ButtonDefinitionId.Cancel, ButtonAction.Reject),
                new DialogButtonDefinition(confirmText, confirm, ButtonAction.Accept, ButtonStyling.Destructive),
            ]
        );
        var result = await _windowManager.ShowDialog(dialog, DialogWindowType.Modal);
        return result.ButtonId == confirm;
    }

    private async Task DeleteDownload(LibraryItemId itemId, string name)
    {
        var item = LibraryItem.Load(_conn.Db, itemId);
        if (!item.IsValid()) return;

        if (!await Confirm($"Delete \"{name}\"?",
                "The downloaded archive is deleted from your Library. It's also uninstalled from any loadout that uses it.",
                "Delete"))
            return;

        var libraryService = _serviceProvider.GetRequiredService<ILibraryService>();
        await Task.Run(() => libraryService.RemoveLibraryItems([item]));
    }

    private async Task RemoveUninstalledLoadout(LoadoutId loadoutId, string gameName, string loadoutName, int modCount)
    {
        if (!await Confirm($"Remove {gameName} loadout?",
                $"""
                "{loadoutName}" and its {modCount} mod(s) will be removed from CaveMods.

                {gameName} isn't installed, so no game files are touched. Downloaded archives stay in your Library.
                """,
                "Remove loadout"))
            return;

        // deactivation needs the installation, which no longer exists
        var loadoutManager = _serviceProvider.GetRequiredService<ILoadoutManager>();
        await Task.Run(async () => await loadoutManager.DeleteLoadout(loadoutId, GarbageCollectorRunMode.RunAsyncInBackground, deactivateIfActive: false));
    }
}

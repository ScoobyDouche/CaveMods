using System.Reactive;
using NexusMods.App.UI.WorkspaceSystem;
using ReactiveUI;

namespace NexusMods.App.UI.Pages.Mods;

public interface IModsViewModel : IPageViewModelInterface
{
    /// <summary>
    /// Downloaded archives in the library, grouped by game.
    /// </summary>
    IReadOnlyList<ModGroup> Downloaded { get; }

    /// <summary>
    /// Mods installed in loadouts, grouped by game + loadout (including games that are no longer installed).
    /// </summary>
    IReadOnlyList<ModGroup> Installed { get; }
}

/// <summary>
/// A titled group of mods, optionally with an action for the whole group (e.g. removing a loadout).
/// Groups are rebuilt whenever the database changes, so the expanded state lives in a store
/// shared by the view model; that keeps it across rebuilds and tab switches.
/// </summary>
public class ModGroup(string key, string title, IReadOnlyList<ModEntry> mods, IDictionary<string, bool> expandedStore, string? actionText = null, ReactiveCommand<Unit, Unit>? action = null)
{
    public string Title { get; } = title;
    public IReadOnlyList<ModEntry> Mods { get; } = mods;
    public string? ActionText { get; } = actionText;
    public ReactiveCommand<Unit, Unit>? Action { get; } = action;

    public bool HasAction => Action is not null;
    public string CountText => Mods.Count == 1 ? "1 mod" : $"{Mods.Count} mods";

    public bool IsExpanded
    {
        get => expandedStore.TryGetValue(key, out var expanded) && expanded;
        set => expandedStore[key] = value;
    }
}

public record ModEntry(string Name, string Detail, string? ActionText = null, ReactiveCommand<Unit, Unit>? Action = null)
{
    public bool HasAction => Action is not null;
}

using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace NexusMods.Games.CreationEngine.Abstractions;

/// <summary>
/// The parts of a plugin header that the app cares about: its own key and the masters it depends on.
/// </summary>
/// <remarks>
/// This is game-neutral so that games Mutagen can't parse (Fallout 3, New Vegas) can still take part
/// in load order sorting and missing master diagnostics.
/// </remarks>
public sealed record PluginHeader(ModKey ModKey, IReadOnlyList<ModKey> Masters)
{
    /// <summary>
    /// Builds a header from a plugin parsed by Mutagen.
    /// </summary>
    public static PluginHeader FromMutagen(IModGetter mod)
        => new(mod.ModKey, mod.MasterReferences.Select(m => m.Master).ToArray());
}

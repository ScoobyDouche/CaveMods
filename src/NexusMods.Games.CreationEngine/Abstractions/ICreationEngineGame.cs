using Mutagen.Bethesda.Plugins;
using NexusMods.Abstractions.Games;
using NexusMods.Hashing.xxHash3;
using NexusMods.Paths;
using NexusMods.Sdk.Games;

namespace NexusMods.Games.CreationEngine.Abstractions;

/// <summary>
/// A common interface for all games that use the creation engine (or its Gamebryo predecessor).
/// </summary>
public interface ICreationEngineGame : IGame
{
    public GamePath PluginsFile { get; }

    /// <summary>
    /// Parse a plugin file's header.
    /// </summary>
    public ValueTask<PluginHeader?> ParsePlugin(Hash hash, RelativePath? name = null);

    /// <summary>
    /// The format of the plugins file and how load order is applied.
    /// </summary>
    public PluginListFormat PluginListFormat => PluginListFormat.Asterisk;

    /// <summary>
    /// The base game and official DLC plugins, in the order the game expects to load them.
    /// These always load before any modded plugins.
    /// </summary>
    public ModKey[] OfficialPlugins => [];
}

/// <summary>
/// How a game reads its plugins file.
/// </summary>
public enum PluginListFormat
{
    /// <summary>
    /// Skyrim SE / Fallout 4 style: active plugins are prefixed with <c>*</c>, and line order is load order.
    /// </summary>
    Asterisk,

    /// <summary>
    /// Fallout 3 / New Vegas style: every listed plugin is active, and load order comes from the
    /// plugin files' modification times (oldest loads first).
    /// </summary>
    TimestampOrdered,
}

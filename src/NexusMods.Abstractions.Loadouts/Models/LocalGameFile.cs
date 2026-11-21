using NexusMods.Hashing.xxHash3;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Paths;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.Models;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Hashes;

namespace NexusMods.Abstractions.Loadouts;

/// <summary>
/// A vanilla game file recorded from disk. Used as the game's baseline when the
/// Nexus file-hash database has no entries for the game (e.g. games the upstream
/// app never supported), so the existing install isn't treated as External Changes.
/// </summary>
public partial class LocalGameFile : IModelDefinition
{
    private const string Namespace = "CaveMods.Loadouts.LocalGameFile";

    /// <summary>
    /// The path of the file, parented to the game installation.
    /// </summary>
    public static readonly GamePathParentAttribute Path = new(Namespace, nameof(Path));

    /// <summary>
    /// The hash of the file.
    /// </summary>
    public static readonly HashAttribute Hash = new(Namespace, nameof(Hash));

    /// <summary>
    /// The size of the file (in bytes).
    /// </summary>
    public static readonly SizeAttribute Size = new(Namespace, nameof(Size));

    /// <summary>
    /// The owning game installation.
    /// </summary>
    public static readonly ReferenceAttribute<GameInstallMetadata> Game = new(Namespace, nameof(Game)) { IsIndexed = true };
}

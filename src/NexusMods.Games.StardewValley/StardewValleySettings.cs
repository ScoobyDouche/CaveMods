using NexusMods.Sdk.Settings;

namespace NexusMods.Games.StardewValley;

public class StardewValleySettings : ISettings
{
    /// <summary>
    /// If true, the contents of the Content folder will not be backed up. If the game updates
    /// the loadout may become invalid. If mods are installed into this folder via the app they
    /// will still be backed up as needed
    /// </summary>
    // CaveMods removed the full game backup option: it is always off.
    public bool DoFullGameBackup { get => false; set { } }

    public static ISettingsBuilder Configure(ISettingsBuilder settingsBuilder)
    {
        return settingsBuilder;
    }
}

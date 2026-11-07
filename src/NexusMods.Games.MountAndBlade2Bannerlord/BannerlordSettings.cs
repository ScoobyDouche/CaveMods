
using NexusMods.Sdk.Settings;

namespace NexusMods.Games.MountAndBlade2Bannerlord;

public class BannerlordSettings : ISettings
{
    // CaveMods removed the full game backup option: it is always off.
    public bool DoFullGameBackup { get => false; set { } }
    public bool BetaSorting { get; set; } = false;


    public static ISettingsBuilder Configure(ISettingsBuilder settingsBuilder)
    {
        return settingsBuilder;
    }
}

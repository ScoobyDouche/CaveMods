using JetBrains.Annotations;
using NexusMods.Sdk;
using NexusMods.Sdk.Settings;

namespace NexusMods.Backend.Games.Locators;

public record GameLocatorSettings : ISettings
{
    public bool EnableXboxGamePass { get; [UsedImplicitly] set; } = false;

    public static ISettingsBuilder Configure(ISettingsBuilder settingsBuilder)
    {
        return settingsBuilder
            .ConfigureBackend(StorageBackendOptions.Use(StorageBackends.Json));
    }
}

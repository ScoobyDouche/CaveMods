using JetBrains.Annotations;
using Mutagen.Bethesda.Plugins;
using NexusMods.Abstractions.Diagnostics;
using NexusMods.Abstractions.Diagnostics.References;
using NexusMods.Abstractions.Diagnostics.Values;
using NexusMods.Generators.Diagnostics;

namespace NexusMods.Games.CreationEngine;

internal static partial class Diagnostics
{
    private const string Source = "NexusMods.Games.CreationEngine";

    [DiagnosticTemplate]
    [UsedImplicitly]
    internal static IDiagnosticTemplate MissingMaster = DiagnosticTemplateBuilder
        .Start()
        .WithId(new DiagnosticId(Source, number: 1))
        .WithTitle("Missing Master")
        .WithSeverity(DiagnosticSeverity.Critical)
        .WithSummary("{PluginName} requires {MissingMasterPluginName} which is not installed")
        .WithDetails("""
The mod **{PluginName}** requires **{MissingMasterPluginName}** to function, but **{MissingMasterPluginName}** is not provided by any mods.

This means the plugin can’t find one of its required “master files.” Masters contain important data that other mods depend on—without them, the plugin is left pointing to records that don’t exist.

If you try to launch the game in this state, it won’t start. Bethesda games stop loading when a master is missing to prevent crashes, broken saves, or corrupted load orders. 
To fix this, you’ll need to install the missing master before playing.
""")
        .WithMessageData(messageBuilder => messageBuilder
            .AddDataReference<LoadoutItemGroupReference>("Mod")
            .AddValue<ModKey>("PluginName")
            .AddValue<ModKey>("MissingMasterPluginName")
        )
        .Finish();

    
    [DiagnosticTemplate]
    [UsedImplicitly]
    internal static IDiagnosticTemplate MissingMasterIsDisabled = DiagnosticTemplateBuilder
        .Start()
        .WithId(new DiagnosticId(Source, number: 2))
        .WithTitle("Disabled Master")
        .WithSeverity(DiagnosticSeverity.Critical)
        .WithSummary("{PluginName} requires {MissingMasterPluginName} which is installed in {MissingMasterMod} but is disabled")
        .WithDetails("""
The mod **{PluginName}** requires **{MissingMasterPluginName}** to function, but it is not provided by any enabled mods. The 
mod **{MissingMasterMod}** provides this file, but it is disabled. Enable the mod to fix this issue.

This means the plugin can’t find one of its required “master files.” Masters contain important data that other mods depend on—without them, the plugin is left pointing to records that don’t exist.

If you try to launch the game in this state, it won’t start. Bethesda games stop loading when a master is missing to prevent crashes, broken saves, or corrupted load orders. 
To fix this, you’ll need to install the missing master before playing.
""")
        .WithMessageData(messageBuilder => messageBuilder
            .AddDataReference<LoadoutItemGroupReference>("Mod")
            .AddValue<ModKey>("PluginName")
            .AddValue<ModKey>("MissingMasterPluginName")
            .AddValue<LoadoutItemGroupReference>("MissingMasterMod")
        )
        .Finish();

    [DiagnosticTemplate]
    [UsedImplicitly]
    internal static IDiagnosticTemplate ArchiveInvalidationDisabled = DiagnosticTemplateBuilder
        .Start()
        .WithId(new DiagnosticId(Source, number: 3))
        .WithTitle("Loose texture and mesh mods are ignored")
        .WithSeverity(DiagnosticSeverity.Warning)
        .WithSummary("Archive invalidation is off, so the game will ignore installed texture and mesh replacers")
        .WithDetails("""
Some of your mods replace textures or meshes with loose files. Fallout 3 and New Vegas only use loose files over the
ones packed in the game's BSA archives when **archive invalidation** is turned on, which it currently isn't.

To fix this, open **{IniFile}** in a text editor and make sure the `[Archive]` section contains:

```
bInvalidateOlderFiles=1
SInvalidationFile=
```

If the file doesn't exist yet, launch the game once from Steam so it gets created, then edit it.
""")
        .WithMessageData(messageBuilder => messageBuilder
            .AddValue<string>("IniFile")
        )
        .Finish();
}

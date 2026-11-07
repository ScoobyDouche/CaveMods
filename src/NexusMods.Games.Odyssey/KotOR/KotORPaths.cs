using System.Collections.Frozen;
using NexusMods.Paths;
using NexusMods.Sdk.Games;

namespace NexusMods.Games.Odyssey.KotOR;

/// <summary>
/// Where a KotOR install keeps its data, and the part of it the app is allowed to manage.
/// </summary>
/// <remarks>
/// KotOR isn't in the file hashes database, so the synchronizer has no list of the game's own files
/// and would treat them as stray files to delete. Instead the app only ever looks at places the
/// base game doesn't ship anything: the Override folder and .mod files in modules (vanilla only has .rim/.erf).
/// </remarks>
internal sealed class KotORPaths
{
    internal static readonly RelativePath ModulesFolder = "modules";
    internal static readonly Extension ModuleExtension = new(".mod");

    /// <summary>
    /// KotOR 1 (Windows build, run through Proton): data sits next to swkotor.exe.
    /// </summary>
    internal static readonly KotORPaths KotOR1 = new(
        dataFolder: RelativePath.Empty,
        overrideFolder: "Override",
        lowercaseNames: false,
        vanillaOverrideFiles: []);

    /// <summary>
    /// KotOR 2 native Linux build by Aspyr: the KOTOR2 binary is in the install root and all data is
    /// under "steamassets", in lower case. The engine looks files up by their lower-case name, so mod
    /// files have to be lower-cased too or they're silently ignored on a case-sensitive file system.
    /// </summary>
    internal static readonly KotORPaths KotOR2 = new(
        dataFolder: "steamassets",
        overrideFolder: "override",
        lowercaseNames: true,
        vanillaOverrideFiles: KotOR2VanillaOverrideFiles());

    private readonly RelativePath _dataFolder;
    private readonly RelativePath[] _topFolders;
    private readonly bool _lowercaseNames;
    private readonly FrozenSet<string> _vanillaOverrideFiles;

    private KotORPaths(RelativePath dataFolder, RelativePath overrideFolder, bool lowercaseNames, string[] vanillaOverrideFiles)
    {
        _dataFolder = dataFolder;
        OverrideFolder = overrideFolder;
        _topFolders = [overrideFolder, ModulesFolder];
        _lowercaseNames = lowercaseNames;
        _vanillaOverrideFiles = vanillaOverrideFiles.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The Override folder's name as the game spells it.
    /// </summary>
    internal RelativePath OverrideFolder { get; }

    /// <summary>
    /// The folder (relative to the game root) that holds Override and modules.
    /// </summary>
    internal RelativePath DataFolder => _dataFolder;

    /// <summary>
    /// The game's Override folder, relative to the game root.
    /// </summary>
    internal RelativePath OverridePath => _dataFolder.Join(OverrideFolder);

    internal bool IsManaged(GamePath path)
    {
        if (path.LocationId != LocationId.Game) return false;

        if (!path.Path.StartsWith(_dataFolder)) return false;

        var rest = path.Path.RelativeTo(_dataFolder).Parts.ToArray();
        if (rest.Length < 2) return false;

        if (rest[0].Equals(OverrideFolder))
            return !(rest.Length == 2 && _vanillaOverrideFiles.Contains(rest[1].ToString()));

        return rest.Length == 2 && rest[0].Equals(ModulesFolder) && path.Path.Extension.Equals(ModuleExtension);
    }

    /// <summary>
    /// Rewrites a path relative to the game root to the game's own spelling: the first folder under the
    /// data folder gets the game's casing (Linux file systems are case-sensitive, so "Modules" would create
    /// a second folder next to "modules"), and for games that need it everything below the data folder is lower-cased.
    /// </summary>
    internal RelativePath Normalize(RelativePath path)
    {
        if (!path.StartsWith(_dataFolder)) return path;

        var rest = path.RelativeTo(_dataFolder);
        var parts = rest.Parts.ToArray();
        if (parts.Length < 2) return path;

        foreach (var folder in _topFolders)
        {
            if (!folder.Equals(parts[0])) continue;

            var below = rest.DropFirst(1);
            if (_lowercaseNames) below = below.ToString().ToLowerInvariant();
            return _dataFolder.Join(folder).Join(below);
        }

        return path;
    }

    /// <summary>
    /// Files Aspyr's native KotOR 2 (v1.0.2) ships in steamassets/override: controller UI and a few fixes.
    /// They must never be managed or the synchronizer would delete them.
    /// </summary>
    private static string[] KotOR2VanillaOverrideFiles() =>
    [
        "cntrl_ps3_eng.tga", "cntrl_ps3_fre.tga", "cntrl_ps3_ger.tga", "cntrl_ps3_ita.tga", "cntrl_ps3_spa.tga",
        "cntrl_xb360_eng.tga", "cntrl_xb360_fre.tga", "cntrl_xb360_ger.tga", "cntrl_xb360_ita.tga", "cntrl_xb360_spa.tga",
        "cus_button_a.tga", "cus_button_aps.tga", "cus_button_b.tga", "cus_button_bps.tga",
        "cus_button_x.tga", "cus_button_xps.tga", "cus_button_y.tga", "cus_button_yps.tga",
        "cus_gpad_bg.tga", "cus_gpad_fper.tga", "cus_gpad_fper2.tga", "cus_gpad_gen.tga", "cus_gpad_gen2.tga",
        "cus_gpad_hand.tga", "cus_gpad_hand2.tga", "cus_gpad_help.tga", "cus_gpad_help2.tga",
        "cus_gpad_map.tga", "cus_gpad_map2.tga", "cus_gpad_save.tga", "cus_gpad_save2.tga",
        "cus_gpad_solo.tga", "cus_gpad_solo2.tga", "cus_gpad_solox.tga", "cus_gpad_solox2.tga",
        "cus_gpad_ste.tga", "cus_gpad_ste2.tga", "cus_gpad_ste3.tga",
        "custom.txt", "custpnl_p.gui",
        "d2xfnt_d16x16b.tga", "d2xfont16x16b.tga", "d2xfont16x16b_ps.tga",
        "d3xfnt_d16x16b.tga", "d3xfont16x16b.tga", "d3xfont16x16b_ps.tga",
        "diafnt16x16b_ps.tga", "dialogfont16x16b.tga",
        "equip_p.gui", "fx_step_splash.mdl", "gamepad.txt", "gui_scroll.wav", "handmaiden.dlg",
        "lbl_miscroll_open_f.tga", "lbl_miscroll_open_f2.tga",
        "mainmenu8x6_p.gui", "optgraphics_p.gui", "saveload_p.gui", "v_damagetat.mdl", "w_rockpoisn.mdl",
    ];
}

using System.Threading;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.System;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class ClassicContextMenuTweak : IDebloatTweak
{
    public const string TweakId = "visual.classicContextMenu";

    private const string Hive = "HKCU";
    private const string SubKey = "Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32";

    public string Id => TweakId;
    public string Title => "Classic Right-Click Menu";
    public string Description => "Enables the classic context menu (Windows 11). Explorer restart may be required.";

    public DebloatCategory Category => DebloatCategory.Visual;
    public DebloatSeverity Severity => DebloatSeverity.Caution;

    public bool IsAdminRequired => false;
    public bool SupportsUndo => true;

    public Task<DebloatTweakStatus> GetStatusAsync(CancellationToken token = default)
    {
        var exists = Registry.KeyExists(Hive, SubKey);
        return Task.FromResult(exists ? DebloatTweakStatus.Applied : DebloatTweakStatus.NotApplied);
    }

    public Task ApplyAsync(CancellationToken token = default)
    {
        // Default value empty string.
        Registry.SetValue(Hive, SubKey, string.Empty, string.Empty, fixPermissions: false);
        return Task.CompletedTask;
    }

    public Task UndoAsync(CancellationToken token = default)
    {
        Registry.Delete(Hive, SubKey);
        return Task.CompletedTask;
    }
}

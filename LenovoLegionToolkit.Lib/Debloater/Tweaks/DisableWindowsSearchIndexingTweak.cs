using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableWindowsSearchIndexingTweak : RegistryTweakBase
{
    public const string TweakId = "performance.disableSearchIndexing";

    public override string Id => TweakId;
    public override string Title => "Disable Windows Search Indexing";
    public override string Description => "Stops Windows Search service from indexing files in the background, reducing disk/CPU usage.";

    public override DebloatCategory Category => DebloatCategory.Performance;
    public override DebloatSeverity Severity => DebloatSeverity.Caution;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", "SYSTEM\\CurrentControlSet\\Services\\WSearch", "Start", 4, 2, RegistryValueKind.DWord),
    ];
}

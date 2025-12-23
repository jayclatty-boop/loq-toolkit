using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableSuperfetchTweak : RegistryTweakBase
{
    public const string TweakId = "performance.disableSuperfetch";

    public override string Id => TweakId;
    public override string Title => "Disable Superfetch/SysMain";
    public override string Description => "Disables the Superfetch (SysMain) service. May improve performance on SSDs.";

    public override DebloatCategory Category => DebloatCategory.Performance;
    public override DebloatSeverity Severity => DebloatSeverity.Caution;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", "SYSTEM\\CurrentControlSet\\Services\\SysMain", "Start", 4, 2, RegistryValueKind.DWord),
    ];
}

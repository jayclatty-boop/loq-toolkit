using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableWindowsUpdateTweak : RegistryTweakBase
{
    public const string TweakId = "security.disableWindowsUpdate";

    public override string Id => TweakId;
    public override string Title => "Disable Windows Update";
    public override string Description => "Stops Windows Update service and disables automatic system updates. DANGEROUS: Leaves your system vulnerable to security patches. Only disable if managing updates manually.";

    public override DebloatCategory Category => DebloatCategory.Advanced;
    public override DebloatSeverity Severity => DebloatSeverity.Dangerous;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, 0, RegistryValueKind.DWord),
        ("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "DisableWindowsUpdateAccess", 1, 0, RegistryValueKind.DWord),
    ];
}

using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableXboxServicesTweak : RegistryTweakBase
{
    public const string TweakId = "services.disableXboxServices";

    public override string Id => TweakId;
    public override string Title => "Disable Xbox Services";
    public override string Description => "Disables Xbox Live authentication, networking, and background services. Xbox App and Game Pass remain installed but with reduced background activity.";

    public override DebloatCategory Category => DebloatCategory.Services;
    public override DebloatSeverity Severity => DebloatSeverity.Caution;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", @"SYSTEM\CurrentControlSet\Services\XblAuthManager", "Start", 4, 2, RegistryValueKind.DWord),
        ("HKLM", @"SYSTEM\CurrentControlSet\Services\XblGameSave", "Start", 4, 2, RegistryValueKind.DWord),
        ("HKLM", @"SYSTEM\CurrentControlSet\Services\XboxNetApiSvc", "Start", 4, 2, RegistryValueKind.DWord),
    ];
}

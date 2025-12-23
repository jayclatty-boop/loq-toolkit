using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableAdvertisingIdTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableAdvertisingId";

    public override string Id => TweakId;
    public override string Title => "Disable Advertising ID";
    public override string Description => "Turns off the per-user advertising ID.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKCU", "Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo", "Enabled", 0, 1, RegistryValueKind.DWord),
    ];
}

using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableWindowsSpotlightTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableWindowsSpotlight";

    public override string Id => TweakId;
    public override string Title => "Disable Windows Spotlight";
    public override string Description => "Disables Windows Spotlight on lock screen and personalized tips.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "RotatingLockScreenEnabled", 0, 1, RegistryValueKind.DWord),
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, 1, RegistryValueKind.DWord),
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "SubscribedContent-338387Enabled", 0, 1, RegistryValueKind.DWord),
    ];
}

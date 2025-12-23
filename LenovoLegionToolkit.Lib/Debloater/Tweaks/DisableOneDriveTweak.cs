using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableOneDriveTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableOneDrive";

    public override string Id => TweakId;
    public override string Title => "Disable OneDrive";
    public override string Description => "Prevents OneDrive from starting automatically and showing in File Explorer.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Caution;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Windows\\OneDrive", "DisableFileSyncNGSC", 1, 0, RegistryValueKind.DWord),
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowSyncProviderNotifications", 0, 1, RegistryValueKind.DWord),
    ];
}

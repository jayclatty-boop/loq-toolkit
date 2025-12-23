using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableActivityHistoryTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableActivityHistory";

    public override string Id => TweakId;
    public override string Title => "Disable Activity History";
    public override string Description => "Disables Windows activity feed / user activity publishing policies.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Caution;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Windows\\System", "EnableActivityFeed", 0, 1, RegistryValueKind.DWord),
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Windows\\System", "PublishUserActivities", 0, 1, RegistryValueKind.DWord),
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Windows\\System", "UploadUserActivities", 0, 1, RegistryValueKind.DWord),
    ];
}

using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableGameDvrTweak : RegistryTweakBase
{
    public const string TweakId = "performance.disableGameDvr";

    public override string Id => TweakId;
    public override string Title => "Disable Game DVR / Captures";
    public override string Description => "Disables Xbox Game Bar capture features that can impact performance.";

    public override DebloatCategory Category => DebloatCategory.Performance;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKCU", "System\\GameConfigStore", "GameDVR_Enabled", 0, 1, RegistryValueKind.DWord),
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Windows\\GameDVR", "AllowGameDVR", 0, 1, RegistryValueKind.DWord),
    ];

    public override bool IsAdminRequired => true; // HKLM entry
}

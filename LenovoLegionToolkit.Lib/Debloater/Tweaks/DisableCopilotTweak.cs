using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableCopilotTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableCopilot";

    public override string Id => TweakId;
    public override string Title => "Disable Copilot";
    public override string Description => "Disables Windows Copilot AI assistant in Windows 11 23H2+. Has no effect on older Windows versions. Safe to apply.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, 0, RegistryValueKind.DWord),
    ];
}

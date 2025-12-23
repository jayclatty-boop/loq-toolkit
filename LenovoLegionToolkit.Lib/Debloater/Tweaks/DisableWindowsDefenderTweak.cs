using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableWindowsDefenderTweak : RegistryTweakBase
{
    public const string TweakId = "security.disableDefender";

    public override string Id => TweakId;
    public override string Title => "Disable Windows Defender";
    public override string Description => "Disables Windows Defender real-time scanning and cloud protection. DANGEROUS: Requires active antivirus solution. System will be unprotected if no alternative antivirus is running.";

    public override DebloatCategory Category => DebloatCategory.Advanced;
    public override DebloatSeverity Severity => DebloatSeverity.Dangerous;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", @"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", 1, 0, RegistryValueKind.DWord),
        ("HKLM", @"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", "SpyNetReporting", 0, 2, RegistryValueKind.DWord),
    ];
}

using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableCortanaTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableCortana";

    public override string Id => TweakId;
    public override string Title => "Disable Cortana";
    public override string Description => "Disables Cortana voice assistant integration.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Caution;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search", "AllowCortana", 0, 1, RegistryValueKind.DWord),
    ];
}

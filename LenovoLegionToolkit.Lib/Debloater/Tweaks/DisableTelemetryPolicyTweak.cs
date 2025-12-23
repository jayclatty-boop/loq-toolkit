using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableTelemetryPolicyTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableTelemetryPolicy";

    public override string Id => TweakId;
    public override string Title => "Disable Telemetry (Policy)";
    public override string Description => "Sets Windows policy to reduce diagnostic data collection.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Caution;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection", "AllowTelemetry", 0, 3, RegistryValueKind.DWord),
    ];
}

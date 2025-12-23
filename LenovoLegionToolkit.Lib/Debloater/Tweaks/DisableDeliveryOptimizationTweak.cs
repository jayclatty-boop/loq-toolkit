using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableDeliveryOptimizationTweak : RegistryTweakBase
{
    public const string TweakId = "services.disableDeliveryOptimization";

    public override string Id => TweakId;
    public override string Title => "Disable Delivery Optimization";
    public override string Description => "Stops Windows Delivery Optimization service (P2P update distribution). Reduces bandwidth usage but may slow Windows Update delivery.";

    public override DebloatCategory Category => DebloatCategory.Services;
    public override DebloatSeverity Severity => DebloatSeverity.Caution;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 99, 1, RegistryValueKind.DWord),
    ];
}

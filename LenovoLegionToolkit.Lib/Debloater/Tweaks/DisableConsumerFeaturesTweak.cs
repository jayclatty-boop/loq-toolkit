using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableConsumerFeaturesTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableConsumerFeatures";

    public override string Id => TweakId;
    public override string Title => "Disable Consumer Features";
    public override string Description => "Prevents automatic installation of suggested apps and consumer features.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Windows\\CloudContent", "DisableWindowsConsumerFeatures", 1, 0, RegistryValueKind.DWord),
    ];
}

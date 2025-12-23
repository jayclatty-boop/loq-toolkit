using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableCloudClipboardTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableCloudClipboard";

    public override string Id => TweakId;
    public override string Title => "Disable Cloud Clipboard";
    public override string Description => "Prevents clipboard history from syncing across devices.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKCU", "SOFTWARE\\Microsoft\\Clipboard", "EnableClipboardHistory", 0, 1, RegistryValueKind.DWord),
        ("HKCU", "SOFTWARE\\Microsoft\\Clipboard", "CloudClipboardAutomaticUpload", 0, 1, RegistryValueKind.DWord),
    ];
}

using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableWidgetsTweak : RegistryTweakBase
{
    public const string TweakId = "visual.disableWidgets";

    public override string Id => TweakId;
    public override string Title => "Disable Widgets";
    public override string Description => "Disables the Windows 11 Widgets panel and taskbar button.";

    public override DebloatCategory Category => DebloatCategory.Visual;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "TaskbarDa", 0, 1, RegistryValueKind.DWord),
        ("HKLM", "SOFTWARE\\Policies\\Microsoft\\Dsh", "AllowNewsAndInterests", 0, 1, RegistryValueKind.DWord),
    ];
}

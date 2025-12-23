using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableTaskViewButtonTweak : RegistryTweakBase
{
    public const string TweakId = "visual.disableTaskViewButton";

    public override string Id => TweakId;
    public override string Title => "Hide Task View Button";
    public override string Description => "Removes the Task View button from the taskbar.";

    public override DebloatCategory Category => DebloatCategory.Visual;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowTaskViewButton", 0, 1, RegistryValueKind.DWord),
    ];
}

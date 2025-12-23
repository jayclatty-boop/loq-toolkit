using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableStartupDelayTweak : RegistryTweakBase
{
    public const string TweakId = "performance.disableStartupDelay";

    public override string Id => TweakId;
    public override string Title => "Disable Startup Delay";
    public override string Description => "Removes the 10-second delay for startup programs, improving boot time.";

    public override DebloatCategory Category => DebloatCategory.Performance;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Serialize", "StartupDelayInMSec", 0, 10000, RegistryValueKind.DWord),
    ];
}

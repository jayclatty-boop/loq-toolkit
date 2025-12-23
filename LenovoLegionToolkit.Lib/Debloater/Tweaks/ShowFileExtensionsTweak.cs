using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class ShowFileExtensionsTweak : RegistryTweakBase
{
    public const string TweakId = "visual.showFileExtensions";

    public override string Id => TweakId;
    public override string Title => "Show File Extensions";
    public override string Description => "Shows file extensions in File Explorer.";

    public override DebloatCategory Category => DebloatCategory.Visual;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKCU", "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "HideFileExt", 0, 1, RegistryValueKind.DWord),
    ];
}

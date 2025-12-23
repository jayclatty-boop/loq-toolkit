using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableBingSearchTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableBingSearch";

    public override string Id => TweakId;
    public override string Title => "Disable Bing Search in Start";
    public override string Description => "Disables web search suggestions in Start menu search (where supported).";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        // Windows 10 style keys
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Search", "BingSearchEnabled", 0, 1, RegistryValueKind.DWord),
        ("HKCU", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Search", "CortanaConsent", 0, 1, RegistryValueKind.DWord),

        // Windows 11 policy key
        ("HKCU", "SOFTWARE\\Policies\\Microsoft\\Windows\\Explorer", "DisableSearchBoxSuggestions", 1, 0, RegistryValueKind.DWord),
    ];
}

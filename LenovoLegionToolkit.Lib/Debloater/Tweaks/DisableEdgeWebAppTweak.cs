using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableEdgeWebAppTweak : RegistryTweakBase
{
    public const string TweakId = "privacy.disableEdgeWebApp";

    public override string Id => TweakId;
    public override string Title => "Disable Edge Web App Integration";
    public override string Description => "Disables Microsoft Edge web app integration and background task scheduling. Reduces Edge-related background activity.";

    public override DebloatCategory Category => DebloatCategory.Privacy;
    public override DebloatSeverity Severity => DebloatSeverity.Safe;

    public override bool IsAdminRequired => true;

    protected override (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries =>
    [
        ("HKLM", @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\Update", "UpdateDefault", 0, 1, RegistryValueKind.DWord),
    ];
}

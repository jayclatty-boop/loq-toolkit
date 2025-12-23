using System.Collections.Generic;
using LenovoLegionToolkit.Lib.Debloater.Tweaks;

namespace LenovoLegionToolkit.Lib.Debloater;

public static class DebloaterCatalog
{
    public static IReadOnlyList<IDebloatTweak> CreateDefaultTweaks()
    {
        return new IDebloatTweak[]
        {
            // Privacy tweaks
            new DisableTelemetryPolicyTweak(),
            new DisableAdvertisingIdTweak(),
            new DisableActivityHistoryTweak(),
            new DisableWindowsSpotlightTweak(),
            new DisableConsumerFeaturesTweak(),
            new DisableCloudClipboardTweak(),
            new DisableCortanaTweak(),
            new DisableOneDriveTweak(),
            
            // Performance tweaks
            new DisableGameDvrTweak(),
            new DisableStartupDelayTweak(),
            new DisableWindowsSearchIndexingTweak(),
            new DisableSuperfetchTweak(),
            
            // Visual tweaks
            new DisableBingSearchTweak(),
            new ShowFileExtensionsTweak(),
            new ClassicContextMenuTweak(),
            new DisableTaskViewButtonTweak(),
            new DisableWidgetsTweak(),
            
            // Services tweaks
            new DisableDiagTrackServiceTweak(),
            new DisableDeliveryOptimizationTweak(),
            new DisableXboxServicesTweak(),
            
            // Privacy - Extended
            new DisableCopilotTweak(),
            new DisableEdgeWebAppTweak(),
            
            // Advanced/Dangerous tweaks
            new DisableWindowsUpdateTweak(),
            new DisableWindowsDefenderTweak(),
            
            // Bloatware tweaks
            new RemoveBloatwareAppsTweak(),
        };
    }

    public static IReadOnlyList<DebloatProfile> CreateDefaultProfiles()
    {
        // Minimal set of profiles, per your request.
        return new[]
        {
            new DebloatProfile(
                Id: "recommended",
                Title: "Recommended",
                Description: "Safe privacy + quality-of-life tweaks.",
                TweakIds: new[]
                {
                    DisableAdvertisingIdTweak.TweakId,
                    DisableActivityHistoryTweak.TweakId,
                    DisableGameDvrTweak.TweakId,
                    DisableBingSearchTweak.TweakId,
                    ShowFileExtensionsTweak.TweakId,
                    DisableWindowsSpotlightTweak.TweakId,
                    DisableConsumerFeaturesTweak.TweakId,
                    DisableStartupDelayTweak.TweakId,
                    DisableTaskViewButtonTweak.TweakId,
                    DisableCopilotTweak.TweakId,
                    DisableDeliveryOptimizationTweak.TweakId,
                }
            ),
            new DebloatProfile(
                Id: "privacy",
                Title: "Privacy Focused",
                Description: "Maximum privacy with telemetry and cloud features disabled.",
                TweakIds: new[]
                {
                    DisableTelemetryPolicyTweak.TweakId,
                    DisableAdvertisingIdTweak.TweakId,
                    DisableActivityHistoryTweak.TweakId,
                    DisableBingSearchTweak.TweakId,
                    DisableWindowsSpotlightTweak.TweakId,
                    DisableConsumerFeaturesTweak.TweakId,
                    DisableCloudClipboardTweak.TweakId,
                    DisableCortanaTweak.TweakId,
                    DisableOneDriveTweak.TweakId,
                    DisableCopilotTweak.TweakId,
                    DisableEdgeWebAppTweak.TweakId,
                    DisableDeliveryOptimizationTweak.TweakId,
                }
            ),
            new DebloatProfile(
                Id: "performance",
                Title: "Performance Focused",
                Description: "Disables background services and indexing for maximum performance.",
                TweakIds: new[]
                {
                    DisableGameDvrTweak.TweakId,
                    DisableDiagTrackServiceTweak.TweakId,
                    DisableStartupDelayTweak.TweakId,
                    DisableWindowsSearchIndexingTweak.TweakId,
                    DisableSuperfetchTweak.TweakId,
                    DisableDeliveryOptimizationTweak.TweakId,
                    DisableXboxServicesTweak.TweakId,
                }
            ),
        };
    }
}

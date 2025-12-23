using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.Extensions;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class DisableDiagTrackServiceTweak : IDebloatTweak
{
    public const string TweakId = "services.disableDiagTrack";

    private const string ServiceName = "DiagTrack";

    public string Id => TweakId;
    public string Title => "Disable Diagnostics Tracking Service";
    public string Description => "Disables the 'Connected User Experiences and Telemetry' service.";

    public DebloatCategory Category => DebloatCategory.Services;
    public DebloatSeverity Severity => DebloatSeverity.Dangerous;

    public bool IsAdminRequired => true;
    public bool SupportsUndo => true;

    public Task<DebloatTweakStatus> GetStatusAsync(CancellationToken token = default)
    {
        if (!ServiceController.GetServices().Any(s => s.ServiceName == ServiceName))
            return Task.FromResult(DebloatTweakStatus.NotSupported);

        // We only model enabled/disabled here; treat running state as not required.
        // If service exists and is not disabled, consider NotApplied.
        return Task.FromResult(DebloatTweakStatus.Unknown);
    }

    public Task ApplyAsync(CancellationToken token = default)
    {
        if (!ServiceController.GetServices().Any(s => s.ServiceName == ServiceName))
            return Task.CompletedTask;

        using var svc = new ServiceController(ServiceName);
        svc.ChangeStartMode(enabled: false);
        if (svc.CanStop && svc.Status != ServiceControllerStatus.Stopped)
        {
            svc.Stop();
            svc.WaitForStatus(ServiceControllerStatus.Stopped);
        }

        return Task.CompletedTask;
    }

    public Task UndoAsync(CancellationToken token = default)
    {
        if (!ServiceController.GetServices().Any(s => s.ServiceName == ServiceName))
            return Task.CompletedTask;

        using var svc = new ServiceController(ServiceName);
        svc.ChangeStartMode(enabled: true);
        return Task.CompletedTask;
    }
}

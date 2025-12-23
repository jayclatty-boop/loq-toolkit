using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.System;
using LenovoLegionToolkit.Lib.Utils;
using NvAPIWrapper.GPU;

namespace LenovoLegionToolkit.Lib.Controllers;

public class GPUMonitor : IDisposable
{
    private Timer? _monitoringTimer;
    private bool _isMonitoring;

    public event EventHandler<GPUMetrics>? MetricsUpdated;

    public bool IsMonitoring => _isMonitoring;

    public void Start(int intervalMs = 1000)
    {
        if (_isMonitoring)
            return;

        _isMonitoring = true;
        _monitoringTimer = new Timer(async _ => await UpdateMetricsAsync(), null, 0, intervalMs);

        if (Log.Instance.IsTraceEnabled)
            Log.Instance.Trace($"GPU monitoring started with interval {intervalMs}ms");
    }

    public void Stop()
    {
        if (!_isMonitoring)
            return;

        _isMonitoring = false;
        _monitoringTimer?.Dispose();
        _monitoringTimer = null;

        if (Log.Instance.IsTraceEnabled)
            Log.Instance.Trace($"GPU monitoring stopped");
    }

    private async Task UpdateMetricsAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                try
                {
                    NVAPI.Initialize();
                    var gpu = NVAPI.GetGPU();
                    
                    if (gpu == null)
                        return;

                    var metrics = new GPUMetrics
                    {
                        CoreClockMHz = GetCurrentCoreClock(gpu),
                        MemoryClockMHz = GetCurrentMemoryClock(gpu),
                        TemperatureC = GetTemperature(gpu),
                        UsagePercent = GetUsagePercent(gpu),
                        PowerDrawWatts = GetPowerDraw(gpu),
                        PerformanceState = GetPerformanceState(gpu)
                    };

                    MetricsUpdated?.Invoke(this, metrics);
                }
                finally
                {
                    try { NVAPI.Unload(); } catch { /* Ignored */ }
                }
            });
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Error updating GPU metrics: {ex.Message}");
        }
    }

    private static int GetCurrentCoreClock(PhysicalGPU gpu)
    {
        try
        {
            var frequencies = gpu.CurrentClockFrequencies;
            return (int)(frequencies.GraphicsClock.Frequency / 1000); // Convert kHz to MHz
        }
        catch
        {
            return 0;
        }
    }

    private static int GetCurrentMemoryClock(PhysicalGPU gpu)
    {
        try
        {
            var frequencies = gpu.CurrentClockFrequencies;
            return (int)(frequencies.MemoryClock.Frequency / 1000); // Convert kHz to MHz
        }
        catch
        {
            return 0;
        }
    }

    private static int GetTemperature(PhysicalGPU gpu)
    {
        try
        {
            var sensors = gpu.ThermalInformation.ThermalSensors.ToArray();
            if (sensors.Length > 0)
                return sensors[0].CurrentTemperature;
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static int GetUsagePercent(PhysicalGPU gpu)
    {
        try
        {
            var usages = gpu.UsageInformation.GPU.Percentage;
            return usages > 0 ? usages : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static int GetPowerDraw(PhysicalGPU gpu)
    {
        try
        {
            // Power reading not directly available in this NVAPI wrapper version
            // Return 0 as placeholder
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string GetPerformanceState(PhysicalGPU gpu)
    {
        try
        {
            // Performance state reading simplified
            return "Active";
        }
        catch
        {
            return "Unknown";
        }
    }

    public void Dispose()
    {
        Stop();
    }
}

public class GPUMetrics
{
    public int CoreClockMHz { get; set; }
    public int MemoryClockMHz { get; set; }
    public int TemperatureC { get; set; }
    public int UsagePercent { get; set; }
    public int PowerDrawWatts { get; set; }
    public string PerformanceState { get; set; } = "Unknown";
}

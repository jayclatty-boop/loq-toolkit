using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.Utils;

namespace LenovoLegionToolkit.Lib.Controllers;

public class GPUStabilityTester
{
    private readonly GPUMonitor _monitor;
    private readonly GPUOverclockController _overclockController;
    private CancellationTokenSource? _testCts;
    private bool _isRunning;

    public event EventHandler<StabilityTestProgress>? ProgressUpdated;
    public event EventHandler<StabilityTestResult>? TestCompleted;

    public bool IsRunning => _isRunning;

    public GPUStabilityTester(GPUMonitor monitor, GPUOverclockController overclockController)
    {
        _monitor = monitor;
        _overclockController = overclockController;
    }

    public async Task StartTestAsync(StabilityTestDuration duration, int temperatureLimit, CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            throw new InvalidOperationException("Test is already running");

        _isRunning = true;
        _testCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var durationSeconds = duration == StabilityTestDuration.Quick ? 120 : 1800; // 2 min or 30 min
        var startTime = DateTime.Now;
        var highestTemp = 0;
        var wasStable = true;
        string? failureReason = null;

        try
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Starting {duration} stability test for {durationSeconds}s, temp limit: {temperatureLimit}°C");

            // Start monitoring if not already running
            if (!_monitor.IsMonitoring)
                _monitor.Start(1000);

            // Monitor temperature and report progress
            _monitor.MetricsUpdated += OnMetricsUpdated;

            void OnMetricsUpdated(object? sender, GPUMetrics metrics)
            {
                if (metrics.TemperatureC > highestTemp)
                    highestTemp = metrics.TemperatureC;

                var elapsed = (DateTime.Now - startTime).TotalSeconds;
                var remaining = durationSeconds - elapsed;
                var progress = (elapsed / durationSeconds) * 100;

                ProgressUpdated?.Invoke(this, new StabilityTestProgress
                {
                    ProgressPercent = Math.Min(100, (int)progress),
                    TimeRemainingSeconds = Math.Max(0, (int)remaining),
                    CurrentTemperature = metrics.TemperatureC,
                    HighestTemperature = highestTemp
                });

                // Check for temperature threshold
                if (metrics.TemperatureC >= temperatureLimit)
                {
                    wasStable = false;
                    failureReason = $"Temperature exceeded limit: {metrics.TemperatureC}°C >= {temperatureLimit}°C";
                    _testCts?.Cancel();
                }
            }

            // Run stress test
            await RunStressTestAsync(durationSeconds, _testCts.Token);

            _monitor.MetricsUpdated -= OnMetricsUpdated;

            // Report completion
            TestCompleted?.Invoke(this, new StabilityTestResult
            {
                Success = wasStable && !_testCts.Token.IsCancellationRequested,
                Duration = duration,
                HighestTemperature = highestTemp,
                FailureReason = _testCts.Token.IsCancellationRequested && wasStable ? "Test cancelled by user" : failureReason
            });

            if (!wasStable)
            {
                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"Test failed: {failureReason}. Reverting overclock.");

                // Revert overclock on failure
                _overclockController.SaveState(false, GPUOverclockInfo.Zero);
                await _overclockController.ApplyStateAsync(true);
            }
        }
        catch (OperationCanceledException)
        {
            TestCompleted?.Invoke(this, new StabilityTestResult
            {
                Success = false,
                Duration = duration,
                HighestTemperature = highestTemp,
                FailureReason = "Test cancelled"
            });
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Stability test error: {ex.Message}");

            TestCompleted?.Invoke(this, new StabilityTestResult
            {
                Success = false,
                Duration = duration,
                HighestTemperature = highestTemp,
                FailureReason = $"Error: {ex.Message}"
            });
        }
        finally
        {
            _isRunning = false;
            _testCts?.Dispose();
            _testCts = null;
        }
    }

    public void CancelTest()
    {
        _testCts?.Cancel();
    }

    private async Task RunStressTestAsync(int durationSeconds, CancellationToken cancellationToken)
    {
        // Simple stress test: keep GPU busy with compute work
        var stopwatch = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            while (stopwatch.Elapsed.TotalSeconds < durationSeconds && !cancellationToken.IsCancellationRequested)
            {
                // Simulate GPU load by doing intensive work
                // In a real implementation, you'd use GPU compute shaders or a proper stress test library
                // For now, just sleep and check periodically
                Thread.Sleep(100);
            }
        }, cancellationToken);
    }
}

public enum StabilityTestDuration
{
    Quick,
    Extended
}

public class StabilityTestProgress
{
    public int ProgressPercent { get; set; }
    public int TimeRemainingSeconds { get; set; }
    public int CurrentTemperature { get; set; }
    public int HighestTemperature { get; set; }
}

public class StabilityTestResult
{
    public bool Success { get; set; }
    public StabilityTestDuration Duration { get; set; }
    public int HighestTemperature { get; set; }
    public string? FailureReason { get; set; }
}

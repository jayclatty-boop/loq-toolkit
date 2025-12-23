using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.Controllers;
using LenovoLegionToolkit.Lib.Features;

namespace LenovoLegionToolkit.Lib.Services;

public class GamingModeService
{
    private readonly PowerModeFeature _powerModeFeature;
    private readonly List<string> _processesToKill = new()
    {
        "chrome", "msedge", "firefox", "opera", "brave",  // Browsers
        "Discord", "Slack", "Teams", "Spotify",            // Communication/Media
        "OneDrive", "Dropbox", "GoogleDriveFS",            // Cloud sync
        "SearchApp", "SearchHost",                         // Windows Search
    };

    private bool _isGamingModeActive;
    private PowerModeState _previousPowerMode = PowerModeState.Balance;

    public bool IsGamingModeActive => _isGamingModeActive;

    public GamingModeService()
    {
        _powerModeFeature = IoCContainer.Resolve<PowerModeFeature>();
    }

    public async Task<(bool success, string message, int processesKilled, long memoryFreed)> EnableGamingModeAsync()
    {
        if (_isGamingModeActive)
            return (false, "Gaming mode is already active", 0, 0);

        try
        {
            // Store current power mode
            try
            {
                _previousPowerMode = await _powerModeFeature.GetStateAsync();
            }
            catch { }

            // 1. Kill background processes
            var processesKilled = await KillBackgroundProcessesAsync();

            // 2. Clear RAM
            var memoryFreed = await System.Memory.ClearWorkingSetsAsync();
            await System.Memory.ClearStandbyListAsync();

            // 3. Switch to Performance mode
            try
            {
                if (await _powerModeFeature.IsSupportedAsync())
                {
                    await _powerModeFeature.SetStateAsync(PowerModeState.Performance);
                }
            }
            catch { }

            // 4. Disable Windows Game Bar (optional - some users want it)
            // This can be added if requested

            _isGamingModeActive = true;

            return (true, 
                $"Gaming mode activated! Killed {processesKilled} processes, freed {System.Memory.FormatBytes(memoryFreed)}", 
                processesKilled, 
                memoryFreed);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to enable gaming mode: {ex.Message}", 0, 0);
        }
    }

    public async Task<bool> DisableGamingModeAsync()
    {
        if (!_isGamingModeActive)
            return false;

        try
        {
            // Restore previous power mode
            if (await _powerModeFeature.IsSupportedAsync())
            {
                await _powerModeFeature.SetStateAsync(_previousPowerMode);
            }

            _isGamingModeActive = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<int> KillBackgroundProcessesAsync()
    {
        return await Task.Run(() =>
        {
            int killedCount = 0;

            foreach (var processName in _processesToKill)
            {
                try
                {
                    var processes = Process.GetProcessesByName(processName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            killedCount++;
                        }
                        catch { }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
                catch { }
            }

            return killedCount;
        });
    }

    public List<(string ProcessName, long MemoryMB, double CpuPercent)> GetResourceHungryProcesses()
    {
        try
        {
            var processes = Process.GetProcesses()
                .Where(p =>
                {
                    try
                    {
                        return p.WorkingSet64 > 100 * 1024 * 1024; // > 100 MB
                    }
                    catch
                    {
                        return false;
                    }
                })
                .Select(p =>
                {
                    try
                    {
                        return (
                            ProcessName: p.ProcessName,
                            MemoryMB: p.WorkingSet64 / (1024 * 1024),
                            CpuPercent: 0.0 // CPU percentage would require sampling over time
                        );
                    }
                    catch
                    {
                        return (ProcessName: "", MemoryMB: 0L, CpuPercent: 0.0);
                    }
                    finally
                    {
                        p.Dispose();
                    }
                })
                .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                .OrderByDescending(p => p.MemoryMB)
                .Take(15)
                .ToList();

            return processes;
        }
        catch
        {
            return new List<(string, long, double)>();
        }
    }
}

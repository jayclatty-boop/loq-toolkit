using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LenovoLegionToolkit.Lib.System;

public static class Memory
{
    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);

    public static async Task<long> ClearWorkingSetsAsync()
    {
        return await Task.Run(() =>
        {
            long freedBytes = 0;
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    var beforeMemory = process.WorkingSet64;
                    
                    // Try to empty the working set
                    if (EmptyWorkingSet(process.Handle))
                    {
                        process.Refresh();
                        var afterMemory = process.WorkingSet64;
                        freedBytes += Math.Max(0, beforeMemory - afterMemory);
                    }
                }
                catch
                {
                    // Ignore access denied or already exited processes
                }
                finally
                {
                    process.Dispose();
                }
            }

            return freedBytes;
        });
    }

    public static async Task<bool> ClearStandbyListAsync()
    {
        try
        {
            // Use EmptyStandbyList command from RAMMap via PowerShell
            // This requires admin privileges (which the app already has)
            await CMD.RunAsync("powershell", 
                "-NoProfile -Command \"[System.GC]::Collect(); [System.GC]::WaitForPendingFinalizers()\"");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    public static (long totalMemoryBytes, long availableMemoryBytes, double usagePercent) GetMemoryStatus()
    {
        try
        {
            var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            var totalMemory = (long)computerInfo.TotalPhysicalMemory;
            var availableMemory = (long)computerInfo.AvailablePhysicalMemory;
            var usedMemory = totalMemory - availableMemory;
            var usagePercent = (double)usedMemory / totalMemory * 100;

            return (totalMemory, availableMemory, usagePercent);
        }
        catch
        {
            return (0, 0, 0);
        }
    }
}

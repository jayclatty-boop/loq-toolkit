using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading.Tasks;

namespace LenovoLegionToolkit.Lib.Debloater;

/// <summary>
/// Manages snapshots of service start types for reliable undo operations.
/// Saves original state to JSON before any tweaks are applied.
/// </summary>
public class SnapshotManager
{
    private const string SnapshotFileName = "service_snapshots.json";
    private readonly string _snapshotPath;

    public SnapshotManager(string appDataFolder)
    {
        _snapshotPath = Path.Combine(appDataFolder, SnapshotFileName);
    }

    /// <summary>
    /// Creates a snapshot of the specified services' current start types before applying changes.
    /// </summary>
    public async Task CreateSnapshotAsync(params string[] serviceNames)
    {
        var existing = await LoadSnapshotsAsync();
        var snapshot = new ServiceSnapshot { Timestamp = DateTime.UtcNow.ToString("O") };

        foreach (var serviceName in serviceNames)
        {
            try
            {
                var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                if (service != null)
                {
                    // Read actual start type from registry
                    var startType = ReadServiceStartType(serviceName);
                    snapshot.Services[serviceName] = startType;
                }
            }
            catch
            {
                // If service doesn't exist or can't be read, skip it
            }
        }

        existing.Add(snapshot);
        await SaveSnapshotsAsync(existing);
    }

    /// <summary>
    /// Gets the most recent snapshot for the given service name.
    /// </summary>
    public async Task<string?> GetSnapshotStartTypeAsync(string serviceName)
    {
        var snapshots = await LoadSnapshotsAsync();
        // Find most recent snapshot containing this service
        for (int i = snapshots.Count - 1; i >= 0; i--)
        {
            if (snapshots[i].Services.TryGetValue(serviceName, out var startType))
                return startType;
        }
        return null;
    }

    /// <summary>
    /// Clears all snapshots (call after successful undo or manual cleanup).
    /// </summary>
    public async Task ClearSnapshotsAsync()
    {
        try
        {
            if (File.Exists(_snapshotPath))
                File.Delete(_snapshotPath);
        }
        catch
        {
            // Ignore cleanup errors
        }
        await Task.CompletedTask;
    }

    private async Task<List<ServiceSnapshot>> LoadSnapshotsAsync()
    {
        if (!File.Exists(_snapshotPath))
            return new List<ServiceSnapshot>();

        try
        {
            var json = await File.ReadAllTextAsync(_snapshotPath);
            var snapshots = JsonSerializer.Deserialize<List<ServiceSnapshot>>(json) ?? new List<ServiceSnapshot>();
            return snapshots;
        }
        catch
        {
            return new List<ServiceSnapshot>();
        }
    }

    private async Task SaveSnapshotsAsync(List<ServiceSnapshot> snapshots)
    {
        try
        {
            var directory = Path.GetDirectoryName(_snapshotPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            var json = JsonSerializer.Serialize(snapshots, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_snapshotPath, json);
        }
        catch
        {
            // Silently fail - snapshots are best-effort
        }
    }

    private static string ReadServiceStartType(string serviceName)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
            var startValue = key?.GetValue("Start");
            return startValue switch
            {
                0 => "Boot",
                1 => "System",
                2 => "Automatic",
                3 => "Manual",
                4 => "Disabled",
                _ => "Unknown"
            };
        }
        catch
        {
            return "Unknown";
        }
    }

    private class ServiceSnapshot
    {
        public string Timestamp { get; set; } = "";
        public Dictionary<string, string> Services { get; set; } = new();
    }
}

using System;
using System.IO;
using System.Threading.Tasks;

namespace LenovoLegionToolkit.Lib.Debloater;

/// <summary>
/// Centralized logging for debloater operations.
/// Writes timestamped logs to %APPDATA%\LOQToolkit\debloater.log
/// </summary>
public class DebloaterLogger
{
    private readonly string _logPath;
    private readonly object _lockObj = new object();

    public DebloaterLogger(string appDataFolder)
    {
        var logFolder = Path.Combine(appDataFolder, "logs");
        _logPath = Path.Combine(logFolder, "debloater.log");
    }

    public async Task LogOperationAsync(string operationType, string details, bool isSuccess = true)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var status = isSuccess ? "SUCCESS" : "FAILED";
        var message = $"[{timestamp}] [{status}] [{operationType}] {details}{Environment.NewLine}";

        await WriteToLogAsync(message);
    }

    public async Task LogApplyAsync(string tweakId, string tweakName, bool success)
    {
        var message = success 
            ? $"Applied tweak: {tweakName} ({tweakId})"
            : $"Failed to apply tweak: {tweakName} ({tweakId})";
        
        await LogOperationAsync("APPLY", message, success);
    }

    public async Task LogUndoAsync(string tweakId, string tweakName, bool success)
    {
        var message = success
            ? $"Undid tweak: {tweakName} ({tweakId})"
            : $"Failed to undo tweak: {tweakName} ({tweakId})";
        
        await LogOperationAsync("UNDO", message, success);
    }

    public async Task LogDryRunAsync(string tweakId, string tweakName, string changes)
    {
        var message = $"Dry-run for {tweakName} ({tweakId}): {changes}";
        await LogOperationAsync("DRY-RUN", message, true);
    }

    public async Task LogRestorePointAsync(string description, bool success)
    {
        await LogOperationAsync("RESTORE-POINT", description, success);
    }

    public async Task LogAdminCheckAsync(string operation, bool adminRequired, bool hasAdmin)
    {
        var message = $"{operation} - Admin required: {adminRequired}, Has admin: {hasAdmin}";
        await LogOperationAsync("ADMIN-CHECK", message, adminRequired == hasAdmin);
    }

    public async Task LogSessionStartAsync()
    {
        var message = $"=== Debloater Session Started ===";
        await LogOperationAsync("SESSION", message, true);
    }

    public async Task LogSessionEndAsync()
    {
        var message = $"=== Debloater Session Ended ===";
        await LogOperationAsync("SESSION", message, true);
    }

    public string GetLogPath() => _logPath;

    public async Task<string> ExportLogsAsync()
    {
        if (!File.Exists(_logPath))
            return "No log file found.";

        try
        {
            return await File.ReadAllTextAsync(_logPath);
        }
        catch (Exception ex)
        {
            return $"Error reading logs: {ex.Message}";
        }
    }

    public async Task ClearLogsAsync()
    {
        try
        {
            if (File.Exists(_logPath))
                File.Delete(_logPath);
        }
        catch
        {
            // Ignore cleanup errors
        }
        await Task.CompletedTask;
    }

    private async Task WriteToLogAsync(string message)
    {
        lock (_lockObj)
        {
            try
            {
                var directory = Path.GetDirectoryName(_logPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory!);

                File.AppendAllText(_logPath, message);
            }
            catch
            {
                // Silently fail - logging is best-effort
            }
        }
        await Task.CompletedTask;
    }
}

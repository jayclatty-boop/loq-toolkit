using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.System;

namespace LenovoLegionToolkit.Lib.Debloater;

public class DebloaterEngine
{
    private readonly IReadOnlyDictionary<string, IDebloatTweak> _tweaksById;
    private readonly DebloaterLogger? _logger;
    private readonly SnapshotManager? _snapshots;

    public DebloaterEngine(IEnumerable<IDebloatTweak> tweaks, DebloaterLogger? logger = null, SnapshotManager? snapshots = null)
    {
        _tweaksById = tweaks.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
        _snapshots = snapshots;
    }

    public bool RequiresAdmin(IEnumerable<string> tweakIds)
    {
        foreach (var id in tweakIds)
        {
            if (_tweaksById.TryGetValue(id, out var tweak) && tweak.IsAdminRequired)
                return true;
        }

        return false;
    }

    public async Task CreateRestorePointAsync(string description, CancellationToken token = default)
    {
        // Requires admin and System Protection enabled.
        // PowerShell is the most compatible approach here.
        var args = $"-NoProfile -ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description '{EscapePs(description)}' -RestorePointType 'MODIFY_SETTINGS'\"";
        var (exitCode, _) = await CMD.RunAsync("powershell", args, createNoWindow: true, waitForExit: true, token: token);
        if (exitCode != 0)
            throw new InvalidOperationException($"Failed to create restore point. ExitCode={exitCode}");
    }

    public async Task ApplyAsync(IEnumerable<string> tweakIds, bool createRestorePoint, string restorePointDescription, IProgress<string>? progress = null, CancellationToken token = default)
    {
        var ids = tweakIds.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        try
        {
            if (createRestorePoint)
            {
                progress?.Report("Creating restore point...");
                await CreateRestorePointAsync(restorePointDescription, token);
                await _logger?.LogRestorePointAsync(restorePointDescription, true)!;
            }

            foreach (var id in ids)
            {
                token.ThrowIfCancellationRequested();

                if (!_tweaksById.TryGetValue(id, out var tweak))
                    continue;

                progress?.Report($"Applying: {tweak.Title}");
                try
                {
                    await tweak.ApplyAsync(token);
                    await _logger?.LogApplyAsync(id, tweak.Title, true)!;
                }
                catch (Exception ex)
                {
                    await _logger?.LogApplyAsync(id, tweak.Title, false)!;
                    progress?.Report($"Error applying {tweak.Title}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            await _logger?.LogOperationAsync("APPLY", $"Apply operation failed: {ex.Message}", false)!;
            throw;
        }
    }

    public async Task UndoAsync(IEnumerable<string> tweakIds, IProgress<string>? progress = null, CancellationToken token = default)
    {
        var ids = tweakIds.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        try
        {
            foreach (var id in ids)
            {
                token.ThrowIfCancellationRequested();

                if (!_tweaksById.TryGetValue(id, out var tweak))
                    continue;

                if (!tweak.SupportsUndo)
                {
                    progress?.Report($"Skipping (no undo): {tweak.Title}");
                    await _logger?.LogOperationAsync("UNDO-SKIP", $"{tweak.Title} does not support undo", true)!;
                    continue;
                }

                progress?.Report($"Undoing: {tweak.Title}");
                try
                {
                    await tweak.UndoAsync(token);
                    await _logger?.LogUndoAsync(id, tweak.Title, true)!;
                }
                catch (Exception ex)
                {
                    await _logger?.LogUndoAsync(id, tweak.Title, false)!;
                    progress?.Report($"Error undoing {tweak.Title}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            await _logger?.LogOperationAsync("UNDO", $"Undo operation failed: {ex.Message}", false)!;
            throw;
        }
    }

    private static string EscapePs(string s) => s.Replace("'", "''");
}

using System;
using System.IO;
using System.Text.Json;

namespace LenovoLegionToolkit.Lib.Utils;

internal static class NdjsonDebugLogger
{
    // #region agent log
    private const string DebugLogPath = @"c:\LenovoLegionToolkit-master\.cursor\debug.log";
    // #endregion

    // #region agent log
    public static void Log(string hypothesisId, string location, string message, object? data = null)
    {
        try
        {
            var payload = new
            {
                sessionId = "debug-session",
                runId = Environment.GetEnvironmentVariable("LLT_DEBUG_RUNID") ?? "run1",
                hypothesisId,
                location,
                message,
                data,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var json = JsonSerializer.Serialize(payload);
            File.AppendAllText(DebugLogPath, json + Environment.NewLine);
        }
        catch
        {
            // Intentionally ignored: debug instrumentation must never crash the app.
        }
    }
    // #endregion
}



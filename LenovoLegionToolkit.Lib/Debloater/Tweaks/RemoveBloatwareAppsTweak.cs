using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.System;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public sealed class RemoveBloatwareAppsTweak : IDebloatTweak
{
    public const string TweakId = "bloatware.removeApps";

    public string Id => TweakId;
    public string Title => "Remove Preinstalled Apps";
    public string Description => "Removes selected preinstalled apps via PowerShell (current user / optional provisioned packages).";

    public DebloatCategory Category => DebloatCategory.Bloatware;
    public DebloatSeverity Severity => DebloatSeverity.Dangerous;

    public bool IsAdminRequired => false; // current user removal typically does not require admin
    public bool SupportsUndo => false; // uninstall isn't reliably undoable

    public Task<DebloatTweakStatus> GetStatusAsync(CancellationToken token = default)
    {
        // This is intentionally Unknown; user chooses scope and apps can vary.
        return Task.FromResult(DebloatTweakStatus.Unknown);
    }

    public async Task ApplyAsync(CancellationToken token = default)
    {
        // Default behavior: remove the "third party" list for current user.
        // UI will call ApplyAsync through DebloaterEngine; scope is controlled via DebloaterSettings.
        // To keep the tweak interface simple, we read scope from DebloaterSettings in the WPF layer when applying.
        await Task.CompletedTask;
    }

    public Task UndoAsync(CancellationToken token = default) => Task.CompletedTask;

    public static IReadOnlyList<string> ThirdPartyWildcards => new[]
    {
        "*TikTok*",
        "*Disney*",
        "*Spotify*",
        "*Facebook*",
        "*Instagram*",
        "*CandyCrush*",
        "*BubbleWitch*",
        "*Dropbox*",
        "*Booking*",
        "*Twitter*",
    };

    public static IReadOnlyList<string> MicrosoftWildcards => new[]
    {
        "Microsoft.BingNews",
        "Microsoft.BingWeather",
        "Microsoft.GetHelp",
        "Microsoft.Getstarted",
        "Microsoft.MicrosoftSolitaireCollection",
        "Microsoft.People",
        "Microsoft.YourPhone",
        "Microsoft.ZuneMusic",
        "Microsoft.ZuneVideo",
        "Microsoft.Xbox*",
    };

    public static async Task RemoveAppsAsync(IEnumerable<string> wildcards, bool allUsers, bool removeProvisioned, CancellationToken token)
    {
        var wildcardList = wildcards.Where(w => !string.IsNullOrWhiteSpace(w)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (wildcardList.Length == 0)
            return;

        // Remove installed AppX packages
        foreach (var wildcard in wildcardList)
        {
            token.ThrowIfCancellationRequested();

            var allUsersFlag = allUsers ? " -AllUsers" : string.Empty;
            var script = $"Get-AppxPackage{allUsersFlag} -Name '{EscapePs(wildcard)}' | Remove-AppxPackage -ErrorAction SilentlyContinue";
            var args = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"";
            _ = await CMD.RunAsync("powershell", args, createNoWindow: true, waitForExit: true, token: token);
        }

        if (!removeProvisioned)
            return;

        // Remove provisioned packages (requires admin)
        foreach (var wildcard in wildcardList)
        {
            token.ThrowIfCancellationRequested();

            var script = $"Get-AppxProvisionedPackage -Online | Where-Object {{$_.DisplayName -like '{EscapePs(wildcard)}'}} | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue";
            var args = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"";
            _ = await CMD.RunAsync("powershell", args, createNoWindow: true, waitForExit: true, token: token);
        }
    }

    private static string EscapePs(string s) => s.Replace("'", "''");
}

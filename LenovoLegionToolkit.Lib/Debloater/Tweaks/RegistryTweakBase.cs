using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using LLTRegistry = LenovoLegionToolkit.Lib.System.Registry;

namespace LenovoLegionToolkit.Lib.Debloater.Tweaks;

public abstract class RegistryTweakBase : IDebloatTweak
{
    public abstract string Id { get; }
    public abstract string Title { get; }
    public abstract string Description { get; }

    public abstract DebloatCategory Category { get; }
    public abstract DebloatSeverity Severity { get; }

    public virtual bool IsAdminRequired => false;
    public virtual bool SupportsUndo => true;

    protected abstract (string Hive, string SubKey, string ValueName, object EnabledValue, object DisabledValue, RegistryValueKind Kind)[] Entries { get; }

    public virtual Task<DebloatTweakStatus> GetStatusAsync(CancellationToken token = default)
    {
        // Best-effort heuristic: if all entries match EnabledValue => Applied; if all match DisabledValue => NotApplied.
        var allApplied = true;
        var allNotApplied = true;

        foreach (var (hive, subKey, valueName, enabledValue, disabledValue, _) in Entries)
        {
            var current = LLTRegistry.GetValue<object>(hive, subKey, valueName, disabledValue);
            var equalsEnabled = EqualsNormalized(current, enabledValue);
            var equalsDisabled = EqualsNormalized(current, disabledValue);

            allApplied &= equalsEnabled;
            allNotApplied &= equalsDisabled;
        }

        if (allApplied)
            return Task.FromResult(DebloatTweakStatus.Applied);
        if (allNotApplied)
            return Task.FromResult(DebloatTweakStatus.NotApplied);

        return Task.FromResult(DebloatTweakStatus.Unknown);
    }

    public virtual Task ApplyAsync(CancellationToken token = default)
    {
        foreach (var (hive, subKey, valueName, enabledValue, _, kind) in Entries)
            LLTRegistry.SetValue(hive, subKey, valueName, enabledValue, fixPermissions: IsAdminRequired, valueKind: kind);

        return Task.CompletedTask;
    }

    public virtual Task UndoAsync(CancellationToken token = default)
    {
        foreach (var (hive, subKey, valueName, _, disabledValue, kind) in Entries)
            LLTRegistry.SetValue(hive, subKey, valueName, disabledValue, fixPermissions: IsAdminRequired, valueKind: kind);

        return Task.CompletedTask;
    }

    private static bool EqualsNormalized(object? current, object expected)
    {
        if (current is null)
            return false;

        // Registry values often return int for DWORD.
        if (expected is int ei)
        {
            if (current is int ci)
                return ci == ei;
            if (current is long cl)
                return cl == ei;
            if (current is string cs && int.TryParse(cs, out var parsed))
                return parsed == ei;
        }

        return current.Equals(expected);
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace LenovoLegionToolkit.Lib.Debloater;

public interface IDebloatTweak
{
    string Id { get; }
    string Title { get; }
    string Description { get; }

    DebloatCategory Category { get; }
    DebloatSeverity Severity { get; }

    bool IsAdminRequired { get; }
    bool SupportsUndo { get; }

    Task<DebloatTweakStatus> GetStatusAsync(CancellationToken token = default);

    Task ApplyAsync(CancellationToken token = default);

    Task UndoAsync(CancellationToken token = default);
}

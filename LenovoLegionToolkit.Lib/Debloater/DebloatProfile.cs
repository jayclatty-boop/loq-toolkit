using System.Collections.Generic;

namespace LenovoLegionToolkit.Lib.Debloater;

public sealed record DebloatProfile(
    string Id,
    string Title,
    string Description,
    IReadOnlyList<string> TweakIds
);

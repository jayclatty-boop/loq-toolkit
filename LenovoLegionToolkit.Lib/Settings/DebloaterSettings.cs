using System.Collections.Generic;
using LenovoLegionToolkit.Lib.Debloater;

namespace LenovoLegionToolkit.Lib.Settings;

public class DebloaterSettings() : AbstractSettings<DebloaterSettings.DebloaterSettingsStore>("debloater.json")
{
    public class DebloaterSettingsStore
    {
        public bool CreateRestorePoint { get; set; } = true;

        public string? SelectedProfileId { get; set; } = "recommended";

        public BloatwareScope BloatwareScope { get; set; } = BloatwareScope.ThirdPartyOnly;

        public bool LockDangerousTweaks { get; set; } = false;

        // TweakId -> selected
        public Dictionary<string, bool> SelectedTweaks { get; set; } = new();
    }
}

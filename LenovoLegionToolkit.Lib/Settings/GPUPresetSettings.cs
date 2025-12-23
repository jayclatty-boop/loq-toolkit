using System.Collections.Generic;
using static LenovoLegionToolkit.Lib.Settings.GPUPresetSettings;

namespace LenovoLegionToolkit.Lib.Settings;

public class GPUPresetSettings() : AbstractSettings<GPUPresetSettingsStore>("gpu_presets.json")
{
    public class GPUPreset
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CoreDeltaMhz { get; set; }
        public int MemoryDeltaMhz { get; set; }
    }

    public class GPUModelPresets
    {
        public string GPUModelPattern { get; set; } = string.Empty;
        public List<GPUPreset> Presets { get; set; } = [];
    }

    public class GPUPresetSettingsStore
    {
        public string? ActivePresetName { get; set; }
        public Dictionary<string, GPUModelPresets> ModelPresets { get; set; } = [];
    }

    protected override GPUPresetSettingsStore Default => new()
    {
        ModelPresets = GetDefaultPresets()
    };

    private static Dictionary<string, GPUModelPresets> GetDefaultPresets()
    {
        return new Dictionary<string, GPUModelPresets>
        {
            // RTX 4060 Laptop GPU presets
            ["RTX 4060"] = new GPUModelPresets
            {
                GPUModelPattern = "RTX 4060",
                Presets =
                [
                    new GPUPreset { Name = "Conservative", Description = "Safe overclock with minimal risk", CoreDeltaMhz = 100, MemoryDeltaMhz = 200 },
                    new GPUPreset { Name = "Balanced", Description = "Moderate overclock for improved performance", CoreDeltaMhz = 150, MemoryDeltaMhz = 300 },
                    new GPUPreset { Name = "Aggressive", Description = "Maximum performance, may cause instability", CoreDeltaMhz = 200, MemoryDeltaMhz = 400 }
                ]
            },
            // RTX 4050 Laptop GPU presets
            ["RTX 4050"] = new GPUModelPresets
            {
                GPUModelPattern = "RTX 4050",
                Presets =
                [
                    new GPUPreset { Name = "Conservative", Description = "Safe overclock with minimal risk", CoreDeltaMhz = 100, MemoryDeltaMhz = 200 },
                    new GPUPreset { Name = "Balanced", Description = "Moderate overclock for improved performance", CoreDeltaMhz = 150, MemoryDeltaMhz = 250 },
                    new GPUPreset { Name = "Aggressive", Description = "Maximum performance, may cause instability", CoreDeltaMhz = 175, MemoryDeltaMhz = 350 }
                ]
            },
            // RTX 3050 Laptop GPU presets
            ["RTX 3050"] = new GPUModelPresets
            {
                GPUModelPattern = "RTX 3050",
                Presets =
                [
                    new GPUPreset { Name = "Conservative", Description = "Safe overclock with minimal risk", CoreDeltaMhz = 75, MemoryDeltaMhz = 150 },
                    new GPUPreset { Name = "Balanced", Description = "Moderate overclock for improved performance", CoreDeltaMhz = 125, MemoryDeltaMhz = 250 },
                    new GPUPreset { Name = "Aggressive", Description = "Maximum performance, may cause instability", CoreDeltaMhz = 150, MemoryDeltaMhz = 300 }
                ]
            },
            // RTX 4070 Laptop GPU presets
            ["RTX 4070"] = new GPUModelPresets
            {
                GPUModelPattern = "RTX 4070",
                Presets =
                [
                    new GPUPreset { Name = "Conservative", Description = "Safe overclock with minimal risk", CoreDeltaMhz = 100, MemoryDeltaMhz = 200 },
                    new GPUPreset { Name = "Balanced", Description = "Moderate overclock for improved performance", CoreDeltaMhz = 175, MemoryDeltaMhz = 350 },
                    new GPUPreset { Name = "Aggressive", Description = "Maximum performance, may cause instability", CoreDeltaMhz = 225, MemoryDeltaMhz = 450 }
                ]
            },
            // Default fallback presets for unknown GPUs
            ["Default"] = new GPUModelPresets
            {
                GPUModelPattern = "",
                Presets =
                [
                    new GPUPreset { Name = "Conservative", Description = "Safe overclock with minimal risk", CoreDeltaMhz = 75, MemoryDeltaMhz = 150 },
                    new GPUPreset { Name = "Balanced", Description = "Moderate overclock for improved performance", CoreDeltaMhz = 100, MemoryDeltaMhz = 200 },
                    new GPUPreset { Name = "Aggressive", Description = "Maximum performance, may cause instability", CoreDeltaMhz = 150, MemoryDeltaMhz = 300 }
                ]
            }
        };
    }

    /// <summary>
    /// Gets presets for the specified GPU model name.
    /// </summary>
    public GPUModelPresets GetPresetsForGPU(string gpuName)
    {
        foreach (var kvp in Store.ModelPresets)
        {
            if (kvp.Key != "Default" && gpuName.Contains(kvp.Value.GPUModelPattern, global::System.StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        // Return default presets if no specific match
        return Store.ModelPresets.TryGetValue("Default", out var defaultPresets)
            ? defaultPresets
            : new GPUModelPresets { Presets = [] };
    }
}

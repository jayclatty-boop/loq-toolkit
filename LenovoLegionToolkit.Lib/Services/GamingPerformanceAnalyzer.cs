using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.Controllers;
using LenovoLegionToolkit.Lib.GameDetection;
using LenovoLegionToolkit.Lib.Utils;

namespace LenovoLegionToolkit.Lib.Services;

public class GamingPerformanceAnalyzer
{
    public class GamePerformanceMetrics
    {
        public string? GameName { get; set; }
        public string? DetectedGameDisplayName { get; set; }
        public bool IsGameRunning { get; set; }
        public int EstimatedSustainableFps { get; set; }
        public int MaxTemperatureCelsius { get; set; }
        public double ThermalHeadroomPercent { get; set; }
        public double PowerHeadroomPercent { get; set; }
        public double CurrentPowerDrawWatts { get; set; }
        public double MaxPowerDrawWatts { get; set; }
        public int CurrentTemperatureCelsius { get; set; }
        public string? RecommendedPreset { get; set; }
        public string? PerformanceProfile { get; set; }
        public GameInfo? DetectedGameInfo { get; set; }
    }

    /// <summary>
    /// Game information with baseline FPS data at different quality levels
    /// </summary>
    public class GameInfo
    {
        public string ProcessName { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public int BaselineFpsLow { get; init; }      // Low quality @ 1080p
        public int BaselineFpsMedium { get; init; }   // Medium quality @ 1080p
        public int BaselineFpsHigh { get; init; }     // High quality @ 1080p
        public int BaselineFpsUltra { get; init; }    // Ultra quality @ 1080p
        public int BaselineFpsMax { get; init; }      // Performance mode @ 1080p
        public bool IsCpuBound { get; init; }         // True if more CPU-dependent
        public string Category { get; init; } = "General";
    }

    private readonly GPUMonitor _gpuMonitor;
    private GPUMetrics? _lastMetrics;
    private string? _lastDetectedGame;
    private GameInfo? _lastDetectedGameInfo;
    
    // Constants for thermal and power analysis
    private const int GPU_POWER_MAX = 150;  // Max GPU TDP in watts
    private const int THERMAL_LIMIT = 85;   // Target max temp
    private const int THROTTLE_THRESHOLD = 95;

    // Comprehensive game database with realistic FPS expectations
    // Based on RTX 4060/4070 laptop GPU performance benchmarks
    private static readonly Dictionary<string, GameInfo> GameDatabase = new(StringComparer.OrdinalIgnoreCase)
    {
        // Popular Competitive/Esports Games
        ["rocketleague"] = new() { ProcessName = "rocketleague", DisplayName = "Rocket League", BaselineFpsLow = 300, BaselineFpsMedium = 250, BaselineFpsHigh = 200, BaselineFpsUltra = 150, BaselineFpsMax = 360, IsCpuBound = false, Category = "Esports" },
        ["rainbowsix"] = new() { ProcessName = "rainbowsix", DisplayName = "Rainbow Six Siege", BaselineFpsLow = 280, BaselineFpsMedium = 220, BaselineFpsHigh = 180, BaselineFpsUltra = 140, BaselineFpsMax = 350, IsCpuBound = true, Category = "Esports" },
        ["r6-siege"] = new() { ProcessName = "r6-siege", DisplayName = "Rainbow Six Siege", BaselineFpsLow = 280, BaselineFpsMedium = 220, BaselineFpsHigh = 180, BaselineFpsUltra = 140, BaselineFpsMax = 350, IsCpuBound = true, Category = "Esports" },
        ["minecraft"] = new() { ProcessName = "minecraft", DisplayName = "Minecraft", BaselineFpsLow = 400, BaselineFpsMedium = 200, BaselineFpsHigh = 120, BaselineFpsUltra = 80, BaselineFpsMax = 500, IsCpuBound = true, Category = "Sandbox" },
        ["javaw"] = new() { ProcessName = "javaw", DisplayName = "Minecraft (Java)", BaselineFpsLow = 400, BaselineFpsMedium = 200, BaselineFpsHigh = 120, BaselineFpsUltra = 80, BaselineFpsMax = 500, IsCpuBound = true, Category = "Sandbox" },
        ["minecraft.windows"] = new() { ProcessName = "minecraft.windows", DisplayName = "Minecraft (Bedrock)", BaselineFpsLow = 500, BaselineFpsMedium = 350, BaselineFpsHigh = 250, BaselineFpsUltra = 180, BaselineFpsMax = 600, IsCpuBound = false, Category = "Sandbox" },
        ["valorant"] = new() { ProcessName = "valorant", DisplayName = "Valorant", BaselineFpsLow = 350, BaselineFpsMedium = 300, BaselineFpsHigh = 250, BaselineFpsUltra = 180, BaselineFpsMax = 400, IsCpuBound = true, Category = "Esports" },
        ["valorant-win64-shipping"] = new() { ProcessName = "valorant-win64-shipping", DisplayName = "Valorant", BaselineFpsLow = 350, BaselineFpsMedium = 300, BaselineFpsHigh = 250, BaselineFpsUltra = 180, BaselineFpsMax = 400, IsCpuBound = true, Category = "Esports" },
        ["cs2"] = new() { ProcessName = "cs2", DisplayName = "Counter-Strike 2", BaselineFpsLow = 250, BaselineFpsMedium = 200, BaselineFpsHigh = 160, BaselineFpsUltra = 120, BaselineFpsMax = 300, IsCpuBound = true, Category = "Esports" },
        ["csgo"] = new() { ProcessName = "csgo", DisplayName = "CS:GO", BaselineFpsLow = 400, BaselineFpsMedium = 350, BaselineFpsHigh = 280, BaselineFpsUltra = 220, BaselineFpsMax = 500, IsCpuBound = true, Category = "Esports" },
        ["dota2"] = new() { ProcessName = "dota2", DisplayName = "Dota 2", BaselineFpsLow = 200, BaselineFpsMedium = 160, BaselineFpsHigh = 130, BaselineFpsUltra = 100, BaselineFpsMax = 240, IsCpuBound = true, Category = "Esports" },
        ["leagueoflegends"] = new() { ProcessName = "leagueoflegends", DisplayName = "League of Legends", BaselineFpsLow = 300, BaselineFpsMedium = 250, BaselineFpsHigh = 200, BaselineFpsUltra = 160, BaselineFpsMax = 400, IsCpuBound = true, Category = "Esports" },
        ["league of legends"] = new() { ProcessName = "league of legends", DisplayName = "League of Legends", BaselineFpsLow = 300, BaselineFpsMedium = 250, BaselineFpsHigh = 200, BaselineFpsUltra = 160, BaselineFpsMax = 400, IsCpuBound = true, Category = "Esports" },
        ["overwatch"] = new() { ProcessName = "overwatch", DisplayName = "Overwatch 2", BaselineFpsLow = 220, BaselineFpsMedium = 180, BaselineFpsHigh = 140, BaselineFpsUltra = 100, BaselineFpsMax = 280, IsCpuBound = false, Category = "Esports" },
        ["apexlegends"] = new() { ProcessName = "apexlegends", DisplayName = "Apex Legends", BaselineFpsLow = 180, BaselineFpsMedium = 140, BaselineFpsHigh = 110, BaselineFpsUltra = 85, BaselineFpsMax = 220, IsCpuBound = true, Category = "Esports" },
        ["r5apex"] = new() { ProcessName = "r5apex", DisplayName = "Apex Legends", BaselineFpsLow = 180, BaselineFpsMedium = 140, BaselineFpsHigh = 110, BaselineFpsUltra = 85, BaselineFpsMax = 220, IsCpuBound = true, Category = "Esports" },
        ["fortnite"] = new() { ProcessName = "fortnite", DisplayName = "Fortnite", BaselineFpsLow = 200, BaselineFpsMedium = 160, BaselineFpsHigh = 120, BaselineFpsUltra = 80, BaselineFpsMax = 240, IsCpuBound = false, Category = "Esports" },
        ["fortniteclient-win64-shipping"] = new() { ProcessName = "fortniteclient-win64-shipping", DisplayName = "Fortnite", BaselineFpsLow = 200, BaselineFpsMedium = 160, BaselineFpsHigh = 120, BaselineFpsUltra = 80, BaselineFpsMax = 240, IsCpuBound = false, Category = "Esports" },
        ["pubg"] = new() { ProcessName = "pubg", DisplayName = "PUBG", BaselineFpsLow = 160, BaselineFpsMedium = 120, BaselineFpsHigh = 100, BaselineFpsUltra = 75, BaselineFpsMax = 200, IsCpuBound = true, Category = "Esports" },
        ["tslgame"] = new() { ProcessName = "tslgame", DisplayName = "PUBG", BaselineFpsLow = 160, BaselineFpsMedium = 120, BaselineFpsHigh = 100, BaselineFpsUltra = 75, BaselineFpsMax = 200, IsCpuBound = true, Category = "Esports" },
        
        // AAA Games
        ["eldenring"] = new() { ProcessName = "eldenring", DisplayName = "Elden Ring", BaselineFpsLow = 60, BaselineFpsMedium = 55, BaselineFpsHigh = 50, BaselineFpsUltra = 45, BaselineFpsMax = 60, IsCpuBound = false, Category = "AAA" },
        ["starfield"] = new() { ProcessName = "starfield", DisplayName = "Starfield", BaselineFpsLow = 70, BaselineFpsMedium = 55, BaselineFpsHigh = 45, BaselineFpsUltra = 35, BaselineFpsMax = 80, IsCpuBound = true, Category = "AAA" },
        ["baldursgate3"] = new() { ProcessName = "baldursgate3", DisplayName = "Baldur's Gate 3", BaselineFpsLow = 90, BaselineFpsMedium = 70, BaselineFpsHigh = 55, BaselineFpsUltra = 45, BaselineFpsMax = 100, IsCpuBound = true, Category = "AAA" },
        ["bg3"] = new() { ProcessName = "bg3", DisplayName = "Baldur's Gate 3", BaselineFpsLow = 90, BaselineFpsMedium = 70, BaselineFpsHigh = 55, BaselineFpsUltra = 45, BaselineFpsMax = 100, IsCpuBound = true, Category = "AAA" },
        ["bg3_dx11"] = new() { ProcessName = "bg3_dx11", DisplayName = "Baldur's Gate 3", BaselineFpsLow = 90, BaselineFpsMedium = 70, BaselineFpsHigh = 55, BaselineFpsUltra = 45, BaselineFpsMax = 100, IsCpuBound = true, Category = "AAA" },
        ["palworld"] = new() { ProcessName = "palworld", DisplayName = "Palworld", BaselineFpsLow = 80, BaselineFpsMedium = 65, BaselineFpsHigh = 55, BaselineFpsUltra = 45, BaselineFpsMax = 90, IsCpuBound = false, Category = "AAA" },
        ["palworld-win64-shipping"] = new() { ProcessName = "palworld-win64-shipping", DisplayName = "Palworld", BaselineFpsLow = 80, BaselineFpsMedium = 65, BaselineFpsHigh = 55, BaselineFpsUltra = 45, BaselineFpsMax = 90, IsCpuBound = false, Category = "AAA" },
        ["cyberpunk2077"] = new() { ProcessName = "cyberpunk2077", DisplayName = "Cyberpunk 2077", BaselineFpsLow = 80, BaselineFpsMedium = 60, BaselineFpsHigh = 50, BaselineFpsUltra = 35, BaselineFpsMax = 90, IsCpuBound = false, Category = "AAA" },
        ["rdr2"] = new() { ProcessName = "rdr2", DisplayName = "Red Dead Redemption 2", BaselineFpsLow = 85, BaselineFpsMedium = 70, BaselineFpsHigh = 55, BaselineFpsUltra = 45, BaselineFpsMax = 100, IsCpuBound = true, Category = "AAA" },
        ["hogwartslegacy"] = new() { ProcessName = "hogwartslegacy", DisplayName = "Hogwarts Legacy", BaselineFpsLow = 80, BaselineFpsMedium = 65, BaselineFpsHigh = 50, BaselineFpsUltra = 40, BaselineFpsMax = 90, IsCpuBound = false, Category = "AAA" },
        ["gta5"] = new() { ProcessName = "gta5", DisplayName = "GTA V", BaselineFpsLow = 140, BaselineFpsMedium = 120, BaselineFpsHigh = 100, BaselineFpsUltra = 80, BaselineFpsMax = 160, IsCpuBound = true, Category = "AAA" },
        ["gtav"] = new() { ProcessName = "gtav", DisplayName = "GTA V", BaselineFpsLow = 140, BaselineFpsMedium = 120, BaselineFpsHigh = 100, BaselineFpsUltra = 80, BaselineFpsMax = 160, IsCpuBound = true, Category = "AAA" },
        ["modernwarfare"] = new() { ProcessName = "modernwarfare", DisplayName = "Call of Duty: MW", BaselineFpsLow = 140, BaselineFpsMedium = 110, BaselineFpsHigh = 90, BaselineFpsUltra = 70, BaselineFpsMax = 165, IsCpuBound = true, Category = "AAA" },
        ["cod"] = new() { ProcessName = "cod", DisplayName = "Call of Duty", BaselineFpsLow = 140, BaselineFpsMedium = 110, BaselineFpsHigh = 90, BaselineFpsUltra = 70, BaselineFpsMax = 165, IsCpuBound = true, Category = "AAA" },
        
        // Simulation / Racing
        ["assettocorsa"] = new() { ProcessName = "assettocorsa", DisplayName = "Assetto Corsa", BaselineFpsLow = 180, BaselineFpsMedium = 150, BaselineFpsHigh = 120, BaselineFpsUltra = 90, BaselineFpsMax = 200, IsCpuBound = true, Category = "Racing" },
        ["acc"] = new() { ProcessName = "acc", DisplayName = "Assetto Corsa Competizione", BaselineFpsLow = 120, BaselineFpsMedium = 100, BaselineFpsHigh = 80, BaselineFpsUltra = 60, BaselineFpsMax = 140, IsCpuBound = false, Category = "Racing" },
        ["forzahorizon5"] = new() { ProcessName = "forzahorizon5", DisplayName = "Forza Horizon 5", BaselineFpsLow = 110, BaselineFpsMedium = 95, BaselineFpsHigh = 80, BaselineFpsUltra = 65, BaselineFpsMax = 120, IsCpuBound = false, Category = "Racing" },
        ["forzahorizon4"] = new() { ProcessName = "forzahorizon4", DisplayName = "Forza Horizon 4", BaselineFpsLow = 130, BaselineFpsMedium = 110, BaselineFpsHigh = 95, BaselineFpsUltra = 75, BaselineFpsMax = 145, IsCpuBound = false, Category = "Racing" },
        ["msfs"] = new() { ProcessName = "msfs", DisplayName = "Microsoft Flight Simulator", BaselineFpsLow = 50, BaselineFpsMedium = 40, BaselineFpsHigh = 35, BaselineFpsUltra = 28, BaselineFpsMax = 60, IsCpuBound = true, Category = "Simulation" },
        ["flightsimulator"] = new() { ProcessName = "flightsimulator", DisplayName = "Microsoft Flight Simulator", BaselineFpsLow = 50, BaselineFpsMedium = 40, BaselineFpsHigh = 35, BaselineFpsUltra = 28, BaselineFpsMax = 60, IsCpuBound = true, Category = "Simulation" },
        
        // Other Popular Games
        ["warframe"] = new() { ProcessName = "warframe", DisplayName = "Warframe", BaselineFpsLow = 200, BaselineFpsMedium = 160, BaselineFpsHigh = 130, BaselineFpsUltra = 100, BaselineFpsMax = 240, IsCpuBound = false, Category = "Looter Shooter" },
        ["destiny2"] = new() { ProcessName = "destiny2", DisplayName = "Destiny 2", BaselineFpsLow = 140, BaselineFpsMedium = 110, BaselineFpsHigh = 90, BaselineFpsUltra = 70, BaselineFpsMax = 165, IsCpuBound = true, Category = "Looter Shooter" },
        ["deadbydaylight"] = new() { ProcessName = "deadbydaylight", DisplayName = "Dead by Daylight", BaselineFpsLow = 120, BaselineFpsMedium = 100, BaselineFpsHigh = 85, BaselineFpsUltra = 70, BaselineFpsMax = 130, IsCpuBound = true, Category = "Horror" },
        ["phasmophobia"] = new() { ProcessName = "phasmophobia", DisplayName = "Phasmophobia", BaselineFpsLow = 100, BaselineFpsMedium = 85, BaselineFpsHigh = 70, BaselineFpsUltra = 55, BaselineFpsMax = 120, IsCpuBound = true, Category = "Horror" },
        ["satisfactory"] = new() { ProcessName = "satisfactory", DisplayName = "Satisfactory", BaselineFpsLow = 90, BaselineFpsMedium = 75, BaselineFpsHigh = 60, BaselineFpsUltra = 50, BaselineFpsMax = 100, IsCpuBound = true, Category = "Sandbox" },
        ["rust"] = new() { ProcessName = "rust", DisplayName = "Rust", BaselineFpsLow = 100, BaselineFpsMedium = 80, BaselineFpsHigh = 65, BaselineFpsUltra = 50, BaselineFpsMax = 120, IsCpuBound = true, Category = "Survival" },
        ["ark"] = new() { ProcessName = "ark", DisplayName = "ARK: Survival Evolved", BaselineFpsLow = 80, BaselineFpsMedium = 60, BaselineFpsHigh = 45, BaselineFpsUltra = 35, BaselineFpsMax = 100, IsCpuBound = false, Category = "Survival" },
        ["terraria"] = new() { ProcessName = "terraria", DisplayName = "Terraria", BaselineFpsLow = 144, BaselineFpsMedium = 144, BaselineFpsHigh = 144, BaselineFpsUltra = 144, BaselineFpsMax = 144, IsCpuBound = true, Category = "Sandbox" },
        ["stardewvalley"] = new() { ProcessName = "stardewvalley", DisplayName = "Stardew Valley", BaselineFpsLow = 144, BaselineFpsMedium = 144, BaselineFpsHigh = 144, BaselineFpsUltra = 144, BaselineFpsMax = 144, IsCpuBound = true, Category = "Sandbox" },
        
        // MMOs
        ["wow"] = new() { ProcessName = "wow", DisplayName = "World of Warcraft", BaselineFpsLow = 180, BaselineFpsMedium = 140, BaselineFpsHigh = 110, BaselineFpsUltra = 80, BaselineFpsMax = 200, IsCpuBound = true, Category = "MMO" },
        ["ffxiv"] = new() { ProcessName = "ffxiv", DisplayName = "Final Fantasy XIV", BaselineFpsLow = 160, BaselineFpsMedium = 130, BaselineFpsHigh = 100, BaselineFpsUltra = 75, BaselineFpsMax = 180, IsCpuBound = true, Category = "MMO" },
        ["ffxiv_dx11"] = new() { ProcessName = "ffxiv_dx11", DisplayName = "Final Fantasy XIV", BaselineFpsLow = 160, BaselineFpsMedium = 130, BaselineFpsHigh = 100, BaselineFpsUltra = 75, BaselineFpsMax = 180, IsCpuBound = true, Category = "MMO" },
        ["genshinimpact"] = new() { ProcessName = "genshinimpact", DisplayName = "Genshin Impact", BaselineFpsLow = 120, BaselineFpsMedium = 90, BaselineFpsHigh = 70, BaselineFpsUltra = 60, BaselineFpsMax = 120, IsCpuBound = false, Category = "ARPG" },
        ["zenlesszonezero"] = new() { ProcessName = "zenlesszonezero", DisplayName = "Zenless Zone Zero", BaselineFpsLow = 120, BaselineFpsMedium = 90, BaselineFpsHigh = 70, BaselineFpsUltra = 60, BaselineFpsMax = 120, IsCpuBound = false, Category = "ARPG" },
        ["honkaistarrail"] = new() { ProcessName = "honkaistarrail", DisplayName = "Honkai: Star Rail", BaselineFpsLow = 120, BaselineFpsMedium = 90, BaselineFpsHigh = 70, BaselineFpsUltra = 60, BaselineFpsMax = 120, IsCpuBound = false, Category = "ARPG" },
        ["starrail"] = new() { ProcessName = "starrail", DisplayName = "Honkai: Star Rail", BaselineFpsLow = 120, BaselineFpsMedium = 90, BaselineFpsHigh = 70, BaselineFpsUltra = 60, BaselineFpsMax = 120, IsCpuBound = false, Category = "ARPG" },
    };

    public GamingPerformanceAnalyzer(GPUMonitor gpuMonitor)
    {
        _gpuMonitor = gpuMonitor;
        // Subscribe to GPU metrics updates to cache them
        _gpuMonitor.MetricsUpdated += (s, metrics) => { _lastMetrics = metrics; };
    }

    /// <summary>
    /// Detects if a game is currently running and returns its display name
    /// </summary>
    public async Task<string?> DetectRunningGameAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    try
                    {
                        var processName = Path.GetFileNameWithoutExtension(process.ProcessName).ToLower();
                        
                        // Check against our game database
                        foreach (var game in GameDatabase)
                        {
                            if (processName.Contains(game.Key.ToLower()) || game.Key.ToLower().Contains(processName))
                            {
                                _lastDetectedGame = processName;
                                _lastDetectedGameInfo = game.Value;
                                return game.Value.DisplayName;
                            }
                        }
                    }
                    catch { /* Ignore process access exceptions */ }
                }

                _lastDetectedGame = null;
                _lastDetectedGameInfo = null;
                return null;
            }
            catch { return null; }
        });
    }

    /// <summary>
    /// Gets detailed info about the currently detected game
    /// </summary>
    public GameInfo? GetDetectedGameInfo() => _lastDetectedGameInfo;

    /// <summary>
    /// Analyzes current performance metrics and returns gaming performance data
    /// </summary>
    public GamePerformanceMetrics AnalyzeCurrentPerformance()
    {
        var metrics = new GamePerformanceMetrics();

        try
        {
            if (_lastMetrics == null)
                return metrics;

            metrics.DetectedGameInfo = _lastDetectedGameInfo;
            metrics.DetectedGameDisplayName = _lastDetectedGameInfo?.DisplayName;

            // Thermal headroom calculation
            var thermalMargin = THERMAL_LIMIT - _lastMetrics.TemperatureC;
            metrics.ThermalHeadroomPercent = Math.Max(0, Math.Min(100, (thermalMargin / (double)THERMAL_LIMIT) * 100));
            metrics.CurrentTemperatureCelsius = _lastMetrics.TemperatureC;
            metrics.MaxTemperatureCelsius = THERMAL_LIMIT;

            // Power headroom calculation
            metrics.CurrentPowerDrawWatts = _lastMetrics.PowerDrawWatts;
            metrics.MaxPowerDrawWatts = GPU_POWER_MAX;
            var powerMargin = GPU_POWER_MAX - _lastMetrics.PowerDrawWatts;
            metrics.PowerHeadroomPercent = Math.Max(0, Math.Min(100, (powerMargin / (double)GPU_POWER_MAX) * 100));

            // Calculate sustainable FPS - game-specific if detected
            if (_lastDetectedGameInfo != null)
            {
                metrics.EstimatedSustainableFps = CalculateGameSpecificFps(_lastDetectedGameInfo, metrics);
            }
            else
            {
                // Generic calculation for unknown games
                var performanceFactor = (metrics.ThermalHeadroomPercent + metrics.PowerHeadroomPercent) / 200.0;
                metrics.EstimatedSustainableFps = (int)(60 * (0.7 + performanceFactor * 0.8));
                metrics.EstimatedSustainableFps = Math.Clamp(metrics.EstimatedSustainableFps, 30, 165);
            }

            // Performance profile recommendation based on headroom
            if (metrics.ThermalHeadroomPercent > 50 && metrics.PowerHeadroomPercent > 50)
            {
                metrics.PerformanceProfile = "Excellent";
                metrics.RecommendedPreset = "Aggressive";
            }
            else if (metrics.ThermalHeadroomPercent > 30 && metrics.PowerHeadroomPercent > 30)
            {
                metrics.PerformanceProfile = "Good";
                metrics.RecommendedPreset = "Balanced";
            }
            else if (metrics.ThermalHeadroomPercent > 15 && metrics.PowerHeadroomPercent > 15)
            {
                metrics.PerformanceProfile = "Fair";
                metrics.RecommendedPreset = "Conservative";
            }
            else
            {
                metrics.PerformanceProfile = "Limited";
                metrics.RecommendedPreset = "Quiet";
            }
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Error analyzing performance: {ex.Message}");
        }

        return metrics;
    }

    /// <summary>
    /// Calculates expected FPS for a specific game based on thermal/power headroom
    /// </summary>
    private int CalculateGameSpecificFps(GameInfo gameInfo, GamePerformanceMetrics metrics)
    {
        // Base FPS at medium settings (our benchmark reference)
        var baseFps = gameInfo.BaselineFpsMedium;

        // Performance multiplier based on headroom
        // Full headroom = can maintain baseline, less headroom = lower fps due to throttling
        var thermalFactor = Math.Min(1.0, metrics.ThermalHeadroomPercent / 50.0);  // 50% headroom = full performance
        var powerFactor = Math.Min(1.0, metrics.PowerHeadroomPercent / 50.0);
        
        // Combined factor - use the lower of the two (bottleneck)
        double performanceFactor;
        if (gameInfo.IsCpuBound)
        {
            // CPU-bound games are less affected by GPU thermal/power limits
            performanceFactor = 0.5 + (thermalFactor + powerFactor) / 4.0;
        }
        else
        {
            // GPU-bound games scale more directly with headroom
            performanceFactor = 0.4 + Math.Min(thermalFactor, powerFactor) * 0.6;
        }

        // Apply GPU utilization factor if available
        if (_lastMetrics != null && _lastMetrics.UsagePercent > 0)
        {
            // If GPU isn't fully loaded, actual FPS is likely higher
            var utilizationBonus = (100 - _lastMetrics.UsagePercent) / 200.0;  // Up to 50% bonus if GPU idle
            performanceFactor = Math.Min(1.5, performanceFactor + utilizationBonus);
        }

        var estimatedFps = (int)(baseFps * performanceFactor);
        
        // Clamp to reasonable range for the game
        return Math.Clamp(estimatedFps, 20, gameInfo.BaselineFpsMax);
    }

    /// <summary>
    /// Returns FPS predictions for different quality levels
    /// </summary>
    public Dictionary<string, int> GetFPSPredictions(GamePerformanceMetrics metrics)
    {
        if (metrics.DetectedGameInfo != null)
        {
            // Game-specific predictions
            var gameInfo = metrics.DetectedGameInfo;
            var performanceFactor = CalculatePerformanceFactor(metrics);
            
            return new Dictionary<string, int>
            {
                { "Ultra Quality", Math.Max(20, (int)(gameInfo.BaselineFpsUltra * performanceFactor)) },
                { "High Quality", Math.Max(30, (int)(gameInfo.BaselineFpsHigh * performanceFactor)) },
                { "Medium Quality", Math.Max(40, (int)(gameInfo.BaselineFpsMedium * performanceFactor)) },
                { "Low Quality", Math.Max(50, (int)(gameInfo.BaselineFpsLow * performanceFactor)) },
                { "Performance Mode", Math.Min(gameInfo.BaselineFpsMax, (int)(gameInfo.BaselineFpsMax * performanceFactor)) }
            };
        }
        
        // Generic predictions for unknown games
        var baseFps = metrics.EstimatedSustainableFps;
        return new Dictionary<string, int>
        {
            { "Ultra Quality", Math.Max(30, baseFps - 25) },
            { "High Quality", Math.Max(40, baseFps - 10) },
            { "Medium Quality", baseFps },
            { "Low Quality", Math.Min(165, baseFps + 20) },
            { "Performance Mode", Math.Min(165, baseFps + 40) }
        };
    }

    private double CalculatePerformanceFactor(GamePerformanceMetrics metrics)
    {
        var thermalFactor = Math.Min(1.0, metrics.ThermalHeadroomPercent / 50.0);
        var powerFactor = Math.Min(1.0, metrics.PowerHeadroomPercent / 50.0);
        return 0.5 + Math.Min(thermalFactor, powerFactor) * 0.5;
    }

    /// <summary>
    /// Recommends a God Mode preset based on current thermal and power headroom
    /// </summary>
    public string GetGodModePresetRecommendation(GamePerformanceMetrics metrics)
    {
        var gameSpecificHint = metrics.DetectedGameInfo != null 
            ? $" ({metrics.DetectedGameInfo.DisplayName} detected)" 
            : "";
            
        return metrics.RecommendedPreset switch
        {
            "Aggressive" => $"Max Performance{gameSpecificHint} - Excellent thermal/power headroom. Safe for aggressive overclocking.",
            "Balanced" => $"Balanced Performance{gameSpecificHint} - Good headroom. Moderate overclocking recommended.",
            "Conservative" => $"Conservative{gameSpecificHint} - Limited headroom. Light overclocking only.",
            _ => $"Quiet Mode{gameSpecificHint} - Low headroom. Focus on cooling before overclocking."
        };
    }

    /// <summary>
    /// Gets a color-coded performance indicator (Green/Yellow/Red)
    /// </summary>
    public string GetPerformanceStatus(GamePerformanceMetrics metrics)
    {
        if (metrics.CurrentTemperatureCelsius >= THROTTLE_THRESHOLD)
            return "Critical";
        if (metrics.CurrentTemperatureCelsius >= THERMAL_LIMIT)
            return "Warning";
        if (metrics.ThermalHeadroomPercent < 20 || metrics.PowerHeadroomPercent < 20)
            return "Caution";
        
        return "Optimal";
    }

    /// <summary>
    /// Gets all games in the database for display purposes
    /// </summary>
    public static IEnumerable<GameInfo> GetKnownGames() => GameDatabase.Values.Distinct();

    /// <summary>
    /// Checks if a specific game is in the database
    /// </summary>
    public static bool IsGameKnown(string processName) => GameDatabase.ContainsKey(processName.ToLower());
}

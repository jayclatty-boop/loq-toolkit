using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Controllers;
using LenovoLegionToolkit.Lib.Controllers.Sensors;
using LenovoLegionToolkit.Lib.Features;
using LenovoLegionToolkit.Lib.Settings;
using LenovoLegionToolkit.Lib.Services;
using LenovoLegionToolkit.Lib.System.Management;
using LenovoLegionToolkit.Lib.Utils;
using LenovoLegionToolkit.WPF.Resources;
using LenovoLegionToolkit.WPF.Utils;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace LenovoLegionToolkit.WPF.Pages;

public partial class SystemInfoPage : UiPage
{
    private readonly GPUOverclockController _gpuOverclockController = IoCContainer.Resolve<GPUOverclockController>();
    private readonly GPUController _gpuController = IoCContainer.Resolve<GPUController>();
    private readonly GPUPresetSettings _gpuPresetSettings = IoCContainer.Resolve<GPUPresetSettings>();
    private readonly SensorsController _sensorsController = IoCContainer.Resolve<SensorsController>();
    private readonly PowerModeFeature _powerModeFeature = IoCContainer.Resolve<PowerModeFeature>();

    private readonly GPUMonitor _gpuMonitor = new();
    private readonly GPUStabilityTester _stabilityTester;
    private readonly GamingModeService _gamingModeService = new();
    private readonly GamingPerformanceAnalyzer _performanceAnalyzer;

    private string? _currentGpuName;
    private bool _gpuOverclockSupported;
    private int _temperatureLimit = 85;
    private System.Windows.Threading.DispatcherTimer? _performanceAnalysisTimer;
    private PowerModeState _currentFanMode = PowerModeState.Balance;

    public SystemInfoPage()
    {
        InitializeComponent();
        _stabilityTester = new GPUStabilityTester(_gpuMonitor, _gpuOverclockController);
        _performanceAnalyzer = new GamingPerformanceAnalyzer(_gpuMonitor);
        _gpuMonitor.MetricsUpdated += GpuMonitor_MetricsUpdated;
        _stabilityTester.ProgressUpdated += StabilityTester_ProgressUpdated;
        _stabilityTester.TestCompleted += StabilityTester_TestCompleted;
        
        Loaded += SystemInfoPage_Loaded;
        Unloaded += SystemInfoPage_Unloaded;
    }

    private void SystemInfoPage_Unloaded(object sender, RoutedEventArgs e)
    {
        _gpuMonitor?.Stop();
        _gpuMonitor?.Dispose();
        _performanceAnalysisTimer?.Stop();
    }

    private async void SystemInfoPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDeviceInformationAsync();
        await LoadBatteryInformationAsync();
        await LoadStorageHealthAsync();
        UpdateMemoryStatus();
        UpdateGamingModeStatus();
        await LoadGPUInformationAsync();
        await LoadGPUPresetsAsync();
        await InitializePerformanceAnalyzerAsync();
        await InitializeFanControlAsync();
    }

    private async Task LoadDeviceInformationAsync()
    {
        try
        {
            var machineInfo = await Compatibility.GetMachineInformationAsync();

            _modelText.Text = machineInfo.Model;
            _machineTypeText.Text = machineInfo.MachineType;
            _serialNumberText.Text = machineInfo.SerialNumber;
            _biosVersionText.Text = machineInfo.BiosVersionRaw ?? machineInfo.BiosVersion?.ToString() ?? "Unknown";

            // Check if this is a LOQ model
            var isLOQ = Compatibility.IsLOQModel(machineInfo.Model, machineInfo.MachineType);
            _loqWarningBanner.Visibility = isLOQ ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
            _modelText.Text = "Error loading device info";
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Error loading device info: {ex.Message}");
        }
    }

    private async Task LoadGPUInformationAsync()
    {
        try
        {
            _gpuOverclockSupported = await _gpuOverclockController.IsSupportedAsync();

            _currentGpuName = await Task.Run(() => _gpuController.GetGPUName());

            if (!string.IsNullOrEmpty(_currentGpuName))
            {
                _gpuNameText.Text = _currentGpuName;
            }
            else
            {
                _gpuNameText.Text = "No NVIDIA GPU detected";
                _gpuOverclockSupported = false;
            }
        }
        catch (Exception ex)
        {
            _gpuNameText.Text = "Error detecting GPU";
            _gpuOverclockSupported = false;
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Error loading GPU info: {ex.Message}");
        }
    }

    private async Task LoadGPUPresetsAsync()
    {
        _presetsContainer.Items.Clear();

        if (!_gpuOverclockSupported || string.IsNullOrEmpty(_currentGpuName))
        {
            _gpuNotSupportedBanner.Visibility = Visibility.Visible;
            _monitoringCard.Visibility = Visibility.Collapsed;
            _stabilityTestingCard.Visibility = Visibility.Collapsed;
            _stabilityTestingHeader.Visibility = Visibility.Collapsed;
            return;
        }

        _gpuNotSupportedBanner.Visibility = Visibility.Collapsed;
        _monitoringCard.Visibility = Visibility.Visible;
        _stabilityTestingCard.Visibility = Visibility.Visible;
        _stabilityTestingHeader.Visibility = Visibility.Visible;

        var presets = _gpuPresetSettings.GetPresetsForGPU(_currentGpuName);

        foreach (var preset in presets.Presets)
        {
            var card = CreatePresetCard(preset);
            _presetsContainer.Items.Add(card);
        }

        await Task.CompletedTask;
    }

    private void GpuMonitor_MetricsUpdated(object? sender, GPUMetrics metrics)
    {
        Dispatcher.Invoke(() =>
        {
            _coreClockText.Text = $"{metrics.CoreClockMHz} MHz";
            _memoryClockText.Text = $"{metrics.MemoryClockMHz} MHz";
            _temperatureText.Text = $"{metrics.TemperatureC} °C";
            _gpuUsageText.Text = $"{metrics.UsagePercent} %";
            _powerDrawText.Text = $"{metrics.PowerDrawWatts} W";
            _perfStateText.Text = metrics.PerformanceState;

            // Temperature warning
            if (metrics.TemperatureC >= _temperatureLimit)
            {
                _tempWarningBorder.Visibility = Visibility.Visible;
                _temperatureText.Foreground = new SolidColorBrush(Colors.OrangeRed);

                // Auto-revert overclock if temperature is too high
                if (metrics.TemperatureC >= _temperatureLimit + 5) // 5°C grace period
                {
                    _ = Task.Run(async () =>
                    {
                        _gpuOverclockController.SaveState(false, GPUOverclockInfo.Zero);
                        await _gpuOverclockController.ApplyStateAsync(true);
                        
                        await Dispatcher.InvokeAsync(async () =>
                        {
                            await SnackbarHelper.ShowAsync(
                                "Temperature Alert",
                                $"Overclock automatically reverted due to high temperature ({metrics.TemperatureC}°C)",
                                SnackbarType.Warning);
                        });
                    });
                }
            }
            else
            {
                _tempWarningBorder.Visibility = Visibility.Collapsed;
                _temperatureText.Foreground = (Brush)FindResource("TextFillColorPrimaryBrush");
            }
        });
    }

    private void ToggleMonitoringButton_Click(object sender, RoutedEventArgs e)
    {
        if (_gpuMonitor.IsMonitoring)
        {
            _gpuMonitor.Stop();
            _toggleMonitoringButton.Content = "Start Monitoring";
            _toggleMonitoringButton.Icon = SymbolRegular.Play24;
        }
        else
        {
            _gpuMonitor.Start(1000);
            _toggleMonitoringButton.Content = "Stop Monitoring";
            _toggleMonitoringButton.Icon = SymbolRegular.Stop24;
        }
    }

    private async void QuickTestButton_Click(object sender, RoutedEventArgs e)
    {
        await StartStabilityTestAsync(StabilityTestDuration.Quick);
    }

    private async void ExtendedTestButton_Click(object sender, RoutedEventArgs e)
    {
        await StartStabilityTestAsync(StabilityTestDuration.Extended);
    }

    private async Task StartStabilityTestAsync(StabilityTestDuration duration)
    {
        if (_stabilityTester.IsRunning)
        {
            await SnackbarHelper.ShowAsync("Stability Test", "A test is already running.", SnackbarType.Info);
            return;
        }

        _testProgressCard.Visibility = Visibility.Visible;
        _quickTestButton.IsEnabled = false;
        _extendedTestButton.IsEnabled = false;
        _testProgressBar.IsIndeterminate = false;
        _testProgressBar.Value = 0;

        var durationName = duration == StabilityTestDuration.Quick ? "Quick" : "Extended";
        _testStatusText.Text = $"Running {durationName} stability test...";

        await _stabilityTester.StartTestAsync(duration, _temperatureLimit);
    }

    private void StabilityTester_ProgressUpdated(object? sender, StabilityTestProgress progress)
    {
        Dispatcher.Invoke(() =>
        {
            _testProgressBar.Value = progress.ProgressPercent;
            var minutes = progress.TimeRemainingSeconds / 60;
            var seconds = progress.TimeRemainingSeconds % 60;
            _testTimeText.Text = $"Time remaining: {minutes:D2}:{seconds:D2} | Current: {progress.CurrentTemperature}°C | Peak: {progress.HighestTemperature}°C";
        });
    }

    private void StabilityTester_TestCompleted(object? sender, StabilityTestResult result)
    {
        Dispatcher.Invoke(async () =>
        {
            _testProgressCard.Visibility = Visibility.Collapsed;
            _quickTestButton.IsEnabled = true;
            _extendedTestButton.IsEnabled = true;

            if (result.Success)
            {
                await SnackbarHelper.ShowAsync(
                    "Stability Test Complete",
                    $"Test passed! Peak temperature: {result.HighestTemperature}°C",
                    SnackbarType.Success);
            }
            else
            {
                await SnackbarHelper.ShowAsync(
                    "Stability Test Failed",
                    result.FailureReason ?? "Test did not complete successfully",
                    SnackbarType.Error);
            }
        });
    }

    private void CancelTestButton_Click(object sender, RoutedEventArgs e)
    {
        _stabilityTester.CancelTest();
    }

    private void TempLimitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _temperatureLimit = (int)e.NewValue;
        if (_tempLimitText != null)
        {
            _tempLimitText.Text = $"{_temperatureLimit} °C";
        }
    }

    private Border CreatePresetCard(GPUPresetSettings.GPUPreset preset)
    {
        var card = new Border
        {
            Margin = new Thickness(0, 0, 12, 12),
            Padding = new Thickness(16),
            Background = (Brush)FindResource("ControlFillColorDefaultBrush"),
            CornerRadius = new CornerRadius(8),
            MinHeight = 180
        };

        var stackPanel = new StackPanel();

        // Preset name
        var nameText = new TextBlock
        {
            Text = GetLocalizedPresetName(preset.Name),
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        };
        stackPanel.Children.Add(nameText);

        // Description
        var descText = new TextBlock
        {
            Text = GetLocalizedPresetDescription(preset.Name),
            TextWrapping = TextWrapping.Wrap,
            Foreground = (Brush)FindResource("TextFillColorSecondaryBrush"),
            Margin = new Thickness(0, 0, 0, 12)
        };
        stackPanel.Children.Add(descText);

        // Core clock delta
        var coreClockGrid = CreateInfoRow(GetString("SystemInfoPage_CoreClock") ?? "Core Clock", $"+{preset.CoreDeltaMhz} MHz");
        stackPanel.Children.Add(coreClockGrid);

        // Memory clock delta
        var memClockGrid = CreateInfoRow(GetString("SystemInfoPage_MemoryClock") ?? "Memory Clock", $"+{preset.MemoryDeltaMhz} MHz");
        stackPanel.Children.Add(memClockGrid);

        // Apply button
        var applyButton = new Wpf.Ui.Controls.Button
        {
            Content = GetString("SystemInfoPage_ApplyPreset") ?? "Apply Preset",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 16, 0, 0),
            Appearance = GetAppearanceForPreset(preset.Name)
        };
        applyButton.Click += async (s, e) => await ApplyPresetAsync(preset);
        stackPanel.Children.Add(applyButton);

        card.Child = stackPanel;
        return card;
    }

    private static string? GetString(string name)
    {
        return Resource.ResourceManager.GetString(name, Resource.Culture);
    }

    private static Grid CreateInfoRow(string label, string value)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var labelBlock = new TextBlock
        {
            Text = label + ": ",
            FontSize = 12
        };
        Grid.SetColumn(labelBlock, 0);
        grid.Children.Add(labelBlock);

        var valueBlock = new TextBlock
        {
            Text = value,
            FontWeight = FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Right,
            FontSize = 12
        };
        Grid.SetColumn(valueBlock, 1);
        grid.Children.Add(valueBlock);

        return grid;
    }

    private static string GetLocalizedPresetName(string presetName)
    {
        return presetName switch
        {
            "Conservative" => GetString("SystemInfoPage_Preset_Conservative") ?? "Conservative",
            "Balanced" => GetString("SystemInfoPage_Preset_Balanced") ?? "Balanced",
            "Aggressive" => GetString("SystemInfoPage_Preset_Aggressive") ?? "Aggressive",
            _ => presetName
        };
    }

    private static string GetLocalizedPresetDescription(string presetName)
    {
        return presetName switch
        {
            "Conservative" => GetString("SystemInfoPage_Preset_Conservative_Description") ?? "Safe overclock with minimal risk. Good for everyday use.",
            "Balanced" => GetString("SystemInfoPage_Preset_Balanced_Description") ?? "Moderate overclock for improved performance.",
            "Aggressive" => GetString("SystemInfoPage_Preset_Aggressive_Description") ?? "Maximum performance. May cause instability on some systems.",
            _ => string.Empty
        };
    }

    private static ControlAppearance GetAppearanceForPreset(string presetName)
    {
        return presetName switch
        {
            "Conservative" => ControlAppearance.Success,
            "Balanced" => ControlAppearance.Primary,
            "Aggressive" => ControlAppearance.Caution,
            _ => ControlAppearance.Secondary
        };
    }

    private async Task ApplyPresetAsync(GPUPresetSettings.GPUPreset preset)
    {
        try
        {
            var info = new GPUOverclockInfo(preset.CoreDeltaMhz, preset.MemoryDeltaMhz);
            _gpuOverclockController.SaveState(true, info);
            await _gpuOverclockController.ApplyStateAsync(true);

            // Save active preset
            _gpuPresetSettings.Store.ActivePresetName = preset.Name;
            _gpuPresetSettings.SynchronizeStore();

            // Show success notification via snackbar
            await SnackbarHelper.ShowAsync(
                GetString("SystemInfoPage_PresetApplied") ?? "Preset applied successfully",
                $"{GetLocalizedPresetName(preset.Name)}: +{preset.CoreDeltaMhz}MHz Core, +{preset.MemoryDeltaMhz}MHz Memory");
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Error applying preset: {ex.Message}");
        }
    }

    // Gaming Mode Methods
    private async void EnableGamingModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_gamingModeService.IsGamingModeActive)
        {
            await _gamingModeService.DisableGamingModeAsync();
            UpdateGamingModeStatus();
            await SnackbarHelper.ShowAsync("Gaming Mode", "Gaming mode deactivated", SnackbarType.Info);
        }
        else
        {
            _enableGamingModeButton.IsEnabled = false;
            var result = await _gamingModeService.EnableGamingModeAsync();
            _enableGamingModeButton.IsEnabled = true;

            if (result.success)
            {
                UpdateGamingModeStatus();
                await SnackbarHelper.ShowAsync("Gaming Mode", result.message, SnackbarType.Success);
            }
            else
            {
                await SnackbarHelper.ShowAsync("Gaming Mode", result.message, SnackbarType.Error);
            }
        }
    }

    private void UpdateGamingModeStatus()
    {
        if (_gamingModeService.IsGamingModeActive)
        {
            _gamingModeStatusText.Text = "Status: Active";
            _gamingModeStatusText.Foreground = new SolidColorBrush(Colors.LimeGreen);
            _enableGamingModeButton.Content = "Disable Gaming Mode";
            _enableGamingModeButton.Icon = SymbolRegular.Stop24;
            _enableGamingModeButton.Appearance = ControlAppearance.Caution;
        }
        else
        {
            _gamingModeStatusText.Text = "Status: Inactive";
            _gamingModeStatusText.Foreground = (Brush)FindResource("TextFillColorSecondaryBrush");
            _enableGamingModeButton.Content = "Enable Gaming Mode";
            _enableGamingModeButton.Icon = SymbolRegular.Play24;
            _enableGamingModeButton.Appearance = ControlAppearance.Primary;
        }
    }

    // Memory Methods
    private async void ClearMemoryButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _clearMemoryButton.IsEnabled = false;
            var freedBytes = await Lib.System.Memory.ClearWorkingSetsAsync();
            await Lib.System.Memory.ClearStandbyListAsync();
            UpdateMemoryStatus();
            _clearMemoryButton.IsEnabled = true;

            await SnackbarHelper.ShowAsync(
                "Memory Cleared",
                $"Freed {Lib.System.Memory.FormatBytes(freedBytes)} of RAM",
                SnackbarType.Success);
        }
        catch (Exception ex)
        {
            _clearMemoryButton.IsEnabled = true;
            await SnackbarHelper.ShowAsync("Error", $"Failed to clear memory: {ex.Message}", SnackbarType.Error);
        }
    }

    private void UpdateMemoryStatus()
    {
        try
        {
            var (total, available, usage) = Lib.System.Memory.GetMemoryStatus();
            _totalMemoryText.Text = Lib.System.Memory.FormatBytes(total);
            _availableMemoryText.Text = Lib.System.Memory.FormatBytes(available);
            _memoryUsageText.Text = $"{usage:F1}%";
        }
        catch
        {
            _totalMemoryText.Text = "N/A";
            _availableMemoryText.Text = "N/A";
            _memoryUsageText.Text = "N/A";
        }
    }

    // Battery Methods
    private async Task LoadBatteryInformationAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var batteryInfo = Lib.System.Battery.GetBatteryInformation();

                Dispatcher.Invoke(() =>
                {
                    if (batteryInfo.FullChargeCapacity > 0 && batteryInfo.DesignCapacity > 0)
                    {
                        var health = (double)batteryInfo.FullChargeCapacity / batteryInfo.DesignCapacity * 100;
                        _batteryHealthText.Text = $"{health:F1}%";

                        // Show warning if health is below 60%
                        if (health < 60)
                        {
                            _batteryWarningBorder.Visibility = Visibility.Visible;
                            _batteryWarningText.Text = "Battery health is significantly degraded. Consider replacement soon.";
                        }
                        else if (health < 80)
                        {
                            _batteryWarningBorder.Visibility = Visibility.Visible;
                            _batteryWarningBorder.Background = (Brush)FindResource("SystemFillColorAttentionBackgroundBrush");
                            _batteryWarningText.Text = "Battery health is slightly degraded. Monitor performance.";
                        }
                        else
                        {
                            _batteryWarningBorder.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        _batteryHealthText.Text = "N/A";
                    }

                    _batteryCycleText.Text = batteryInfo.CycleCount > 0 ? batteryInfo.CycleCount.ToString() : "N/A";
                    _batteryTempText.Text = batteryInfo.BatteryTemperatureC > 0 ? $"{batteryInfo.BatteryTemperatureC}°C" : "N/A";
                    _batteryDesignCapacityText.Text = batteryInfo.DesignCapacity > 0 ? $"{batteryInfo.DesignCapacity} mWh" : "N/A";
                    _batteryFullChargeText.Text = batteryInfo.FullChargeCapacity > 0 ? $"{batteryInfo.FullChargeCapacity} mWh" : "N/A";
                    
                    var chargeRate = batteryInfo.IsCharging ? batteryInfo.DischargeRate : -batteryInfo.DischargeRate;
                    _batteryChargeRateText.Text = $"{chargeRate / 1000.0:F1}W";
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    _batteryHealthText.Text = "Error";
                });
                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"Error loading battery info: {ex.Message}");
            }
        });
    }

    // Fan Control Methods
    private async void FanSilentButton_Click(object sender, RoutedEventArgs e)
    {
        await SetFanModeAsync(PowerModeState.Quiet, "Silent");
    }

    private async void FanBalancedButton_Click(object sender, RoutedEventArgs e)
    {
        await SetFanModeAsync(PowerModeState.Balance, "Balanced");
    }

    private async void FanMaxButton_Click(object sender, RoutedEventArgs e)
    {
        await SetFanModeAsync(PowerModeState.Performance, "Performance");
    }

    private async Task SetFanModeAsync(PowerModeState mode, string modeName)
    {
        try
        {
            // Use the PowerModeFeature to set the fan/power mode
            await _powerModeFeature.SetStateAsync(mode);
            _currentFanMode = mode;
            
            // Update fan speed display
            await UpdateFanSpeedAsync();
            
            // Update button appearances
            UpdateFanButtonAppearances(mode);
            
            await SnackbarHelper.ShowAsync("Fan Control", $"Fan mode set to {modeName}", SnackbarType.Success);
        }
        catch (PowerModeUnavailableWithoutACException)
        {
            await SnackbarHelper.ShowAsync("Fan Control", "Performance mode requires AC power adapter", SnackbarType.Warning);
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Failed to set fan mode: {ex.Message}");
            await SnackbarHelper.ShowAsync("Error", $"Failed to set fan mode: {ex.Message}", SnackbarType.Error);
        }
    }

    private void UpdateFanButtonAppearances(PowerModeState currentMode)
    {
        // Visual feedback for the selected mode
        Dispatcher.Invoke(() =>
        {
            // Reset all buttons
            if (FindName("_fanSilentButton") is Wpf.Ui.Controls.Button silentBtn)
                silentBtn.Appearance = currentMode == PowerModeState.Quiet ? ControlAppearance.Primary : ControlAppearance.Secondary;
            if (FindName("_fanBalancedButton") is Wpf.Ui.Controls.Button balancedBtn)
                balancedBtn.Appearance = currentMode == PowerModeState.Balance ? ControlAppearance.Primary : ControlAppearance.Secondary;
            if (FindName("_fanMaxButton") is Wpf.Ui.Controls.Button maxBtn)
                maxBtn.Appearance = currentMode == PowerModeState.Performance ? ControlAppearance.Primary : ControlAppearance.Secondary;
        });
    }

    private async Task UpdateFanSpeedAsync()
    {
        try
        {
            // Get actual fan speeds from sensors controller
            var sensorData = await _sensorsController.GetDataAsync();
            var cpuFanSpeed = sensorData.CPU.FanSpeed;
            var gpuFanSpeed = sensorData.GPU.FanSpeed;
            
            Dispatcher.Invoke(() =>
            {
                if (cpuFanSpeed > 0 || gpuFanSpeed > 0)
                {
                    _fanSpeedText.Text = $"CPU: {cpuFanSpeed} RPM | GPU: {gpuFanSpeed} RPM";
                }
                else
                {
                    _fanSpeedText.Text = "Monitoring...";
                }
            });
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Failed to get fan speed: {ex.Message}");
            Dispatcher.Invoke(() => _fanSpeedText.Text = "N/A");
        }
    }

    private async Task InitializeFanControlAsync()
    {
        try
        {
            // Get current power mode
            _currentFanMode = await _powerModeFeature.GetStateAsync();
            UpdateFanButtonAppearances(_currentFanMode);
            await UpdateFanSpeedAsync();
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Failed to initialize fan control: {ex.Message}");
        }
    }

    // Storage Health Methods
    private async Task LoadStorageHealthAsync()
    {
        await RefreshStorageHealthAsync();
    }

    private async void RefreshStorageButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshStorageHealthAsync();
    }

    private async Task RefreshStorageHealthAsync()
    {
        try
        {
            _refreshStorageButton.IsEnabled = false;
            var drives = await Lib.System.Storage.GetDriveHealthInfoAsync();

            _storageItemsControl.ItemsSource = drives.Select(d => new
            {
                d.Model,
                d.DeviceName,
                HealthPercentage = d.HealthPercentage.HasValue ? $"{d.HealthPercentage}%" : "N/A",
                TemperatureC = d.TemperatureC.HasValue ? $"{d.TemperatureC}°C" : "N/A",
                d.MediaType
            }).ToList();

            _refreshStorageButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _refreshStorageButton.IsEnabled = true;
            await SnackbarHelper.ShowAsync("Error", $"Failed to load storage health: {ex.Message}", SnackbarType.Error);
        }
    }

    // Gaming Performance Analyzer Methods
    private async Task InitializePerformanceAnalyzerAsync()
    {
        try
        {
            // Start GPU monitoring for performance analysis
            if (!_gpuMonitor.IsMonitoring)
            {
                _gpuMonitor.Start(1000);
            }

            // Initial game detection
            await RefreshGameDetectionAsync();

            // Set up timer for periodic updates (every 2 seconds)
            _performanceAnalysisTimer = new System.Windows.Threading.DispatcherTimer();
            _performanceAnalysisTimer.Interval = TimeSpan.FromSeconds(2);
            _performanceAnalysisTimer.Tick += async (s, e) => await UpdatePerformanceMetricsAsync();
            _performanceAnalysisTimer.Start();
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Error initializing performance analyzer: {ex.Message}");
        }
    }

    private async void RefreshGameDetectionButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshGameDetectionAsync();
    }

    private async Task RefreshGameDetectionAsync()
    {
        try
        {
            var detectedGame = await _performanceAnalyzer.DetectRunningGameAsync();
            var gameInfo = _performanceAnalyzer.GetDetectedGameInfo();
            
            if (detectedGame != null && gameInfo != null)
            {
                _detectedGameText.Text = $"{gameInfo.DisplayName} ({gameInfo.Category})";
                _exampleIndicator.Text = ""; // Hide example indicator when real game detected
            }
            else
            {
                _detectedGameText.Text = "Rocket League";
                _exampleIndicator.Text = " (Example)"; // Show example indicator
            }
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Error detecting game: {ex.Message}");
        }
    }

    private async Task UpdatePerformanceMetricsAsync()
    {
        try
        {
            // Refresh game detection periodically
            await RefreshGameDetectionAsync();
            
            // Analyze performance using cached metrics from the analyzer
            var metrics = _performanceAnalyzer.AnalyzeCurrentPerformance();

            // Update UI on dispatcher thread
            Dispatcher.Invoke(() =>
            {
                // Update thermal headroom
                _thermalHeadroomBar.Value = metrics.ThermalHeadroomPercent;
                _thermalHeadroomText.Text = $"{metrics.ThermalHeadroomPercent:F1}%";

                // Update power headroom
                _powerHeadroomBar.Value = metrics.PowerHeadroomPercent;
                _powerHeadroomText.Text = $"{metrics.PowerHeadroomPercent:F1}%";

                // Update GPU temperature
                _performanceGpuTempText.Text = $"{metrics.CurrentTemperatureCelsius}°C";

                // Update sustainable FPS with game context
                if (metrics.DetectedGameInfo != null)
                {
                    _sustainableFpsText.Text = $"{metrics.EstimatedSustainableFps} FPS (Medium)";
                }
                else
                {
                    _sustainableFpsText.Text = $"{metrics.EstimatedSustainableFps} FPS";
                }

                // Update performance profile with color coding
                _performanceProfileText.Text = $"Profile: {metrics.PerformanceProfile}";
                _performanceProfileBorder.Background = metrics.PerformanceProfile switch
                {
                    "Excellent" => new SolidColorBrush(Color.FromArgb(30, 0, 200, 0)),
                    "Good" => new SolidColorBrush(Color.FromArgb(30, 0, 150, 200)),
                    "Fair" => new SolidColorBrush(Color.FromArgb(30, 200, 150, 0)),
                    _ => new SolidColorBrush(Color.FromArgb(30, 200, 0, 0))
                };

                // Update FPS predictions
                UpdateFPSPredictions(metrics);

                // Update preset recommendation
                _presetRecommendationText.Text = _performanceAnalyzer.GetGodModePresetRecommendation(metrics);
            });
        }
        catch (Exception ex)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Error updating performance metrics: {ex.Message}");
        }
    }

    private void UpdateFPSPredictions(GamingPerformanceAnalyzer.GamePerformanceMetrics metrics)
    {
        _fpsPredictionsPanel.Children.Clear();

        // Show game-specific header if a game is detected
        if (metrics.DetectedGameInfo != null)
        {
            var headerBlock = new TextBlock
            {
                Text = $"FPS Predictions for {metrics.DetectedGameInfo.DisplayName}:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = (Brush)FindResource("TextFillColorPrimaryBrush")
            };
            _fpsPredictionsPanel.Children.Add(headerBlock);
        }

        var predictions = _performanceAnalyzer.GetFPSPredictions(metrics);

        foreach (var prediction in predictions)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text = prediction.Key,
                Foreground = (Brush)FindResource("TextFillColorSecondaryBrush"),
                FontSize = 11
            };
            Grid.SetColumn(nameBlock, 0);
            grid.Children.Add(nameBlock);

            var fpsBlock = new TextBlock
            {
                Text = $"{prediction.Value} FPS",
                FontWeight = FontWeights.Medium,
                FontSize = 11
            };
            Grid.SetColumn(fpsBlock, 1);
            grid.Children.Add(fpsBlock);

            _fpsPredictionsPanel.Children.Add(grid);
        }
    }
}

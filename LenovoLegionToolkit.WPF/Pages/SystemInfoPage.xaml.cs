using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Controllers;
using LenovoLegionToolkit.Lib.Settings;
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

    private readonly GPUMonitor _gpuMonitor = new();
    private readonly GPUStabilityTester _stabilityTester;

    private string? _currentGpuName;
    private bool _gpuOverclockSupported;
    private int _temperatureLimit = 85;

    public SystemInfoPage()
    {
        InitializeComponent();
        _stabilityTester = new GPUStabilityTester(_gpuMonitor, _gpuOverclockController);
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
    }

    private async void SystemInfoPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDeviceInformationAsync();
        await LoadGPUInformationAsync();
        await LoadGPUPresetsAsync();
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
                    Task.Run(async () =>
                    {
                        _gpuOverclockController.SaveState(false, GPUOverclockInfo.Zero);
                        await _gpuOverclockController.ApplyStateAsync(true);
                        
                        Dispatcher.Invoke(async () =>
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
}

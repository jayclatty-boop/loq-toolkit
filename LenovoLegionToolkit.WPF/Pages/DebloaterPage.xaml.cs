using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Debloater;
using LenovoLegionToolkit.Lib.Debloater.Tweaks;
using LenovoLegionToolkit.Lib.Settings;
using LenovoLegionToolkit.WPF.Controls;
using CustomControls = LenovoLegionToolkit.WPF.Controls.Custom;
using LenovoLegionToolkit.WPF.Utils;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace LenovoLegionToolkit.WPF.Pages;

public partial class DebloaterPage : UiPage
{
    private readonly DebloaterSettings _settings = IoCContainer.Resolve<DebloaterSettings>();
    private readonly DebloaterEngine _engine = IoCContainer.Resolve<DebloaterEngine>();

    private readonly IReadOnlyList<IDebloatTweak> _tweaks = DebloaterCatalog.CreateDefaultTweaks();
    private readonly IReadOnlyList<DebloatProfile> _profiles = DebloaterCatalog.CreateDefaultProfiles();

    private readonly Dictionary<string, Wpf.Ui.Controls.ToggleSwitch> _toggleByTweakId = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CustomControls.CardControl> _cardByTweakId = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DebloatTweakStatus> _statusCache = new(StringComparer.OrdinalIgnoreCase);

    private bool _isRefreshing;
    private string _searchFilter = string.Empty;

    public DebloaterPage()
    {
        InitializeComponent();

        Loaded += DebloaterPage_Loaded;
    }

    private async void DebloaterPage_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
        await RefreshStatusAsync();
    }

    private Task RefreshAsync()
    {
        _isRefreshing = true;

        _restorePointToggle.IsChecked = _settings.Store.CreateRestorePoint;

        _scopeComboBox.Items.Clear();
        _scopeComboBox.Items.Add(new ComboBoxItem { Content = "Third-party only", Tag = BloatwareScope.ThirdPartyOnly });
        _scopeComboBox.Items.Add(new ComboBoxItem { Content = "Include Microsoft apps", Tag = BloatwareScope.IncludeMicrosoftApps });
        SelectComboByTag(_scopeComboBox, _settings.Store.BloatwareScope);

        _profileComboBox.Items.Clear();
        _profileComboBox.Items.Add(new ComboBoxItem { Content = "Custom", Tag = (string?)null });
        foreach (var profile in _profiles)
            _profileComboBox.Items.Add(new ComboBoxItem { Content = profile.Title, Tag = profile.Id });
        SelectComboByTag(_profileComboBox, _settings.Store.SelectedProfileId);

        BuildTweakUI();

        _statusText.Text = WindowsAdmin.IsAdministrator() ? "Running as administrator" : "Not running as administrator";

        _isRefreshing = false;
        return Task.CompletedTask;
    }

    private async Task RefreshStatusAsync()
    {
        _statusCache.Clear();

        foreach (var tweak in _tweaks)
        {
            try
            {
                var status = await tweak.GetStatusAsync();
                _statusCache[tweak.Id] = status;
            }
            catch
            {
                _statusCache[tweak.Id] = DebloatTweakStatus.Unknown;
            }
        }

        // Update UI with status badges
        foreach (var (id, card) in _cardByTweakId)
        {
            UpdateCardStatus(card, _statusCache.GetValueOrDefault(id, DebloatTweakStatus.Unknown));
        }
    }

    private void UpdateCardStatus(CustomControls.CardControl card, DebloatTweakStatus status)
    {
        // Find or create status badge
        if (card.Content is StackPanel panel)
        {
            var statusBadge = panel.Children.OfType<Border>().FirstOrDefault(b => b.Tag?.ToString() == "StatusBadge");
            
            if (statusBadge == null)
            {
                statusBadge = new Border
                {
                    Tag = "StatusBadge",
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(6, 2, 6, 2),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                
                var statusText = new TextBlock
                {
                    FontSize = 11,
                    FontWeight = FontWeights.Medium
                };
                statusBadge.Child = statusText;
                
                panel.Children.Insert(0, statusBadge);
            }

            var textBlock = statusBadge.Child as TextBlock;
            if (textBlock != null)
            {
                switch (status)
                {
                    case DebloatTweakStatus.Applied:
                        textBlock.Text = "✓ Applied";
                        statusBadge.Background = new SolidColorBrush(Color.FromArgb(80, 16, 185, 129)); // Green with transparency
                        textBlock.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                        break;
                    case DebloatTweakStatus.NotApplied:
                        textBlock.Text = "○ Not Applied";
                        statusBadge.Background = new SolidColorBrush(Color.FromArgb(80, 156, 163, 175)); // Gray with transparency
                        textBlock.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
                        break;
                    default:
                        textBlock.Text = "? Unknown";
                        statusBadge.Background = new SolidColorBrush(Color.FromArgb(80, 251, 191, 36)); // Yellow with transparency
                        textBlock.Foreground = new SolidColorBrush(Color.FromRgb(251, 191, 36));
                        break;
                }
            }
        }
    }

    private void BuildTweakUI()
    {
        _categoriesPanel.Children.Clear();
        _toggleByTweakId.Clear();
        _cardByTweakId.Clear();

        foreach (var group in _tweaks.GroupBy(t => t.Category).OrderBy(g => g.Key))
        {
            var expander = new CustomControls.CardExpander
            {
                Margin = new Thickness(0, 0, 0, 8),
                IsExpanded = group.Key is DebloatCategory.Privacy or DebloatCategory.Performance,
                Header = new CardHeaderControl
                {
                    Title = group.Key.ToString(),
                    Subtitle = GetCategorySubtitle(group.Key),
                }
            };

            var panel = new StackPanel();

            foreach (var tweak in group.OrderBy(t => t.Title, StringComparer.OrdinalIgnoreCase))
            {
                var card = new CustomControls.CardControl { Margin = new Thickness(0, 0, 0, 8) };

                var header = new CardHeaderControl
                {
                    Title = tweak.Title,
                    Subtitle = tweak.Description,
                };

                var contentPanel = new StackPanel();

                var toggle = new Wpf.Ui.Controls.ToggleSwitch
                {
                    Margin = new Thickness(0, 0, 0, 8),
                    Content = BuildToggleContent(tweak),
                    IsChecked = _settings.Store.SelectedTweaks.TryGetValue(tweak.Id, out var selected) && selected,
                    IsEnabled = !(tweak.Severity == DebloatSeverity.Dangerous && _settings.Store.LockDangerousTweaks),
                };

                toggle.Click += (_, _) =>
                {
                    if (_isRefreshing)
                        return;

                    // Prevent enabling dangerous tweaks if locked
                    if (tweak.Severity == DebloatSeverity.Dangerous && _settings.Store.LockDangerousTweaks && toggle.IsChecked == true)
                    {
                        toggle.IsChecked = false;
                        _ = SnackbarHelper.ShowAsync("Dangerous tweaks locked", "Dangerous tweaks are currently locked. Unlock them in settings first.");
                        return;
                    }

                    _settings.Store.SelectedProfileId = null; // becomes custom
                    _settings.Store.SelectedTweaks[tweak.Id] = toggle.IsChecked ?? false;
                    _settings.SynchronizeStore();
                    SelectComboByTag(_profileComboBox, (string?)null);
                };

                contentPanel.Children.Add(toggle);

                card.Header = header;
                card.Content = contentPanel;

                _toggleByTweakId[tweak.Id] = toggle;
                _cardByTweakId[tweak.Id] = card;
                
                // Apply status if cached
                if (_statusCache.TryGetValue(tweak.Id, out var status))
                {
                    UpdateCardStatus(card, status);
                }

                // Apply search filter
                ApplySearchFilter(card, tweak);

                panel.Children.Add(card);
            }

            expander.Content = panel;
            _categoriesPanel.Children.Add(expander);
        }
    }

    private void ApplySearchFilter(CustomControls.CardControl card, IDebloatTweak tweak)
    {
        if (string.IsNullOrWhiteSpace(_searchFilter))
        {
            card.Visibility = Visibility.Visible;
            return;
        }

        var searchLower = _searchFilter.ToLowerInvariant();
        var matches = tweak.Title.ToLowerInvariant().Contains(searchLower) ||
                     tweak.Description.ToLowerInvariant().Contains(searchLower) ||
                     tweak.Category.ToString().ToLowerInvariant().Contains(searchLower);

        card.Visibility = matches ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string GetCategorySubtitle(DebloatCategory category) => category switch
    {
        DebloatCategory.Privacy => "Privacy-related settings.",
        DebloatCategory.Performance => "Performance-related settings.",
        DebloatCategory.Visual => "Explorer and UI preferences.",
        DebloatCategory.Services => "Windows services (admin required).",
        DebloatCategory.Bloatware => "App removals (can be destructive).",
        DebloatCategory.Advanced => "High-risk changes.",
        _ => string.Empty
    };

    private static string BuildToggleContent(IDebloatTweak tweak)
    {
        var tags = new List<string>();

        if (tweak.IsAdminRequired)
            tags.Add("Admin");

        if (tweak.Severity == DebloatSeverity.Dangerous)
            tags.Add("Dangerous");
        else if (tweak.Severity == DebloatSeverity.Caution)
            tags.Add("Caution");

        if (tags.Count == 0)
            return "Enabled";

        return $"Enabled ({string.Join(", ", tags)})";
    }

    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
            return;

        if (_profileComboBox.SelectedItem is not ComboBoxItem item)
            return;

        var profileId = item.Tag as string;
        _settings.Store.SelectedProfileId = profileId;

        if (string.IsNullOrEmpty(profileId))
        {
            _settings.SynchronizeStore();
            return;
        }

        var profile = _profiles.FirstOrDefault(p => p.Id.Equals(profileId, StringComparison.OrdinalIgnoreCase));
        if (profile is null)
        {
            _settings.SynchronizeStore();
            return;
        }

        foreach (var tweak in _tweaks)
        {
            var selected = profile.TweakIds.Contains(tweak.Id, StringComparer.OrdinalIgnoreCase);
            _settings.Store.SelectedTweaks[tweak.Id] = selected;
            if (_toggleByTweakId.TryGetValue(tweak.Id, out var toggle))
                toggle.IsChecked = selected;
        }

        _settings.SynchronizeStore();
    }

    private void ScopeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
            return;

        if (_scopeComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not BloatwareScope scope)
            return;

        _settings.Store.BloatwareScope = scope;
        _settings.SynchronizeStore();
    }

    private void RestorePointToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
            return;

        _settings.Store.CreateRestorePoint = _restorePointToggle.IsChecked ?? false;
        _settings.SynchronizeStore();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchFilter = _searchBox.Text ?? string.Empty;
        
        // Re-apply filter to all cards
        foreach (var (id, card) in _cardByTweakId)
        {
            var tweak = _tweaks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (tweak != null)
            {
                ApplySearchFilter(card, tweak);
            }
        }
    }

    private async void RefreshStatusButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshStatusAsync();
        await SnackbarHelper.ShowAsync("Debloater", "Status refreshed for all tweaks.", SnackbarType.Info);
    }

    private async void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedTweakIds();
        if (selected.Count == 0)
        {
            await SnackbarHelper.ShowAsync("Debloater", "No tweaks selected.", SnackbarType.Info);
            return;
        }

        var isAdmin = WindowsAdmin.IsAdministrator();
        if (_settings.Store.CreateRestorePoint && !isAdmin)
        {
            await MessageBoxHelper.ShowAsync(this, "Administrator required", "Creating a restore point requires running as administrator.", "OK", "Cancel");
            return;
        }

        if (_engine.RequiresAdmin(selected) && !isAdmin)
        {
            await MessageBoxHelper.ShowAsync(this, "Administrator required", "One or more selected tweaks require administrator privileges.", "OK", "Cancel");
            return;
        }

        if (SelectedIncludesDangerous(selected))
        {
            var ok = await MessageBoxHelper.ShowAsync(this, "Confirm", "Some selected tweaks are marked as dangerous (e.g., app removal / services). Continue?", "Yes", "No");
            if (!ok)
                return;
        }

        await RunWithLoaderAsync(async token =>
        {
            // Show progress card
            Dispatcher.Invoke(() =>
            {
                _progressCard.Visibility = Visibility.Visible;
                _progressLog.Text = "";
            });

            var progress = new Progress<string>(s =>
            {
                Dispatcher.Invoke(() =>
                {
                    _statusText.Text = s;
                    _progressLog.Text += $"[{DateTime.Now:HH:mm:ss}] {s}\n";
                    
                    // Auto-scroll to bottom
                    if (_progressLog.Parent is ScrollViewer sv)
                    {
                        sv.ScrollToEnd();
                    }
                });
            });

            // Handle bloatware removal separately (it is not reliably undoable and depends on scope).
            if (selected.Contains(RemoveBloatwareAppsTweak.TweakId, StringComparer.OrdinalIgnoreCase))
            {
                ((IProgress<string>)progress).Report("Removing apps...");

                var wildcards = new List<string>();
                wildcards.AddRange(RemoveBloatwareAppsTweak.ThirdPartyWildcards);

                if (_settings.Store.BloatwareScope == BloatwareScope.IncludeMicrosoftApps)
                    wildcards.AddRange(RemoveBloatwareAppsTweak.MicrosoftWildcards);

                // Current user removal; no provisioned removal.
                await RemoveBloatwareAppsTweak.RemoveAppsAsync(wildcards, allUsers: false, removeProvisioned: false, token);
                ((IProgress<string>)progress).Report("App removal completed.");
            }

            var engineIds = selected.Where(id => !id.Equals(RemoveBloatwareAppsTweak.TweakId, StringComparison.OrdinalIgnoreCase));
            await _engine.ApplyAsync(engineIds, _settings.Store.CreateRestorePoint, "Lenovo LOQ Toolkit Debloater", progress, token);

            foreach (var id in selected)
                _settings.Store.SelectedTweaks[id] = true;
            _settings.SynchronizeStore();

            ((IProgress<string>)progress).Report("All tweaks applied successfully!");

            // Refresh status after applying
            await RefreshStatusAsync();

            // Hide progress card after delay
            await Task.Delay(2000);
            Dispatcher.Invoke(() => _progressCard.Visibility = Visibility.Collapsed);

            await SnackbarHelper.ShowAsync("Debloater", "Selected tweaks applied.");
        });
    }

    private async void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedTweakIds();
        if (selected.Count == 0)
        {
            await SnackbarHelper.ShowAsync("Debloater", "No tweaks selected.", SnackbarType.Info);
            return;
        }

        var undoable = _tweaks.Where(t => selected.Contains(t.Id, StringComparer.OrdinalIgnoreCase) && t.SupportsUndo).Select(t => t.Id).ToArray();
        if (undoable.Length == 0)
        {
            await SnackbarHelper.ShowAsync("Debloater", "No selected tweaks support undo.", SnackbarType.Info);
            return;
        }

        var isAdmin = WindowsAdmin.IsAdministrator();
        if (_engine.RequiresAdmin(undoable) && !isAdmin)
        {
            await MessageBoxHelper.ShowAsync(this, "Administrator required", "Undoing one or more selected tweaks requires administrator privileges.", "OK", "Cancel");
            return;
        }

        var ok = await MessageBoxHelper.ShowAsync(this, "Confirm", "Undo selected tweaks that support undo?", "Yes", "No");
        if (!ok)
            return;

        await RunWithLoaderAsync(async token =>
        {
            var progress = new Progress<string>(s => _statusText.Text = s);
            await _engine.UndoAsync(undoable, progress, token);

            foreach (var id in undoable)
            {
                _settings.Store.SelectedTweaks[id] = false;
                if (_toggleByTweakId.TryGetValue(id, out var toggle))
                    toggle.IsChecked = false;
            }

            _settings.Store.SelectedProfileId = null;
            SelectComboByTag(_profileComboBox, (string?)null);

            _settings.SynchronizeStore();

            await SnackbarHelper.ShowAsync("Debloater", "Undo completed.");
        });
    }

    private List<string> GetSelectedTweakIds()
    {
        var selected = new List<string>();
        foreach (var (id, toggle) in _toggleByTweakId)
        {
            if (toggle.IsChecked ?? false)
                selected.Add(id);
        }

        return selected;
    }

    private bool SelectedIncludesDangerous(IEnumerable<string> tweakIds)
    {
        foreach (var id in tweakIds)
        {
            var tweak = _tweaks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (tweak?.Severity == DebloatSeverity.Dangerous)
                return true;
        }

        return false;
    }

    private async Task RunWithLoaderAsync(Func<CancellationToken, Task> work)
    {
        _applyButton.IsEnabled = false;
        _undoButton.IsEnabled = false;
        _loader.Visibility = Visibility.Visible;

        using var cts = new CancellationTokenSource();

        try
        {
            await work(cts.Token);
        }
        catch (Exception ex)
        {
            await SnackbarHelper.ShowAsync("Debloater", ex.Message, SnackbarType.Error);
        }
        finally
        {
            _loader.Visibility = Visibility.Hidden;
            _applyButton.IsEnabled = true;
            _undoButton.IsEnabled = true;
            _statusText.Text = WindowsAdmin.IsAdministrator() ? "Running as administrator" : "Not running as administrator";
        }
    }

    private static void SelectComboByTag(ComboBox comboBox, object? tag)
    {
        foreach (var obj in comboBox.Items)
        {
            if (obj is ComboBoxItem item && Equals(item.Tag, tag))
            {
                comboBox.SelectedItem = item;
                return;
            }
        }

        comboBox.SelectedIndex = 0;
    }
}

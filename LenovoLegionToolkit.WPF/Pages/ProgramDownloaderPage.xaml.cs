using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Downloader;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace LenovoLegionToolkit.WPF.Pages;

public partial class ProgramDownloaderPage : Page
{
    private readonly ProgramDownloadManager _downloadManager;
    private List<Program> _allPrograms = new();
    private string _currentCategoryFilter = "All";

    public ProgramDownloaderPage()
    {
        InitializeComponent();
        _downloadManager = new ProgramDownloadManager();
        _downloadManager.ProgressChanged += DownloadManager_ProgressChanged;
        _downloadManager.DownloadCompleted += DownloadManager_DownloadCompleted;
        LoadPrograms();
        BuildCategoryFilters();
    }

    private void LoadPrograms()
    {
        _allPrograms = _downloadManager.GetDefaultPrograms();
        DisplayPrograms(_allPrograms);
    }

    private void BuildCategoryFilters()
    {
        var categories = new[] { "All" }.Concat(
            Enum.GetValues(typeof(ProgramCategory))
                .Cast<ProgramCategory>()
                .Select(c => c.ToString())
        ).ToList();

        foreach (var category in categories)
        {
            var btn = new ui.Button
            {
                Content = category,
                Margin = new Thickness(5),
                Padding = new Thickness(12, 6, 12, 6),
                Tag = category,
            };
            btn.Click += CategoryButton_Click;
            CategoryFilterPanel.Children.Add(btn);
        }
    }

    private void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ui.Button btn && btn.Tag is string category)
        {
            _currentCategoryFilter = category;
            FilterPrograms();
        }
    }

    private void FilterPrograms()
    {
        var filtered = _currentCategoryFilter == "All"
            ? _allPrograms
            : _allPrograms.Where(p => p.Category.ToString() == _currentCategoryFilter).ToList();

        if (!string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            var search = SearchBox.Text.ToLowerInvariant();
            filtered = filtered
                .Where(p => p.Name.ToLowerInvariant().Contains(search) ||
                           p.Description.ToLowerInvariant().Contains(search))
                .ToList();
        }

        DisplayPrograms(filtered);
    }

    private void DisplayPrograms(List<Program> programs)
    {
        ProgramsPanel.Children.Clear();

        foreach (var program in programs)
        {
            var card = CreateProgramCard(program);
            ProgramsPanel.Children.Add(card);
        }
    }

    private Border CreateProgramCard(Program program)
    {
        var card = new Border
        {
            Width = 200,
            Height = 240,
            Margin = new Thickness(10),
            Padding = new Thickness(15),
            Background = (Brush)FindResource("ControlFillColorDefaultBrush"),
            BorderBrush = (Brush)FindResource("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        // Title
        var titleBlock = new TextBlock
        {
            Text = program.Name,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8)
        };
        stackPanel.Children.Add(titleBlock);

        // Description
        var descBlock = new TextBlock
        {
            Text = program.Description,
            FontSize = 11,
            Foreground = (Brush)FindResource("TextFillColorSecondaryBrush"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8),
            MaxHeight = 60
        };
        stackPanel.Children.Add(descBlock);

        // Version
        var versionBlock = new TextBlock
        {
            Text = $"Version: {program.Version}",
            FontSize = 10,
            Foreground = (Brush)FindResource("TextFillColorTertiaryBrush"),
            Margin = new Thickness(0, 0, 0, 4)
        };
        stackPanel.Children.Add(versionBlock);

        // Size
        var sizeBlock = new TextBlock
        {
            Text = $"Size: {program.SizeDescription}",
            FontSize = 10,
            Foreground = (Brush)FindResource("TextFillColorTertiaryBrush"),
            Margin = new Thickness(0, 0, 0, 12)
        };
        stackPanel.Children.Add(sizeBlock);

        // Download Button
        var downloadBtn = new ui.Button
        {
            Content = "Download",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Tag = program,
            Appearance = ui.ControlAppearance.Primary
        };
        downloadBtn.Click += async (s, e) =>
        {
            try
            {
                var result = await _downloadManager.DownloadProgramAsync(program);
                await new ContentDialog
                {
                    Title = "Download Complete",
                    Content = $"Downloaded to: {result}",
                    PrimaryButtonText = "OK"
                }.ShowAsync();
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "Download Failed",
                    Content = ex.Message,
                    PrimaryButtonText = "OK"
                }.ShowAsync();
            }
        };
        stackPanel.Children.Add(downloadBtn);

        card.Child = stackPanel;
        return card;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterPrograms();
    }

    private void DownloadManager_ProgressChanged(object? sender, DownloadItem e)
    {
        Dispatcher.Invoke(() =>
        {
            DownloadQueue.ItemsSource = _downloadManager.ActiveDownloads;
            (DownloadQueue.Parent as Expander).Header = $"Downloads ({_downloadManager.ActiveDownloads.Count})";
        });
    }

    private void DownloadManager_DownloadCompleted(object? sender, DownloadItem e)
    {
        Dispatcher.Invoke(() =>
        {
            DownloadQueue.ItemsSource = _downloadManager.ActiveDownloads;
        });
    }
}

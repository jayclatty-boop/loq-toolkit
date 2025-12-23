using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LenovoLegionToolkit.Lib.Downloader;
using Wpf.Ui.Controls;

namespace LenovoLegionToolkit.WPF.Pages;

public partial class OSDownloaderPage : Page
{
    private readonly OSDownloadManager _downloadManager;
    private List<OSImage> _currentOSImages = new();

    public OSDownloaderPage()
    {
        InitializeComponent();
        _downloadManager = new OSDownloadManager();
        
        InitializeWindowsOptions();
        InitializeLinuxOptions();
        LoadWindowsImages();

        WindowsRadio.Checked += (s, e) => SwitchToWindows();
        LinuxRadio.Checked += (s, e) => SwitchToLinux();
    }

    private void InitializeWindowsOptions()
    {
        var versions = Enum.GetValues(typeof(WindowsVersionType))
            .Cast<WindowsVersionType>()
            .Select(v => v.ToString())
            .ToList();

        WindowsVersionCombo.ItemsSource = versions;
        WindowsVersionCombo.SelectedIndex = 0;
        WindowsVersionCombo.SelectionChanged += (s, e) => LoadWindowsImages();

        var editions = Enum.GetValues(typeof(WindowsEditionType))
            .Cast<WindowsEditionType>()
            .Select(e => e.ToString())
            .ToList();

        WindowsEditionCombo.ItemsSource = editions;
        WindowsEditionCombo.SelectedIndex = 0;
        WindowsEditionCombo.SelectionChanged += (s, e) => LoadWindowsImages();
    }

    private void InitializeLinuxOptions()
    {
        var distros = Enum.GetValues(typeof(LinuxDistro))
            .Cast<LinuxDistro>()
            .Select(d => d.ToString())
            .ToList();

        LinuxDistroCombo.ItemsSource = distros;
        LinuxDistroCombo.SelectedIndex = 0;
        LinuxDistroCombo.SelectionChanged += (s, e) => LoadLinuxImages();
    }

    private void LoadWindowsImages()
    {
        if (Enum.TryParse<WindowsVersionType>(WindowsVersionCombo.SelectedItem?.ToString(), out var version) &&
            Enum.TryParse<WindowsEditionType>(WindowsEditionCombo.SelectedItem?.ToString(), out var edition))
        {
            _currentOSImages = _downloadManager.GetWindowsImages(version, edition);
            DisplayOSImages(WindowsImagesList, _currentOSImages);
        }
    }

    private void LoadLinuxImages()
    {
        if (Enum.TryParse<LinuxDistro>(LinuxDistroCombo.SelectedItem?.ToString(), out var distro))
        {
            _currentOSImages = _downloadManager.GetLinuxImages(distro);
            DisplayOSImages(LinuxImagesList, _currentOSImages);
        }
    }

    private void DisplayOSImages(ListBox listBox, List<OSImage> images)
    {
        listBox.Items.Clear();

        foreach (var image in images)
        {
            var item = CreateOSImageCard(image);
            listBox.Items.Add(item);
        }
    }

    private Border CreateOSImageCard(OSImage image)
    {
        var card = new Border
        {
            Margin = new Thickness(5),
            Padding = new Thickness(15),
            Background = (Brush)FindResource("ControlFillColorDefaultBrush"),
            BorderBrush = (Brush)FindResource("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var contentPanel = new StackPanel { Orientation = Orientation.Vertical };

        // Title
        var titleBlock = new TextBlock
        {
            Text = image.Name,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 5)
        };
        contentPanel.Children.Add(titleBlock);

        // Description
        var descBlock = new TextBlock
        {
            Text = image.Description,
            FontSize = 11,
            Foreground = (Brush)FindResource("TextFillColorSecondaryBrush"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 5)
        };
        contentPanel.Children.Add(descBlock);

        // Version and Size
        var infoBlock = new TextBlock
        {
            Text = $"Version: {image.Version} | Size: {FormatBytes(image.SizeBytes)} | Released: {image.ReleaseDate:yyyy-MM-dd}",
            FontSize = 10,
            Foreground = (Brush)FindResource("TextFillColorTertiaryBrush"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 0)
        };
        contentPanel.Children.Add(infoBlock);

        Grid.SetColumn(contentPanel, 0);
        grid.Children.Add(contentPanel);

        // Download Button
        var downloadBtn = new ui.Button
        {
            Content = "Download",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 0),
            Tag = image,
            Appearance = ui.ControlAppearance.Primary,
            Width = 100
        };

        downloadBtn.Click += async (s, e) =>
        {
            try
            {
                // Create a proper filename from the URL
                var fileName = System.IO.Path.GetFileName(new Uri(image.DownloadUrl).AbsolutePath);
                if (string.IsNullOrEmpty(fileName))
                    fileName = $"{image.Name}.iso";

                var savePath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Downloads),
                    "LOQToolkit",
                    fileName
                );

                // Create directory if it doesn't exist
                var directory = System.IO.Path.GetDirectoryName(savePath);
                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                // Show download confirmation
                var dialog = new ContentDialog
                {
                    Title = "Download Confirmation",
                    Content = $"Download {image.Name}?\n\nSize: {FormatBytes(image.SizeBytes)}\nSave Location: {savePath}",
                    PrimaryButtonText = "Download",
                    CloseButtonText = "Cancel"
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    // Open download URL in browser
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = image.DownloadUrl,
                        UseShellExecute = true
                    });

                    await new ContentDialog
                    {
                        Title = "Download Started",
                        Content = $"Your download should start automatically. If not, visit:\n{image.DownloadUrl}",
                        PrimaryButtonText = "OK"
                    }.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to process download: {ex.Message}",
                    PrimaryButtonText = "OK"
                }.ShowAsync();
            }
        };

        Grid.SetColumn(downloadBtn, 1);
        grid.Children.Add(downloadBtn);

        card.Child = grid;
        return card;
    }

    private void SwitchToWindows()
    {
        WindowsOptionsGrid.Visibility = Visibility.Visible;
        LinuxOptionsGrid.Visibility = Visibility.Collapsed;
        LoadWindowsImages();
    }

    private void SwitchToLinux()
    {
        WindowsOptionsGrid.Visibility = Visibility.Collapsed;
        LinuxOptionsGrid.Visibility = Visibility.Visible;
        LoadLinuxImages();
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

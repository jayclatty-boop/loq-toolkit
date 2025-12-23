using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LenovoLegionToolkit.Lib.System;

public class CleanupItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public static class SystemCleanup
{
    public static async Task<List<CleanupItem>> GetCleanupItemsAsync()
    {
        return await Task.Run(() =>
        {
            var items = new List<CleanupItem>();

            // Windows Temp folder
            var tempPath = Environment.GetEnvironmentVariable("TEMP");
            if (!string.IsNullOrEmpty(tempPath) && Directory.Exists(tempPath))
            {
                items.Add(new CleanupItem
                {
                    Name = "Windows Temp Files",
                    Description = "Temporary files created by Windows and applications",
                    Path = tempPath,
                    SizeBytes = GetDirectorySize(tempPath),
                    IsEnabled = true
                });
            }

            // Windows Update Cache
            var updateCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), 
                "SoftwareDistribution", "Download");
            if (Directory.Exists(updateCachePath))
            {
                items.Add(new CleanupItem
                {
                    Name = "Windows Update Cache",
                    Description = "Downloaded Windows Update files",
                    Path = updateCachePath,
                    SizeBytes = GetDirectorySize(updateCachePath),
                    IsEnabled = true
                });
            }

            // Recycle Bin
            items.Add(new CleanupItem
            {
                Name = "Recycle Bin",
                Description = "Files in Recycle Bin for all drives",
                Path = "RecycleBin",
                SizeBytes = GetRecycleBinSize(),
                IsEnabled = true
            });

            // Browser Caches
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Chrome Cache
            var chromeCachePath = Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Cache");
            if (Directory.Exists(chromeCachePath))
            {
                items.Add(new CleanupItem
                {
                    Name = "Chrome Cache",
                    Description = "Google Chrome browser cache",
                    Path = chromeCachePath,
                    SizeBytes = GetDirectorySize(chromeCachePath),
                    IsEnabled = false // Disabled by default
                });
            }

            // Edge Cache
            var edgeCachePath = Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Cache");
            if (Directory.Exists(edgeCachePath))
            {
                items.Add(new CleanupItem
                {
                    Name = "Edge Cache",
                    Description = "Microsoft Edge browser cache",
                    Path = edgeCachePath,
                    SizeBytes = GetDirectorySize(edgeCachePath),
                    IsEnabled = false // Disabled by default
                });
            }

            // Firefox Cache
            var firefoxPath = Path.Combine(localAppData, "Mozilla", "Firefox", "Profiles");
            if (Directory.Exists(firefoxPath))
            {
                try
                {
                    var profiles = Directory.GetDirectories(firefoxPath);
                    long totalSize = 0;
                    foreach (var profile in profiles)
                    {
                        var cachePath = Path.Combine(profile, "cache2");
                        if (Directory.Exists(cachePath))
                            totalSize += GetDirectorySize(cachePath);
                    }

                    if (totalSize > 0)
                    {
                        items.Add(new CleanupItem
                        {
                            Name = "Firefox Cache",
                            Description = "Mozilla Firefox browser cache",
                            Path = firefoxPath,
                            SizeBytes = totalSize,
                            IsEnabled = false // Disabled by default
                        });
                    }
                }
                catch { }
            }

            return items;
        });
    }

    public static async Task<long> CleanupAsync(List<CleanupItem> items)
    {
        return await Task.Run(() =>
        {
            long totalFreed = 0;

            foreach (var item in items.Where(i => i.IsEnabled))
            {
                try
                {
                    if (item.Path == "RecycleBin")
                    {
                        // Empty recycle bin
                        CMD.RunAsync("powershell", 
                            "-NoProfile -Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"").Wait();
                        totalFreed += item.SizeBytes;
                    }
                    else if (Directory.Exists(item.Path))
                    {
                        var sizeBefore = item.SizeBytes;
                        DeleteDirectoryContents(item.Path);
                        totalFreed += sizeBefore;
                    }
                }
                catch
                {
                    // Skip items that can't be cleaned
                }
            }

            return totalFreed;
        });
    }

    private static long GetDirectorySize(string path)
    {
        try
        {
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(file => file.Length);
        }
        catch
        {
            return 0;
        }
    }

    private static long GetRecycleBinSize()
    {
        try
        {
            long totalSize = 0;
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);

            foreach (var drive in drives)
            {
                var recycleBinPath = Path.Combine(drive.Name, "$Recycle.Bin");
                if (Directory.Exists(recycleBinPath))
                {
                    totalSize += GetDirectorySize(recycleBinPath);
                }
            }

            return totalSize;
        }
        catch
        {
            return 0;
        }
    }

    private static void DeleteDirectoryContents(string path)
    {
        try
        {
            var dirInfo = new DirectoryInfo(path);

            foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }

            foreach (var dir in dirInfo.EnumerateDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch { }
            }
        }
        catch { }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LenovoLegionToolkit.Lib.Downloader;

public class ProgramDownloadManager
{
    private readonly HttpClient _httpClient;
    private readonly string _downloadFolder;
    private readonly Dictionary<string, DownloadItem> _activeDownloads = new();
    private CancellationTokenSource _cancellationTokenSource = new();

    public event EventHandler<DownloadItem>? ProgressChanged;
    public event EventHandler<DownloadItem>? DownloadCompleted;
    public event EventHandler<(DownloadItem, string)>? DownloadFailed;

    public IReadOnlyList<DownloadItem> ActiveDownloads => _activeDownloads.Values.ToList();

    public ProgramDownloadManager(string downloadFolder = "")
    {
        _downloadFolder = string.IsNullOrEmpty(downloadFolder) 
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "LOQToolkit")
            : downloadFolder;

        if (!Directory.Exists(_downloadFolder))
            Directory.CreateDirectory(_downloadFolder);

        _httpClient = new HttpClient();
    }

    public async Task<string> DownloadProgramAsync(Program program, IProgress<DownloadItem>? progress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(program.DownloadUrl))
            throw new InvalidOperationException("Program download URL is empty");

        var fileName = Path.GetFileName(new Uri(program.DownloadUrl).AbsolutePath);
        if (string.IsNullOrEmpty(fileName))
            fileName = $"{program.Name}.exe";

        var savePath = Path.Combine(_downloadFolder, fileName);
        var downloadItem = new DownloadItem
        {
            Program = program,
            SavePath = savePath,
            Status = DownloadStatus.Downloading
        };

        _activeDownloads[downloadItem.Id] = downloadItem;

        try
        {
            using var response = await _httpClient.GetAsync(program.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            downloadItem.TotalBytes = totalBytes;

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true);

            var totalRead = 0L;
            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) != 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalRead += bytesRead;
                downloadItem.DownloadedBytes = totalRead;

                progress?.Report(downloadItem);
                ProgressChanged?.Invoke(this, downloadItem);
            }

            downloadItem.Status = DownloadStatus.Completed;
            downloadItem.CompletedAt = DateTime.Now;
            DownloadCompleted?.Invoke(this, downloadItem);

            return savePath;
        }
        catch (Exception ex)
        {
            downloadItem.Status = DownloadStatus.Failed;
            downloadItem.ErrorMessage = ex.Message;
            DownloadFailed?.Invoke(this, (downloadItem, ex.Message));
            throw;
        }
        finally
        {
            await Task.Delay(100); // Allow UI to update
            _activeDownloads.Remove(downloadItem.Id);
        }
    }

    public void PauseDownload(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var item))
        {
            item.Status = DownloadStatus.Paused;
        }
    }

    public void ResumeDownload(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var item))
        {
            item.Status = DownloadStatus.Downloading;
        }
    }

    public void CancelDownload(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var item))
        {
            item.Status = DownloadStatus.Cancelled;
            _cancellationTokenSource?.Cancel();

            // Clean up partially downloaded file
            try
            {
                if (File.Exists(item.SavePath))
                    File.Delete(item.SavePath);
            }
            catch { }
        }
    }

    public List<Program> GetDefaultPrograms()
    {
        return new List<Program>
        {
            // Utilities
            new Program { Name = "7-Zip", Description = "Free file archiver", Category = ProgramCategory.Utilities, Version = "23.01", Publisher = "Igor Pavlov", DownloadUrl = "https://www.7-zip.org/a/7z2301-x64.exe", SizeDescription = "1.5 MB" },
            new Program { Name = "VLC Media Player", Description = "Open source multimedia player", Category = ProgramCategory.Media, Version = "3.0.20", Publisher = "VideoLAN", DownloadUrl = "https://get.videolan.org/vlc/3.0.20/win64/vlc-3.0.20-win64.exe", SizeDescription = "30 MB" },
            new Program { Name = "Notepad++", Description = "Free source code editor", Category = ProgramCategory.Development, Version = "8.5.5", Publisher = "Don Ho", DownloadUrl = "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.5.5/npp.8.5.5.Installer.x64.exe", SizeDescription = "5 MB" },
            
            // Development
            new Program { Name = "Git", Description = "Version control system", Category = ProgramCategory.Development, Version = "2.42.0", Publisher = "The Git Project", DownloadUrl = "https://github.com/git-for-windows/git/releases/download/v2.42.0.windows.1/Git-2.42.0-64-bit.exe", SizeDescription = "45 MB" },
            new Program { Name = "Python", Description = "Programming language", Category = ProgramCategory.Development, Version = "3.11.5", Publisher = "Python Software Foundation", DownloadUrl = "https://www.python.org/ftp/python/3.11.5/python-3.11.5-amd64.exe", SizeDescription = "25 MB" },
            new Program { Name = "Visual Studio Code", Description = "Code editor", Category = ProgramCategory.Development, Version = "1.82.2", Publisher = "Microsoft", DownloadUrl = "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64", SizeDescription = "85 MB" },
            
            // Communication
            new Program { Name = "Discord", Description = "Chat and voice communication", Category = ProgramCategory.Communication, Version = "0.0.312", Publisher = "Discord", DownloadUrl = "https://discordapp.com/api/download?platform=win", SizeDescription = "95 MB" },
            new Program { Name = "Telegram", Description = "Messaging application", Category = ProgramCategory.Communication, Version = "4.10.3", Publisher = "Telegram", DownloadUrl = "https://updates.tdesktop.com/tsetup/tsetup.4.10.3.exe", SizeDescription = "30 MB" },
            
            // Productivity
            new Program { Name = "LibreOffice", Description = "Free office suite", Category = ProgramCategory.Productivity, Version = "7.6.0", Publisher = "The Document Foundation", DownloadUrl = "https://download.documentfoundation.org/libreoffice/stable/7.6.0/win/x86_64/LibreOffice_7.6.0_Win_x86-64.msi", SizeDescription = "250 MB" },
            
            // Browser
            new Program { Name = "Google Chrome", Description = "Web browser", Category = ProgramCategory.Browser, Version = "116.0", Publisher = "Google", DownloadUrl = "https://dl.google.com/chrome/install/ChromeStandaloneSetup64.exe", SizeDescription = "45 MB" },
            new Program { Name = "Firefox", Description = "Open source web browser", Category = ProgramCategory.Browser, Version = "117.0", Publisher = "Mozilla", DownloadUrl = "https://download.mozilla.org/?product=firefox-latest&os=win64&lang=en-US", SizeDescription = "50 MB" },
            
            // Gaming
            new Program { Name = "Steam", Description = "Game distribution platform", Category = ProgramCategory.Gaming, Version = "2.0", Publisher = "Valve", DownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/SteamSetup.exe", SizeDescription = "2 MB" },
            
            // Security
            new Program { Name = "Bitwarden", Description = "Password manager", Category = ProgramCategory.Security, Version = "2023.9.0", Publisher = "Bitwarden", DownloadUrl = "https://vault.bitwarden.com/download/?app=desktop&platform=windows", SizeDescription = "150 MB" },
        };
    }
}

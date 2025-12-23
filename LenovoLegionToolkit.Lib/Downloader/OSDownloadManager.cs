using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LenovoLegionToolkit.Lib.Downloader;

public class OSDownloadManager
{
    private readonly HttpClient _httpClient;

    public OSDownloadManager()
    {
        _httpClient = new HttpClient();
    }

    public List<OSImage> GetWindowsImages(WindowsVersionType version, WindowsEditionType edition)
    {
        return version switch
        {
            WindowsVersionType.Windows10_21H2 => GetWindows10Images(edition),
            WindowsVersionType.Windows11_22H2 => GetWindows11_22H2Images(edition),
            WindowsVersionType.Windows11_23H2 => GetWindows11_23H2Images(edition),
            _ => new List<OSImage>()
        };
    }

    public List<OSImage> GetLinuxImages(LinuxDistro distro)
    {
        return distro switch
        {
            LinuxDistro.Ubuntu => GetUbuntuImages(),
            LinuxDistro.Fedora => GetFedoraImages(),
            LinuxDistro.Debian => GetDebianImages(),
            LinuxDistro.ArchLinux => GetArchLinuxImages(),
            LinuxDistro.Linux_Mint => GetLinuxMintImages(),
            LinuxDistro.Manjaro => GetManjaroImages(),
            _ => new List<OSImage>()
        };
    }

    private List<OSImage> GetWindows10Images(WindowsEditionType edition)
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = $"Windows 10 21H2 {edition}",
                Description = $"Windows 10 version 21H2 - {edition} Edition",
                Version = "21H2",
                SizeBytes = 5_000_000_000, // 5 GB
                DownloadUrl = "https://www.microsoft.com/software-download/windows10",
                ReleaseDate = new DateTime(2023, 01, 01)
            }
        };
    }

    private List<OSImage> GetWindows11_22H2Images(WindowsEditionType edition)
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = $"Windows 11 22H2 {edition}",
                Description = $"Windows 11 version 22H2 - {edition} Edition",
                Version = "22H2",
                SizeBytes = 6_000_000_000, // 6 GB
                DownloadUrl = "https://www.microsoft.com/software-download/windows11",
                ReleaseDate = new DateTime(2023, 09, 26)
            }
        };
    }

    private List<OSImage> GetWindows11_23H2Images(WindowsEditionType edition)
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = $"Windows 11 23H2 {edition}",
                Description = $"Windows 11 version 23H2 - {edition} Edition (Latest)",
                Version = "23H2",
                SizeBytes = 6_500_000_000, // 6.5 GB
                DownloadUrl = "https://www.microsoft.com/software-download/windows11",
                ReleaseDate = new DateTime(2023, 10, 10)
            }
        };
    }

    private List<OSImage> GetUbuntuImages()
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = "Ubuntu 23.10 (Mantic Minotaur)",
                Description = "Latest Ubuntu Desktop - 64-bit",
                Version = "23.10",
                SizeBytes = 3_500_000_000,
                DownloadUrl = "https://releases.ubuntu.com/mantic/ubuntu-23.10-desktop-amd64.iso",
                ReleaseDate = new DateTime(2023, 10, 12)
            },
            new OSImage
            {
                Name = "Ubuntu 22.04 LTS (Jammy Jellyfish)",
                Description = "Long-term support version - 64-bit",
                Version = "22.04 LTS",
                SizeBytes = 3_300_000_000,
                DownloadUrl = "https://releases.ubuntu.com/jammy/ubuntu-22.04.3-desktop-amd64.iso",
                ReleaseDate = new DateTime(2023, 08, 01)
            }
        };
    }

    private List<OSImage> GetFedoraImages()
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = "Fedora 38 Workstation",
                Description = "Latest Fedora with GNOME Desktop",
                Version = "38",
                SizeBytes = 2_100_000_000,
                DownloadUrl = "https://download.fedoraproject.org/pub/fedora/linux/releases/38/Workstation/x86_64/iso/Fedora-Workstation-Live-x86_64-38-1.6.iso",
                ReleaseDate = new DateTime(2023, 04, 18)
            }
        };
    }

    private List<OSImage> GetDebianImages()
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = "Debian 12 (Bookworm)",
                Description = "Stable Debian Release",
                Version = "12",
                SizeBytes = 2_400_000_000,
                DownloadUrl = "https://cdimage.debian.org/debian-cd/current/amd64/iso-dvd/debian-12.1.0-amd64-DVD-1.iso",
                ReleaseDate = new DateTime(2023, 06, 10)
            }
        };
    }

    private List<OSImage> GetArchLinuxImages()
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = "Arch Linux 2023.10",
                Description = "Rolling release Linux distribution",
                Version = "2023.10.01",
                SizeBytes = 905_000_000,
                DownloadUrl = "https://mirror.pkgbuild.com/iso/2023.10.01/archlinux-2023.10.01-x86_64.iso",
                ReleaseDate = new DateTime(2023, 10, 01)
            }
        };
    }

    private List<OSImage> GetLinuxMintImages()
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = "Linux Mint 21.2 Cinnamon",
                Description = "Ubuntu-based with Cinnamon Desktop",
                Version = "21.2",
                SizeBytes = 3_000_000_000,
                DownloadUrl = "https://mirror.akado.uz/linuxmint-dvd/stable/21.2/linuxmint-21.2-cinnamon-64bit.iso",
                ReleaseDate = new DateTime(2023, 06, 20)
            }
        };
    }

    private List<OSImage> GetManjaroImages()
    {
        return new List<OSImage>
        {
            new OSImage
            {
                Name = "Manjaro 23.0.0 KDE",
                Description = "Arch-based with KDE Plasma Desktop",
                Version = "23.0.0",
                SizeBytes = 3_200_000_000,
                DownloadUrl = "https://download.manjaro.org/kde/23.0.0/manjaro-kde-23.0.0-231005-x86_64.iso",
                ReleaseDate = new DateTime(2023, 10, 05)
            }
        };
    }

    public string FormatBytes(long bytes)
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

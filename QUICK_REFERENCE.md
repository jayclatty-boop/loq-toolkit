# Quick Reference - Program & OS Downloader

## Navigation
- **Program Downloader**: Navigation Menu → "Program Downloader" (Icon: Download24)
- **OS Downloader**: Navigation Menu → "OS Downloader" (Icon: CompactDisk24)

## Program Downloader - Available Programs

### Utilities (1)
- 7-Zip 23.01

### Media (1)
- VLC Media Player 3.0.20

### Development (3)
- Notepad++ 8.5.5
- Git 2.42.0
- Python 3.11.5
- Visual Studio Code 1.82.2

### Communication (2)
- Discord 0.0.312
- Telegram 4.10.3

### Productivity (1)
- LibreOffice 7.6.0

### Browser (2)
- Google Chrome 116.0
- Firefox 117.0

### Gaming (1)
- Steam 2.0

### Security (1)
- Bitwarden 2023.9.0

## OS Downloader - Available Downloads

### Windows Versions
```
Windows 10 21H2
├── Home
├── Pro
├── Enterprise
└── Education

Windows 11 22H2
├── Home
├── Pro
├── Enterprise
└── Education

Windows 11 23H2 (Latest)
├── Home
├── Pro
├── Enterprise
└── Education
```

### Linux Distributions
```
Ubuntu
├── 23.10 (Mantic Minotaur)
└── 22.04 LTS (Jammy Jellyfish)

Fedora
└── 38 Workstation

Debian
└── 12 (Bookworm)

Arch Linux
└── 2023.10

Linux Mint
└── 21.2 Cinnamon

Manjaro
└── 23.0.0 KDE
```

## File Locations
- **Program Downloads**: `C:\Users\{YourUsername}\Downloads\LOQToolkit\`
- **OS Downloads**: Your system's default Downloads folder

## Download States
1. **Pending** - Queued for download
2. **Downloading** - Currently downloading
3. **Paused** - Temporarily paused
4. **Completed** - Successfully downloaded
5. **Failed** - Download error
6. **Cancelled** - User cancelled

## Features Quick List

### Program Downloader
- ✅ 10+ program categories
- ✅ Real-time search filtering
- ✅ Program details (version, size, publisher)
- ✅ HTTP download with progress tracking
- ✅ Download queue management
- ✅ Automatic folder creation

### OS Downloader
- ✅ Windows 10 & 11 versions
- ✅ Edition selection (4 editions per version)
- ✅ 6 major Linux distributions
- ✅ Distribution-specific versions
- ✅ Direct links to official sources
- ✅ Size and release date information

## Keyboard Shortcuts
- **Ctrl+Tab**: Next page in navigation
- **Ctrl+Shift+Tab**: Previous page in navigation
- **Ctrl+1-0**: Jump to specific navigation item (depends on position)

## Troubleshooting

### Downloads not appearing
- Check `%USERPROFILE%\Downloads\LOQToolkit\` folder
- Verify download completed successfully
- Check for download errors in notification

### OS download not starting
- Check internet connection
- Click download link again
- Ensure your browser's download folder is accessible

### Program crashes
- Check Windows Event Viewer
- Review application logs
- Report issue with reproduction steps

## File Sizes (Approximate)

### Programs
- 7-Zip: 1.5 MB
- VLC: 30 MB
- Notepad++: 5 MB
- Git: 45 MB
- Python: 25 MB
- VS Code: 85 MB
- Discord: 95 MB
- Telegram: 30 MB
- LibreOffice: 250 MB
- Chrome: 45 MB
- Firefox: 50 MB
- Steam: 2 MB
- Bitwarden: 150 MB

### Operating Systems
- Windows 10/11: 5-6.5 GB
- Ubuntu: 3-3.5 GB
- Fedora: 2.1 GB
- Debian: 2.4 GB
- Arch Linux: 905 MB
- Linux Mint: 3 GB
- Manjaro: 3.2 GB

## Support & Documentation
- Full feature guide: `DOWNLOADER_FEATURES.md`
- Implementation details: `IMPLEMENTATION_REPORT.md`
- Source code: `LenovoLegionToolkit.WPF/Pages/` and `LenovoLegionToolkit.Lib/Downloader/`

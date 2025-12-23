# Program & OS Downloader Features

## Overview
Two new comprehensive download managers have been added to the Lenovo Legion Toolkit to help users easily download software and operating systems.

## Features

### 1. Program Downloader
**Location**: Navigation Menu → "Program Downloader"

**Capabilities**:
- Download popular programs organized by category
- Categories: Utilities, Development, Media, Gaming, Communication, Productivity, Security, System, Browser, Other
- Search functionality to find programs
- Real-time download progress tracking
- Support for batch downloading
- Default programs include: 7-Zip, VLC, Notepad++, Git, Python, Visual Studio Code, Discord, Telegram, LibreOffice, Chrome, Firefox, Steam, Bitwarden, and more

**How to Use**:
1. Click on "Program Downloader" in the navigation menu
2. Browse by category or use the search box
3. Click "Download" on any program card
4. Track download progress in the "Downloads" section
5. Files are saved to `%USERPROFILE%\Downloads\LOQToolkit\`

**Features**:
- Category filtering
- Search by name or description
- Program details (version, size, publisher)
- Download status tracking
- Customizable download folder

### 2. OS Downloader
**Location**: Navigation Menu → "OS Downloader"

**Capabilities**:
- Download Windows operating systems (Windows 10 21H2, Windows 11 22H2, Windows 11 23H2)
- Download Linux distributions (Ubuntu, Fedora, Debian, Arch Linux, Linux Mint, Manjaro)
- Select specific editions and versions
- Direct download links to official sources

**Supported Operating Systems**:

**Windows Versions**:
- Windows 10 21H2 (Home, Pro, Enterprise, Education)
- Windows 11 22H2 (Home, Pro, Enterprise, Education)
- Windows 11 23H2 (Home, Pro, Enterprise, Education) - Latest

**Linux Distributions**:
- Ubuntu 23.10 and 22.04 LTS
- Fedora 38 Workstation
- Debian 12 (Bookworm)
- Arch Linux 2023.10
- Linux Mint 21.2 Cinnamon
- Manjaro 23.0.0 KDE

**How to Use**:
1. Click on "OS Downloader" in the navigation menu
2. Select "Windows" or "Linux" using the radio buttons
3. For Windows: Select version and edition from dropdowns
4. For Linux: Select distribution from dropdown
5. Browse available downloads in the list
6. Click "Download" to open the download in your browser
7. File will be saved to your system's Downloads folder

**Features**:
- Easy Windows/Linux switching
- Version and edition selection
- Size information displayed
- Release date information
- Direct download links to official sources

## Architecture

### Files Added

**WPF Pages**:
- `LenovoLegionToolkit.WPF/Pages/ProgramDownloaderPage.xaml` - UI for program downloader
- `LenovoLegionToolkit.WPF/Pages/ProgramDownloaderPage.xaml.cs` - Code-behind for program downloader
- `LenovoLegionToolkit.WPF/Pages/OSDownloaderPage.xaml` - UI for OS downloader
- `LenovoLegionToolkit.WPF/Pages/OSDownloaderPage.xaml.cs` - Code-behind for OS downloader

**Library Classes**:
- `LenovoLegionToolkit.Lib/Downloader/Program.cs` - Program data model
- `LenovoLegionToolkit.Lib/Downloader/ProgramCategory.cs` - Program categories enum
- `LenovoLegionToolkit.Lib/Downloader/ProgramDownloadManager.cs` - Program download handler
- `LenovoLegionToolkit.Lib/Downloader/OSImage.cs` - OS image data model and enums
- `LenovoLegionToolkit.Lib/Downloader/OSDownloadManager.cs` - OS download handler
- `LenovoLegionToolkit.Lib/Downloader/DownloadItem.cs` - Download progress tracking

**Updated Files**:
- `LenovoLegionToolkit.WPF/Windows/MainWindow.xaml` - Added navigation items for both downloaders

## Technical Details

### ProgramDownloadManager
- Handles HTTP downloads with progress tracking
- Creates downloads folder if it doesn't exist
- Supports pause/resume/cancel operations
- Fires events for progress updates and completion
- Default program list with 13 popular applications

### OSDownloadManager
- Manages Windows ISO downloads with edition selection
- Manages Linux distribution downloads
- Provides direct download URLs from official sources
- Size information in GB
- Release date tracking

### DownloadItem
- Tracks individual download progress
- Calculates percentage completion
- Estimates time remaining
- Logs elapsed time
- Stores error messages if download fails

## Download Status States
- **Pending**: Queued for download
- **Downloading**: Currently downloading
- **Paused**: Temporarily paused
- **Completed**: Successfully downloaded
- **Failed**: Download error occurred
- **Cancelled**: User cancelled the download

## File Locations
Programs download to: `%USERPROFILE%\Downloads\LOQToolkit\`

OS images open in your default browser for download to your Downloads folder.

## Future Enhancements
- Direct OS image download support with progress tracking
- Custom program list management
- Download verification with checksums
- Torrent support for larger files
- Scheduled downloads
- Download speed limiting

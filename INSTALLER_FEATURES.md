# LOQ Toolkit Installer - Enhanced Features & Theming

## Build Information
- **Installer Version**: 1.0.0
- **Build Date**: December 23, 2025
- **Output File**: `LOQToolkitSetup_1.0.0.exe` (6.8 MB)
- **Build Tool**: Inno Setup 6.6.1

---

## Enhanced Installer Features

### 1. **Modern Wizard Interface**
- WizardStyle: Modern (Clean, contemporary design)
- Full multilingual support (23 languages)
- Professional installation flow

### 2. **Installation Options**
Users can now choose:
- **Desktop Icon** - Create shortcut on desktop (unchecked by default)
- **Quick Launch** - Add to Quick Launch bar (unchecked by default)
- **System Integration** - Associate with system and register app
- **Startup** - Auto-run on Windows startup (unchecked by default, user-configurable)

### 3. **System Integration Features**
- **File Association**: `.loqt` files are associated with LOQ Toolkit
- **Registry Integration**: Proper Windows registry entries for native OS integration
- **Auto-start Option**: Users can optionally have the app start with Windows
- **Context Menu**: App is properly registered in Add/Remove Programs

### 4. **Installation Features**
- **Compressed Setup**: LZMA2 ultra compression (reduces size)
- **Solid Compression**: Optimized extraction performance
- **Version Info**: Proper file versioning (1.0.0)
- **Admin Privileges**: Runs with administrator rights (required for system features)
- **64-bit Only**: Optimized for modern Windows systems

### 5. **Included Components**
- **Main Application**: LOQ Toolkit with all features
  - Gaming Performance Analyzer
  - Gaming Mode (with process optimization)
  - Memory Optimizer
  - Battery Diagnostics
  - Fan Control
  - Storage Health Monitor
  - System Cleanup
  - GPU Monitoring & Overclocking
  - Stability Testing
  
- **Spectrum Tester**: GPU spectrum testing utility
- **CLI Tool**: Command-line interface (loqt.exe)
- **LICENSE**: Full license file

### 6. **Dependencies**
- **.NET 6 Desktop Runtime**: Auto-installed if not present
- Supports 23 languages with automatic fallback for partial translations

### 7. **Installation Paths**
- **Application**: `C:\Users\[Username]\AppData\Local\LOQToolkit\`
- **Settings**: Stored in user AppData (preserved on uninstall with option)
- **Start Menu**: `Start Menu > Programs > LOQ Toolkit`
- **Desktop** (optional): Desktop shortcut available
- **Quick Launch** (optional): Quick access bar shortcut

### 8. **Uninstallation Features**
- Clean removal of all application files
- Automatic cleanup of user settings (if selected)
- Registry cleanup for file associations
- Automatic removal of scheduled tasks
- Preserves user preferences if desired

### 9. **Version Management**
- Version 1.0.0 included in installer filename for clarity
- Version information embedded in setup executable
- Company information: Bartosz Cichecki
- Product name: LOQ Toolkit

### 10. **Advanced Configuration Options**
```
Installation Tasks:
├── Desktop Icon (optional)
├── Quick Launch (optional)
├── System Integration (recommended)
│   └── File association & registry
└── Auto-start (optional)
    └── Windows startup integration
```

---

## Technical Specifications

### Setup Configuration
- **Architecture**: 64-bit Windows only
- **Privileges**: Administrator required
- **Compression**: LZMA2 Ultra (maximum compression)
- **Output Directory**: `build_installer/`
- **Filename Format**: `LOQToolkitSetup_{Version}.exe`

### Supported Languages
English, Portuguese (Brazil), Bulgarian, Czech, Dutch, French, German, Hungarian, Italian, Japanese, Polish, Portuguese, Russian, Slovak, Spanish, Turkish, Ukrainian, Arabic, Latvian, Chinese (Simplified), Chinese (Traditional), Greek, Romanian, Vietnamese

### Registry Integration
```
HKLM\Software\Classes\.loqt → LOQToolkit file association
HKLM\Software\Classes\LOQToolkit → File type registration
HKCU\Software\Microsoft\Windows\CurrentVersion\Run → Startup entry (optional)
```

---

## Installation Instructions for Users

1. **Run the Installer**
   - Right-click `LOQToolkitSetup_1.0.0.exe` and select "Run as administrator"
   - Or simply double-click (will prompt for admin)

2. **Choose Language**
   - Select your preferred language (23 options available)

3. **Review License**
   - Accept the license agreement

4. **.NET Runtime Check**
   - Installer automatically installs .NET 6 if needed

5. **Choose Installation Options**
   - Desktop Icon: Whether to create desktop shortcut
   - Quick Launch: Whether to add to Quick Launch
   - System Integration: Register app with Windows
   - Auto-start: Whether to run on startup

6. **Install**
   - Click Install to complete
   - Can launch application immediately after

7. **Uninstall**
   - Go to Control Panel > Programs > Programs and Features
   - Find "LOQ Toolkit 1.0.0"
   - Click Uninstall

---

## Build & Distribution

### Building the Installer
```bash
# Build installer with version number
.\make.bat 1.0.0
```

This command:
1. Compiles all projects in Release mode
2. Publishes binaries to `build/` directory
3. Runs Inno Setup to create the installer
4. Outputs `LOQToolkitSetup_1.0.0.exe` to `build_installer/`

### Distribution
- Upload `LOQToolkitSetup_1.0.0.exe` to GitHub Releases
- File size: ~6.8 MB (compressed with LZMA2)
- Installation time: ~30 seconds on SSD, ~1-2 minutes on HDD

---

## Features Added in This Release

### Gaming Performance Analyzer
- Real-time game detection (Steam, Epic, Valorant, CS2, Dota 2, etc.)
- Thermal headroom prediction
- Power headroom analysis
- Sustainable FPS predictions
- Performance profile recommendations
- God Mode preset suggestions

### Gaming Mode Enhancements
- Background process optimization
- RAM clearing on activation
- Power mode switching
- Visual status indicators

### System Monitoring
- Battery health diagnostics
- Storage S.M.A.R.T. monitoring
- Memory utilization tracking
- GPU thermal management
- Real-time performance metrics

---

## Support & Updates

- **GitHub**: https://github.com/BartoszCichecki/LenovoLegionToolkit
- **Issues**: Report bugs on GitHub
- **License**: See LICENSE file in installation directory

---

**Created**: December 23, 2025  
**Version**: 1.0.0  
**Status**: Production Ready ✓

#include "InnoDependencies\install_dotnet.iss"

#define MyAppName "LOQ Toolkit"
#define MyAppNameCompact "LOQToolkit"
#define MyAppPublisher "Bartosz Cichecki"
#define MyAppURL "https://github.com/BartoszCichecki/LenovoLegionToolkit"
#define MyAppExeName "LOQ Toolkit.exe"

#ifndef MyAppVersion
  #define MyAppVersion "0.0.1"
#endif

[Setup]
UsedUserAreasWarning=false
AppId={{8D2F4B7E-5A1C-4E9D-B6F3-9C8E7D4A2B1F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userpf}\{#MyAppNameCompact}
DisableProgramGroupPage=no
LicenseFile=LICENSE
PrivilegesRequired=admin
OutputBaseFilename=LOQToolkitSetup_{#MyAppVersion}
Compression=lzma2/ultra64  
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=build_installer
ArchitecturesInstallIn64BitMode=x64
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Code]
function InitializeSetup: Boolean;
begin
  InstallDotNet6DesktopRuntime;
  Result := True;
end;

[Languages]
Name: "en";      MessagesFile: "compiler:Default.isl"
Name: "ptbr";    MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "bg";      MessagesFile: "compiler:Languages\Bulgarian.isl" 
Name: "cs";      MessagesFile: "compiler:Languages\Czech.isl" 
Name: "nlnl";    MessagesFile: "compiler:Languages\Dutch.isl"
Name: "fr";      MessagesFile: "compiler:Languages\French.isl"
Name: "de";      MessagesFile: "compiler:Languages\German.isl"
Name: "hu";      MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "it";      MessagesFile: "compiler:Languages\Italian.isl"
Name: "ja";      MessagesFile: "compiler:Languages\Japanese.isl"
Name: "pl";      MessagesFile: "compiler:Languages\Polish.isl"
Name: "pt";      MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "ru";      MessagesFile: "compiler:Languages\Russian.isl"
Name: "sk";      MessagesFile: "compiler:Languages\Slovak.isl"
Name: "es";      MessagesFile: "compiler:Languages\Spanish.isl"
Name: "tr";      MessagesFile: "compiler:Languages\Turkish.isl"
Name: "ukr";     MessagesFile: "compiler:Languages\Ukrainian.isl"
Name: "ar";      MessagesFile: "InnoDependencies\Arabic.isl"
Name: "lv";      MessagesFile: "InnoDependencies\Latvian.isl"
Name: "zhhans";  MessagesFile: "InnoDependencies\ChineseSimplified.isl"
Name: "zhhant";  MessagesFile: "InnoDependencies\ChineseTraditional.isl"
Name: "el";      MessagesFile: "InnoDependencies\Greek.isl"
Name: "ro";      MessagesFile: "InnoDependencies\Romanian.isl"
Name: "vi";      MessagesFile: "InnoDependencies\Vietnamese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunch"; Description: "Create a &Quick Launch shortcut"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "associate"; Description: "&Associate with system (register app)"; GroupDescription: "Integration"
Name: "autostart"; Description: "&Run on startup (optional)"; GroupDescription: "Integration"; Flags: unchecked

[Files]
Source: "build\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Lenovo Legion Toolkit - Gaming & System Optimization"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; WorkingDir: "{app}"
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunch; WorkingDir: "{app}"

[InstallDelete]
Type: filesandordirs; Name: "{app}"

[Registry]
Root: "HKLM"; Subkey: "Software\Classes\.loqt"; ValueType: string; ValueName: ""; ValueData: "{#MyAppNameCompact}"; Tasks: associate; Flags: createvalueifdoesntexist
Root: "HKLM"; Subkey: "Software\Classes\{#MyAppNameCompact}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName} File"; Tasks: associate; Flags: createvalueifdoesntexist
Root: "HKLM"; Subkey: "Software\Classes\{#MyAppNameCompact}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: "HKLM"; Subkey: "Software\Classes\{#MyAppNameCompact}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate
Root: "HKCU"; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: "{app}\{#MyAppExeName}"; Tasks: autostart; Flags: createvalueifdoesntexist

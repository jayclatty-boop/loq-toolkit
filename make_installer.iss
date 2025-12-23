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
DisableProgramGroupPage=yes
LicenseFile=LICENSE
PrivilegesRequired=admin
OutputBaseFilename=LOQToolkitSetup
Compression=lzma2/ultra64  
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=build_installer
ArchitecturesInstallIn64BitMode=x64

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

[Files]
Source: "build\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[InstallDelete]
Type: filesandordirs; Name: "{app}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: runascurrentuser nowait postinstall

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\{#MyAppNameCompact}"

[UninstallRun]
RunOnceId: "DelAutorun"; Filename: "schtasks"; Parameters: "/Delete /TN ""LOQToolkit_Autorun_8d2f4b7e-5a1c-4e9d-b6f3-9c8e7d4a2b1f"" /F"; Flags: runhidden 

#define MyAppName "Server App Desktop"
#define MyAppVersion "1.0 Preview 2"
#define MyAppPublisher "Prof Minecraft Dev"
#define MyAppURL "https://github.com/ProfMinecarftDev/ServerAppDesktop"
#define MyAppExeName "ServerAppDesktop.exe"

[Setup]
AppId={{24845C2B-1690-46C7-928C-9B49DB2076CA}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}\Programs\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
PrivilegesRequired=lowest
OutputDir=.\Output
OutputBaseFilename=ServerAppDesktop-User-Setup-1.0-Pre2
SetupIconFile=Assets\AppIcon.ico
SolidCompression=yes
UninstallDisplayName=Server App Desktop (Preview 2)
WizardStyle=modern
WizardSmallImageFile= "Assets\AppMini.bmp"
WizardImageFile= "Assets\AppBanner.bmp"

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\src\bin\release\net9.0-windows10.0.19041.0\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\bin\release\net9.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
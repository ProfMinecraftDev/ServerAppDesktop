#define MyAppName "Server App Desktop (Preview)"
#define MyAppVersion "1.0.0.3"
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
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=.\Output
OutputBaseFilename=ServerAppDesktop-Setup-{#MyAppVersion}-Preview
SetupIconFile=Assets\AppIcon.ico
SolidCompression=yes
UninstallDisplayName=Server App Desktop (Preview)
WizardSmallImageFile= "Assets\WizardSmall.bmp"
WizardImageFile= "Assets\WizardBanner.bmp"
WizardStyle=modern
DisableWelcomePage=False
ShowTasksTreeLines=True
TimeStampsInUTC=True
PrivilegesRequiredOverridesAllowed=dialog
PrivilegesRequired=admin
ChangesEnvironment=True
UsePreviousPrivileges=False
MinVersion=0,10.0.19041

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\src\ServerAppDesktop\bin\x64\Release\net10.0-windows10.0.26100.0\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\ServerAppDesktop\bin\x64\Release\net10.0-windows10.0.26100.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


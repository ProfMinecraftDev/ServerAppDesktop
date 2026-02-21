#define AppName "Server App Desktop (Preview)"
#define AppVersion "1.0.0.3"
#define Publisher "Prof Minecraft Dev"
#define RepoURL "https://github.com/ProfMinecarftDev/ServerAppDesktop"
#define AppMainExe "ServerAppDesktop.exe"
#define VersionTag "1.0.0.3-preview"

[Setup]
AppId={{24845C2B-1690-46C7-928C-9B49DB2076CA}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
AppPublisherURL={#RepoURL}
AppSupportURL={#RepoURL}
AppUpdatesURL={#RepoURL}
DefaultDirName={autopf}\{#AppName}
UninstallDisplayIcon={app}\{#AppMainExe}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64os
DefaultGroupName={#AppName}
AllowNoIcons=yes
OutputDir=..\export\installer
OutputBaseFilename=ServerAppDesktop-Setup-{#VersionTag}
SetupIconFile=Assets\AppIcon.ico
SolidCompression=yes
UninstallDisplayName=Server App Desktop (Preview)
WizardSmallImageFile="Assets\WizardSmall.bmp"
WizardImageFile="Assets\WizardBanner.bmp"
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
Source: "..\export\publish\{#AppMainExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\export\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppMainExe}"
Name: "{group}\{cm:ProgramOnTheWeb,{#AppName}}"; Filename: "{#RepoURL}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppMainExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppMainExe}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


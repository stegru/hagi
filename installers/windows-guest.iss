#define AppName "Host and Guest Integration (Guest client)"
#define AppVersion "0.1"
#define AppPublisher "Steve Grundell"
#define AppURL "https://github.com/stegru/hagi"
#define AppExeName "hagi-guest.exe"

[Setup]
AppId={{1C12D46B-DEBB-4472-BE9B-568523206A61}
AppName={#AppName}
AppVersion={#AppVersion}

AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}

DefaultDirName={pf}\{#AppName}
DisableDirPage=yes
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
ShowLanguageDialog=no

OutputDir=bin
OutputBaseFilename=hagi-guest-setup

SolidCompression=yes
ChangesAssociations=True

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\src\Hagi.GuestClient\publish\windows\hagi-guest.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\Hagi.GuestClient\publish\windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
Filename: "{app}\hagi-guest.exe"; Parameters: "install"; Flags: runasoriginaluser; StatusMsg: "Configuring client"
Filename: "{app}\hagi-guest.exe"; Parameters: "join"; Flags: runasoriginaluser; StatusMsg: "Joining host"

[UninstallRun]
Filename: "{app}\hagi-guest.exe"; Parameters: "uninstall"

[Registry]
Root: "HKCU"; Subkey: "SOFTWARE\hagi-guest"; ValueType: string; ValueName: "InstallDir"; ValueData: "{app}"; Flags: deletekey

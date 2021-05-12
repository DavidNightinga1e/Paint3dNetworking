#define MyAppName "Paint3dNetworking"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "David Nightingale"
#define MyAppExeName "Paint3dNetworking.exe"
#define Path "D:\Projects\Paint3dNetworking\Builds\win"
#define OutputDir "D:\Projects\Paint3dNetworking\Builds\win"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{3B306403-51D8-47DC-9030-20EC7941DCAB}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={userappdata}\paint3dNetworking
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
PrivilegesRequired=lowest
OutputDir={#OutputDir}
OutputBaseFilename=paint3dNetworking_{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#Path}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Path}\*"; Excludes: "*_BackUpThisFolder_ButDontShipItWithYourGame\*,*_BackUpThisFolder_ButDontShipItWithYourGame"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: postinstall skipifsilent nowait

[Code]
{ ///////////////////////////////////////////////////////////////////// }
function GetUninstallString(): String;
var
sUnInstPath: String;
sUnInstallString: String;
begin
sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
sUnInstallString := '';
if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
Result := sUnInstallString;
end;
{ ///////////////////////////////////////////////////////////////////// }
function IsUpgrade(): Boolean;
begin
Result := (GetUninstallString() <> '');
end;
{ ///////////////////////////////////////////////////////////////////// }
function UnInstallOldVersion(): Integer;
var
sUnInstallString: String;
iResultCode: Integer;
begin
{ Return Values: }
{ 1 - uninstall string is empty }
{ 2 - error executing the UnInstallString }
{ 3 - successfully executed the UnInstallString }
{ default return value }
Result := 0;
{ get the uninstall string of the old app }
sUnInstallString := GetUninstallString();
if sUnInstallString <> '' then begin
sUnInstallString := RemoveQuotes(sUnInstallString);
if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
Result := 3
else
Result := 2;
end else
Result := 1;
end;
{ ///////////////////////////////////////////////////////////////////// }
procedure CurStepChanged(CurStep: TSetupStep);
begin
if (CurStep=ssInstall) then
begin
if (IsUpgrade()) then
begin
UnInstallOldVersion();
end;
end;
end;
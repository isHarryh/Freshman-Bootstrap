; *** Inno Setup Script for FreshmanBootstrap ***
; This script is based on Inno Setup 6, a free installer for Windows programs.
; Documentation: https://jrsoftware.org/ishelp.php
; Download Inno Setup: https://jrsoftware.org/isdl.php

#define MyAppName "FreshmanBootstrap"
#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif
#define MyAppPublisher "Harry Huang"
#define MyAppURL "https://github.com/isHarryh/Freshman-Bootstrap"

[Setup]
; WARN: The value of AppId uniquely identifies this app. Do not use the same AppId value in installers for other apps.
; (To generate a new GUID, click Tools | Generate GUID inside the Inno Setup IDE.)
AppCopyright        =Copyright (C) 2026 {#MyAppPublisher}
AppId               ={{9C1C6B1E-6A2E-4F4B-9A3E-2A5B7C4D8E10}
AppName             ={#MyAppName}
AppVersion          ={#MyAppVersion}
AppVerName          ="{#MyAppName} {#MyAppVersion}"
AppPublisher        ={#MyAppPublisher}
AppPublisherURL     ={#MyAppURL}
AppSupportURL       ={#MyAppURL}

AllowNoIcons        =yes
Compression         =lzma2/normal
DefaultDirName      ="{autopf}\{#MyAppName}"
DefaultGroupName    ={#MyAppName}
PrivilegesRequired  =admin
OutputBaseFilename  ={#MyAppName}-v{#MyAppVersion}-Setup
OutputDir           =..\artifacts
SetupIconFile       =..\assets\icon.ico
SolidCompression    =yes
UninstallDisplayIcon={app}\{#MyAppName}.exe
WizardStyle         =modern

ArchitecturesAllowed            =x64compatible
ArchitecturesInstallIn64BitMode =x64compatible
MinVersion                      =10.0.17763
SetupLogging                    =yes

[Languages]
Name: "chinese_simplified";  MessagesFile: "ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\payload\*"; DestDir: "{app}\payload"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppName}.exe"; WorkingDir: "{app}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppName}.exe"; Tasks: desktopicon; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#MyAppName}.exe"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall

[UninstallDelete]
Type: filesandordirs; Name: "{commonappdata}\FreshmanBootstrap"

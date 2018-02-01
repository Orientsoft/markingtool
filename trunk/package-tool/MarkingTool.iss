; �ű��� Inno Setup �ű��� ���ɣ�
; �йش��� Inno Setup �ű��ļ�����ϸ��������İ����ĵ���

#define MyAppName "��һɨ�蹤��"
#define MyAppVersion "0.6.32"
#define MyAppPublisher "�Ĵ���һ�Ƽ����޹�˾"
#define MyAppURL "http://www.dayeasy.net"
#define MyAppExeName "DayEasy.MarkingTool.exe"
#define MyFolder "D:\works\markingtool\trunk\package-tool"

[Setup]
; ע: AppId��ֵΪ������ʶ��Ӧ�ó���
; ��ҪΪ������װ����ʹ����ͬ��AppIdֵ��
; (�����µ�GUID����� ����|��IDE������GUID��)
AppId={{580D6EF5-0B8B-4447-B6E4-0E0C04A37F98}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir={#MyFolder}\packages\
OutputBaseFilename=MarkingTool_{#MyAppVersion}
SetupIconFile={#MyFolder}\src\icon.ico
Compression=lzma
SolidCompression=yes
;WindowVisible=yes

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyFolder}\src\DayEasy.MarkingTool.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\Deyi.AutoUpdater.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\AxMTKTWOCXLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\DayEasy.MarkingTool.BLL.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\DayEasy.MarkingTool.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\DayEasy.MarkingTool.exe.manifest"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\LiteDB.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\DayEasy.Models.Open.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\ICSharpCode.SharpZipLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\log4net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\MTKTWOCXLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\org.in2bits.MyXls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\AForge.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\AForge.Imaging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\AForge.Math.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\zxing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\zxing.presentation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\Microsoft.Expression.Controls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\Microsoft.Expression.Drawing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\regocx.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyFolder}\src\config\*"; DestDir: "{app}\config"; Flags: ignoreversion recursesubdirs createallsubdirs
; ע��: ��Ҫ���κι���ϵͳ�ļ���ʹ�á�Flags: ignoreversion��                                     
;Source: "D:\Program Files (x86)\ISTool\isxdl.dll"; Flags: dontcopy
Source: "d:\Program Files (x86)\Inno Setup 5\Addin\IsSkin.dll"; DestDir: {app}; Flags: dontcopy
Source: "d:\Program Files (x86)\Inno Setup 5\isSkins\Vista.cjstyles"; DestDir: {tmp}; Flags: dontcopy 

[code]
//����Ƥ��
procedure LoadSkin(lpszPath: String; lpszIniFileName: String);
external 'LoadSkin@files:isskin.dll stdcall';

procedure UnloadSkin();
external 'UnloadSkin@files:isskin.dll stdcall';

//.Net Framework ���
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation{FEF7DF1D-1669-4EB8-9668-EDFEEC907864}
//    'v4.5'          .NET Framework 4.5
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, release, serviceCount: cardinal;
    check45, success: boolean;
begin
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.5' then begin
        version := 'v4\Full';
        check45 := true;
    end else
        check45 := false;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check45 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;

function InitializeSetup: Boolean;  
var Path:string ;  
    ResultCode: Integer;  
begin
  //��������
  ExtractTemporaryFile('Vista.cjstyles');
  //MsgBox(ExpandConstant('{tmp}\Office2007.cjstyles'), mbInformation, MB_OK);
  LoadSkin(ExpandConstant('{tmp}\Vista.cjstyles'), 'NormalBlack.ini');

  if IsDotNetDetected('v4\Client',0) then  
  begin  
    Result := true;  
  end  
  else begin  
    if MsgBox('ϵͳ��⵽��û�а�װ.Net Framework 4.0���Ƿ��������ز���װ��', mbConfirmation, MB_YESNO) = idYes then  
    begin  
      Path := ExpandConstant('{pf}/Internet Explorer/iexplore.exe');  
      Exec(Path, 'http://file.dayeasy.net/update/dotnetfx40.exe', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);  
      MsgBox('�밲װ��.Net Framework 4.0�����������б���װ������',mbInformation,MB_OK);  
      Result := false;  
    end  
    else  
    begin  
      MsgBox('û�а�װ.Net Framework 4.0�������޷����а�װ���򣬱���װ���򼴽��˳���',mbInformation,MB_OK);  
      Result := false;  
    end;  
  end;  
end;

//ж������
function ShowWindow(hWnd: Integer; uType: Integer): Integer;
external 'ShowWindow@user32.dll stdcall';

procedure DeinitializeSetup();
begin
  ShowWindow(StrToInt(ExpandConstant('{wizardhwnd}')), 0);
  UnloadSkin();
end; 

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon
;ж��
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runascurrentuser


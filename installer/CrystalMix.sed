[Version]
Class=IEXPRESS
SEDVersion=3

[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=0
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=<None>
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles

[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=
TargetName=dist\CrystalMix-Setup-x64.exe
FriendlyName=CrystalMix 1.0.0 Installer
AppLaunched=cmd.exe /c Install.cmd
AdminQuietInstCmd=cmd.exe /c Install.cmd
UserQuietInstCmd=cmd.exe /c Install.cmd
FILE0=CrystalMix.exe
FILE1=Install.cmd
FILE2=Install.ps1
FILE3=Uninstall.cmd
FILE4=Uninstall.ps1
FILE5=InstallSupport.ps1

[SourceFiles]
SourceFiles0=dist\installer-payload\

[SourceFiles0]
%FILE0%=
%FILE1%=
%FILE2%=
%FILE3%=
%FILE4%=
%FILE5%=

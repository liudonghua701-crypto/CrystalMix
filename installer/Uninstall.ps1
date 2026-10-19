$ErrorActionPreference = 'SilentlyContinue'

$answer = (New-Object -ComObject WScript.Shell).Popup(
    'Remove CrystalMix from this computer? / 是否从此电脑卸载 CrystalMix？',
    0,
    'CrystalMix',
    4 + 32
)
if ($answer -ne 6) { exit 0 }

$installDir = $PSScriptRoot
$programsDir = [Environment]::GetFolderPath('Programs')
$desktopDir = [Environment]::GetFolderPath('Desktop')
Remove-Item -LiteralPath (Join-Path $programsDir 'CrystalMix.lnk') -Force
Remove-Item -LiteralPath (Join-Path $desktopDir 'CrystalMix.lnk') -Force
Remove-Item -LiteralPath 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CrystalMix' -Recurse -Force
Get-Process -Name 'CrystalMix' -ErrorAction SilentlyContinue | Stop-Process -Force

$cleanupPath = Join-Path $env:TEMP ('CrystalMix-cleanup-' + [Guid]::NewGuid().ToString('N') + '.cmd')
$escapedDir = $installDir.Replace('%', '%%')
$cleanup = "@echo off`r`ntimeout /t 2 /nobreak >nul`r`nrmdir /s /q `"$escapedDir`"`r`ndel /q `"%~f0`"`r`n"
[IO.File]::WriteAllText($cleanupPath, $cleanup, [Text.Encoding]::ASCII)
Start-Process -FilePath 'cmd.exe' -ArgumentList '/c', ('"' + $cleanupPath + '"') -WindowStyle Hidden

(New-Object -ComObject WScript.Shell).Popup(
    'CrystalMix has been removed. / CrystalMix 已卸载。',
    4,
    'CrystalMix',
    64
) | Out-Null

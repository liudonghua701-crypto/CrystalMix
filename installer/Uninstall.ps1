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

# 仅移除本程序的文件，不递归删除目录，保留用户自行放入的其他文件。
$expected = [IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'Programs\CrystalMix'))
if ([IO.Path]::GetFullPath($installDir) -ne $expected) { throw 'Unexpected installation directory.' }
foreach ($name in @('CrystalMix.exe', 'Uninstall.cmd', 'Uninstall.ps1')) {
    Remove-Item -LiteralPath (Join-Path $installDir $name) -Force
}
Remove-Item -LiteralPath $installDir -ErrorAction SilentlyContinue
(New-Object -ComObject WScript.Shell).Popup(
    'CrystalMix has been removed. / CrystalMix 已卸载。',
    4,
    'CrystalMix',
    64
) | Out-Null

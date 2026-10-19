param([switch]$Unattended)
$ErrorActionPreference = 'Stop'
try {

if (-not $Unattended) {
$answer = (New-Object -ComObject WScript.Shell).Popup(
    'Remove CrystalMix from this computer? / 是否从此电脑卸载 CrystalMix？',
    0,
    'CrystalMix',
    4 + 32
)
if ($answer -ne 6) { exit 0 }
}

$installDir = [IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\')
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CrystalMix'
$registration = Get-ItemProperty -LiteralPath $uninstallKey -ErrorAction SilentlyContinue
if ($installDir.Length -le 3 -or -not $registration.InstallLocation -or
    $installDir -ne [IO.Path]::GetFullPath($registration.InstallLocation).TrimEnd('\')) {
    throw '卸载目录与安装记录不符，未删除任何内容。 / Installation path mismatch; no files removed.'
}
$targetExe = Join-Path $installDir 'CrystalMix.exe'
# 只停止此安装目录中的进程，避免影响其他位置的程序。
Get-Process -Name CrystalMix -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $targetExe } | ForEach-Object {
    Stop-Process -Id $_.Id -Force -ErrorAction Stop
    if (-not $_.WaitForExit(10000)) { throw 'CrystalMix is still running.' }
}
# 不递归删除安装目录；用户放入的其他文件和子目录保留。
foreach ($name in @('CrystalMix.exe', 'Uninstall.cmd')) {
    $file = Join-Path $installDir $name
    if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file -Force -ErrorAction Stop }
}
$shell = New-Object -ComObject WScript.Shell
foreach ($folder in @([Environment]::GetFolderPath('Programs'), [Environment]::GetFolderPath('Desktop'))) {
    $link = Join-Path $folder 'CrystalMix.lnk'
    if ((Test-Path -LiteralPath $link) -and $shell.CreateShortcut($link).TargetPath -eq $targetExe) {
        Remove-Item -LiteralPath $link -Force -ErrorAction Stop
    }
}
# 先完成文件删除，再清除卸载入口；失败时保留入口以便重试。
Remove-Item -LiteralPath (Join-Path $installDir 'Uninstall.ps1') -Force -ErrorAction Stop
if (-not (Get-ChildItem -LiteralPath $installDir -Force | Select-Object -First 1)) {
    Remove-Item -LiteralPath $installDir -Force -ErrorAction Stop
}
Remove-Item -LiteralPath $uninstallKey -Force -ErrorAction Stop
if (-not $Unattended) {
(New-Object -ComObject WScript.Shell).Popup(
    'CrystalMix has been removed. / CrystalMix 已卸载。',
    4,
    'CrystalMix',
    64
) | Out-Null
}
} catch {
    if (-not $Unattended) { (New-Object -ComObject WScript.Shell).Popup($_.Exception.Message, 0, 'CrystalMix 卸载失败 / Uninstall failed', 16) | Out-Null }
    Write-Error $_ -ErrorAction Continue
    exit 1
}

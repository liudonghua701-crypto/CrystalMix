param([string]$InstallDir, [switch]$Unattended, [switch]$NoLaunch)
$ErrorActionPreference = 'Stop'
try {
. (Join-Path $PSScriptRoot 'InstallSupport.ps1')
# IExpress 只使用 ASCII；双语弹窗交由读取 UTF-8 BOM 的 PowerShell 显示。
if (-not $Unattended) {
$answer = (New-Object -ComObject WScript.Shell).Popup(
    '是否安装 CrystalMix？ / Install CrystalMix?', 0, 'CrystalMix', 4 + 32)
if ($answer -ne 6) { exit 0 }
}

$appName = 'CrystalMix'
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CrystalMix'
$previous = Get-ItemProperty -LiteralPath $uninstallKey -ErrorAction SilentlyContinue
if (-not $InstallDir) {
    $InstallDir = if ($previous.InstallLocation) { $previous.InstallLocation } else { Join-Path $env:LOCALAPPDATA 'Programs\CrystalMix' }
    if (-not $Unattended) { $InstallDir = Select-CrystalMixFolder $InstallDir; if (-not $InstallDir) { exit 0 } }
}
$installDir = Resolve-CrystalMixPath $InstallDir $PSScriptRoot
if ($previous.InstallLocation -and (Test-Path -LiteralPath (Join-Path $previous.InstallLocation 'CrystalMix.exe')) -and
    $installDir -ne [IO.Path]::GetFullPath($previous.InstallLocation).TrimEnd('\')) {
    throw ('旧版本位于 ' + $previous.InstallLocation + '，更换目录前请先卸载。 / Uninstall the previous location before moving.')
}
$programsDir = [Environment]::GetFolderPath('Programs')
$desktopDir = [Environment]::GetFolderPath('Desktop')
$sourceExe = Join-Path $PSScriptRoot 'CrystalMix.exe'

if (-not (Test-Path -LiteralPath $sourceExe)) {
    throw 'CrystalMix.exe was not found in the installer payload.'
}

New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Copy-Item -LiteralPath $sourceExe -Destination (Join-Path $installDir 'CrystalMix.exe') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Uninstall.ps1') -Destination (Join-Path $installDir 'Uninstall.ps1') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Uninstall.cmd') -Destination (Join-Path $installDir 'Uninstall.cmd') -Force

$shell = New-Object -ComObject WScript.Shell
$targetExe = Join-Path $installDir 'CrystalMix.exe'
foreach ($shortcutPath in @(
    (Join-Path $programsDir 'CrystalMix.lnk'),
    (Join-Path $desktopDir 'CrystalMix.lnk')
)) {
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $targetExe
    $shortcut.WorkingDirectory = $installDir
    $shortcut.Description = 'CrystalMix Crystal Growth Ratio Calculator'
    $shortcut.IconLocation = "$targetExe,0"
    $shortcut.Save()
}

$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CrystalMix'
New-Item -Path $uninstallKey -Force | Out-Null
$values = @{
    DisplayName = 'CrystalMix Crystal Growth Ratio Calculator'
    DisplayVersion = '1.1.0'
    Publisher = 'CrystalMix'
    InstallLocation = $installDir
    DisplayIcon = $targetExe
    UninstallString = '"' + (Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe') + '" -NoProfile -ExecutionPolicy Bypass -File "' + (Join-Path $installDir 'Uninstall.ps1') + '"'
    NoModify = 1
    NoRepair = 1
    EstimatedSize = [int]((Get-Item -LiteralPath $targetExe).Length / 1KB)
}
foreach ($entry in $values.GetEnumerator()) {
    New-ItemProperty -Path $uninstallKey -Name $entry.Key -Value $entry.Value -Force | Out-Null
}

if (-not $NoLaunch) { Start-Process -FilePath $targetExe }
if (-not $Unattended) { (New-Object -ComObject WScript.Shell).Popup('CrystalMix 安装完成。 / Installation complete.', 0, 'CrystalMix', 64) | Out-Null }
} catch {
    if (-not $Unattended) { (New-Object -ComObject WScript.Shell).Popup($_.Exception.Message, 0, 'CrystalMix 安装失败 / Installation failed', 16) | Out-Null }
    Write-Error $_ -ErrorAction Continue
    exit 1
}

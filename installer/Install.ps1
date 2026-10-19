$ErrorActionPreference = 'Stop'
# IExpress 只使用 ASCII；双语弹窗交由读取 UTF-8 BOM 的 PowerShell 显示。
$answer = (New-Object -ComObject WScript.Shell).Popup(
    '是否安装 CrystalMix？ / Install CrystalMix?', 0, 'CrystalMix', 4 + 32)
if ($answer -ne 6) { exit 0 }

$appName = 'CrystalMix'
$installDir = Join-Path $env:LOCALAPPDATA 'Programs\CrystalMix'
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
    DisplayVersion = '1.0.0'
    Publisher = 'CrystalMix'
    InstallLocation = $installDir
    DisplayIcon = $targetExe
    UninstallString = '"' + (Join-Path $installDir 'Uninstall.cmd') + '"'
    NoModify = 1
    NoRepair = 1
    EstimatedSize = [int]((Get-Item -LiteralPath $targetExe).Length / 1KB)
}
foreach ($entry in $values.GetEnumerator()) {
    New-ItemProperty -Path $uninstallKey -Name $entry.Key -Value $entry.Value -Force | Out-Null
}

Start-Process -FilePath $targetExe
(New-Object -ComObject WScript.Shell).Popup('CrystalMix 安装完成。 / Installation complete.', 0, 'CrystalMix', 64) | Out-Null

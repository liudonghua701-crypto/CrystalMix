param([string]$TestBase)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
. (Join-Path $root 'installer\InstallSupport.ps1')
if (-not $TestBase) { $TestBase = Join-Path $root '.tools' }
$testRoot = Join-Path $TestBase ('installer-test-' + [Guid]::NewGuid().ToString('N'))
Write-Output ('Test workspace: ' + $testRoot)
$payload = Join-Path $testRoot 'payload'
New-Item -ItemType Directory -Path $payload -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'dist\publish\CrystalMix.exe') -Destination $payload
foreach ($name in @('Install.ps1','InstallSupport.ps1','Uninstall.ps1','Uninstall.cmd')) {
    Copy-Item -LiteralPath (Join-Path $root ('installer\' + $name)) -Destination $payload
}
$key = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CrystalMix'
$regPath = 'HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\CrystalMix'
$hadRegistration = Test-Path -LiteralPath $key
$backup = Join-Path $testRoot 'registration.reg'
if ($hadRegistration) {
    & reg.exe export $regPath $backup /y | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Cannot back up registration.' }
}
$links = @([Environment]::GetFolderPath('Desktop'),[Environment]::GetFolderPath('Programs')) | ForEach-Object { Join-Path $_ 'CrystalMix.lnk' }
$savedLinks = @{}
foreach ($link in $links) { if (Test-Path -LiteralPath $link) { $savedLinks[$link] = [IO.File]::ReadAllBytes($link) } }
function Assert($Condition, $Message) { if (-not $Condition) { throw $Message }; Write-Output ('PASS: ' + $Message) }
function Run-Script($Path, [string[]]$Arguments) {
    $savedPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Path @Arguments 2>&1
        $code = $LASTEXITCODE
        return $code
    } finally { $ErrorActionPreference = $savedPreference }
}
try {
    foreach ($bad in @('relative\CrystalMix','C:\', $payload)) {
        $rejected = $false
        try { $null = Resolve-CrystalMixPath $bad $payload } catch { $rejected = $true }
        Assert $rejected ('Unsafe path rejected: ' + $bad)
    }
    if ($hadRegistration) { Remove-Item -LiteralPath $key -Force }
    $target = Join-Path $testRoot '中文 空格 & 100%\CrystalMix'
    $code = Run-Script (Join-Path $payload 'Install.ps1') @('-InstallDir', $target, '-Unattended', '-NoLaunch')
    Assert ($code -eq 0) 'Custom Unicode path installation succeeds'
    Assert ((Get-ItemProperty -LiteralPath $key).InstallLocation -eq $target) 'Registration uses selected path'
    Assert ((Get-ItemProperty -LiteralPath $key).UninstallString -match 'powershell.exe.*Uninstall.ps1') 'Direct PowerShell uninstall entry'
    $shell = New-Object -ComObject WScript.Shell
    foreach ($link in $links) { Assert ($shell.CreateShortcut($link).TargetPath -eq (Join-Path $target 'CrystalMix.exe')) 'Shortcut targets selected path' }
    $uninstall = Join-Path $target 'Uninstall.ps1'
    Set-ItemProperty -LiteralPath $key -Name InstallLocation -Value $payload
    $code = Run-Script $uninstall @('-Unattended')
    Assert ($code -ne 0 -and (Test-Path -LiteralPath (Join-Path $target 'CrystalMix.exe'))) 'Path mismatch leaves files intact'
    Set-ItemProperty -LiteralPath $key -Name InstallLocation -Value $target
    $locked = [IO.File]::Open((Join-Path $target 'CrystalMix.exe'), 'Open', 'Read', 'Read')
    try { $code = Run-Script $uninstall @('-Unattended') } finally { $locked.Dispose() }
    Assert ($code -ne 0 -and (Test-Path -LiteralPath $key) -and (Test-Path -LiteralPath $uninstall)) 'Locked file preserves retryable uninstall entry'
    $sentinel = Join-Path $target '用户文件.txt'
    [IO.File]::WriteAllText($sentinel, 'keep this file')
    $code = Run-Script $uninstall @('-Unattended')
    Assert ($code -eq 0 -and -not (Test-Path -LiteralPath $key)) 'Uninstall removes registration after success'
    Assert (-not (Test-Path -LiteralPath (Join-Path $target 'CrystalMix.exe'))) 'Uninstall removes application'
    Assert ([IO.File]::ReadAllText($sentinel) -eq 'keep this file') 'Uninstall preserves user files'
    $emptyTarget = Join-Path $testRoot 'EmptyInstall'
    $code = Run-Script (Join-Path $payload 'Install.ps1') @('-InstallDir', $emptyTarget, '-Unattended', '-NoLaunch')
    Assert ($code -eq 0) 'Second clean installation succeeds'
    $code = Run-Script (Join-Path $emptyTarget 'Uninstall.ps1') @('-Unattended')
    Assert ($code -eq 0 -and -not (Test-Path -LiteralPath $emptyTarget)) 'Empty installation directory is removed'
} finally {
    if (Test-Path -LiteralPath $key) { Remove-Item -LiteralPath $key -Force }
    if ($hadRegistration) { & reg.exe import $backup | Out-Null; if ($LASTEXITCODE -ne 0) { throw "Restore registration from $backup" } }
    foreach ($link in $links) {
        if ($savedLinks.ContainsKey($link)) { [IO.File]::WriteAllBytes($link, $savedLinks[$link]) }
        elseif (Test-Path -LiteralPath $link) { Remove-Item -LiteralPath $link -Force }
    }
}
Write-Output 'PASS: original registration and shortcuts restored'

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $projectRoot

$localDotnet = Join-Path $projectRoot '.tools\dotnet\dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $localDotnet) { $localDotnet } else { 'dotnet' }
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.tools\dotnet-home'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.tools\packages'
$env:APPDATA = Join-Path $projectRoot '.tools\appdata'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
New-Item -ItemType Directory -Force -Path $env:DOTNET_CLI_HOME, $env:NUGET_PACKAGES, $env:APPDATA | Out-Null

$publishDir = Join-Path $projectRoot 'dist\publish'
$payloadDir = Join-Path $projectRoot 'dist\installer-payload'
New-Item -ItemType Directory -Force -Path $publishDir, $payloadDir | Out-Null

& $dotnet publish '.\CrystalMix\CrystalMix.csproj' `
    -c Release `
    -r win-x64 `
    --self-contained true `
    --configfile '.\NuGet.Config' `
    -o $publishDir `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

Copy-Item -LiteralPath (Join-Path $publishDir 'CrystalMix.exe') -Destination $payloadDir -Force
foreach ($file in @('Install.cmd', 'Install.ps1', 'Uninstall.cmd', 'Uninstall.ps1')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot "installer\$file") -Destination $payloadDir -Force
}

$iexpress = Join-Path $env:WINDIR 'System32\iexpress.exe'
if (-not (Test-Path -LiteralPath $iexpress)) { throw 'Windows IExpress was not found.' }
& $iexpress /N /Q '.\installer\CrystalMix.sed'
if ($LASTEXITCODE -ne 0) { throw 'IExpress packaging failed.' }

Write-Host ''
Write-Host 'Release artifacts:' -ForegroundColor Green
Get-Item '.\dist\publish\CrystalMix.exe', '.\dist\CrystalMix-Setup-x64.exe' |
    Select-Object FullName, Length, LastWriteTime

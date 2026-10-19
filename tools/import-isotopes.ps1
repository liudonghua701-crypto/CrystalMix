param([string]$SourceDirectory = (Join-Path $PSScriptRoot '..\.tools\isotope-source'))
$ErrorActionPreference = 'Stop'
# 固定序列化运行时，避免不同 PowerShell 版本的数值表示影响快照哈希。
if ($PSVersionTable.PSVersion.Major -ne 5) {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -SourceDirectory $SourceDirectory
    if ($LASTEXITCODE -ne 0) { throw 'Import failed.' }
    return
}
$root = Split-Path -Parent $PSScriptRoot
$manifest = Get-Content (Join-Path $root 'data-sources\manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if (-not (Test-Path $SourceDirectory)) {
    Expand-Archive -LiteralPath (Join-Path $root 'data-sources\isotope-sources.zip') -DestinationPath $SourceDirectory
}
foreach ($source in $manifest) {
    if ((Get-FileHash (Join-Path $SourceDirectory $source.File)).Hash -ne $source.SHA256) {
        throw "Source checksum mismatch: $($source.File)"
    }
}
$ci = [Globalization.CultureInfo]::InvariantCulture
$symbols = [regex]::Matches([IO.File]::ReadAllText((Join-Path $root 'CrystalMix\Chemistry\PeriodicTable.cs')), '\["([A-Z][a-z]?)"\]') | ForEach-Object {$_.Groups[1].Value}
if ($symbols.Count -ne 118) { throw 'Existing element map changed.' }
$records = @{}
$raw = [Net.WebUtility]::HtmlDecode([IO.File]::ReadAllText((Join-Path $SourceDirectory 'nist-isotopes.html')))
$pattern = '(?s)Atomic Number = (\d+)\s+Atomic Symbol = \w+\s+Mass Number = (\d+)\s+Relative Atomic Mass = ([^\r\n]+)'
foreach ($entry in [regex]::Matches($raw, $pattern)) {
    $z = [int]$entry.Groups[1].Value; $a = [int]$entry.Groups[2].Value
    if ($z -lt 1 -or $z -gt 118) { continue }
    $token = $entry.Groups[3].Value.Trim()
    if ($token -notmatch '^([0-9.]+)') { throw "Invalid NIST mass: $token" }
    $mass = [double]::Parse($Matches[1], $ci); $symbol = $symbols[$z - 1]; $key = "[$a$symbol]"
    $records[$key] = [ordered]@{ Species=$key; Symbol=$symbol; AtomicNumber=$z; MassNumber=$a; Mass=$mass; Stable=$false; HalfLifeSeconds=$null; HalfLifeText='unknown'; LifeQualifier=''; Estimated=$token.Contains('#'); MassSource='NIST'; LifeSource='unknown' }
}
if ($records.Count -lt 3000) { throw 'NIST import is incomplete.' }
foreach ($row in (Import-Csv (Join-Path $SourceDirectory 'iaea-ground-states.csv'))) {
    $z = [int]$row.z; if ($z -lt 1 -or $z -gt 118) { continue }
    $a = $z + [int]$row.n; $symbol = $symbols[$z-1]; $key = "[$a$symbol]"
    if (-not $records.ContainsKey($key)) {
        if (-not $row.atomic_mass) { continue }
        $mass = [double]::Parse($row.atomic_mass, $ci) / 1e6
        $records[$key] = [ordered]@{ Species=$key; Symbol=$symbol; AtomicNumber=$z; MassNumber=$a; Mass=$mass; Stable=$false; HalfLifeSeconds=$null; HalfLifeText='unknown'; LifeQualifier=''; Estimated=$row.me_systematics -eq 'Y'; MassSource='IAEA-AME2020'; LifeSource='unknown' }
    }
    $record = $records[$key]
    $record.Stable = $row.half_life -eq 'STABLE'
    $record.HalfLifeSeconds = if ($row.half_life_sec) { [double]::Parse($row.half_life_sec,$ci) } else { $null }
    $record.HalfLifeText = if ($record.Stable) { 'STABLE' } elseif ($row.half_life) { ($row.operator_hl+' '+$row.half_life+' '+$row.unit_hl).Trim() } else { 'unknown' }
    $record.LifeQualifier = $row.operator_hl
    $record.LifeSource = 'IAEA-ENSDF'
}
# 核异构体另行导入，避免把 Ta-180 的 8 小时基态当作天然长寿命 Ta-180m。
$year = 31557600.0
$units = @{ y=$year; ky=1e3*$year; My=1e6*$year; Gy=1e9*$year; Ty=1e12*$year; Py=1e15*$year }
foreach ($line in [IO.File]::ReadLines((Join-Path $SourceDirectory 'nubase2020.txt'))) {
    if ($line -notmatch '^\d{3} \d{4}' -or $line.Length -lt 88) { continue }
    $state = [int]$line.Substring(7,1); if ($state -eq 0) { continue }
    $z = [int]$line.Substring(4,3); if ($z -lt 1 -or $z -gt 118) { continue }
    $life = $line.Substring(69,9).Trim(); $unit = $line.Substring(78,2).Trim()
    $seconds = $null; $qualifier = ''
    if ($life -eq 'stbl' -and $line.Substring(81,7).Trim() -match '^>([0-9.]+)\s*([A-Za-z]+)$') {
        $seconds = [double]::Parse($Matches[1],$ci) * $units[$Matches[2]]; $qualifier = 'GT'
    } elseif ($units.ContainsKey($unit) -and $life -match '^([~<>]?)([0-9.Ee+\-]+)#?$') {
        $qualifier = switch ($Matches[1]) { '>' {'GT'} '<' {'LT'} '~' {'AP'} default {''} }
        $seconds = [double]::Parse($Matches[2],$ci) * $units[$unit]
    }
    if ($null -eq $seconds -or $seconds -lt $year -or $qualifier -eq 'LT') { continue }
    $excess = $line.Substring(18,13).Trim(); if ($excess -notmatch '^([\-0-9.]+)#?$') { throw "Invalid isomer mass: $excess" }
    $a = [int]$line.Substring(0,3); $symbol = $symbols[$z-1]
    $suffix = if ($state -eq 1) { 'm' } else { "m$state" }; $key = "[$a$suffix$symbol]"
    # AME2020 采用 1 u = 931494.10242 keV/c²；质量盈余换算为中性原子质量。
    $mass = $a + [double]::Parse($Matches[1],$ci) / 931494.10242
    $records[$key] = [ordered]@{ Species=$key; Symbol=$symbol; AtomicNumber=$z; MassNumber=$a; Mass=$mass; Stable=$false; HalfLifeSeconds=$seconds; HalfLifeText=($life+' '+$unit+' '+$line.Substring(81,7).Trim()).Trim(); LifeQualifier=$qualifier; Estimated=$excess.Contains('#'); MassSource='NUBASE2020'; LifeSource='NUBASE2020' }
}
$sorted = @($records.Values | ForEach-Object { [pscustomobject]$_ } | Sort-Object AtomicNumber,MassNumber,Species)
$json = ConvertTo-Json -InputObject $sorted -Depth 4 -Compress
[IO.File]::WriteAllText((Join-Path $root 'CrystalMix\Data\isotopes.json'), $json, [Text.UTF8Encoding]::new($false))
"Imported $($sorted.Count) isotope records; stable: $(@($sorted | Where-Object Stable).Count); isomers: $(@($sorted | Where-Object {$_.Species -cmatch '\dm'}).Count)"

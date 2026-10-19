$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$package = Join-Path $projectRoot 'dist\CrystalMix-Setup-x64.exe'
$extractDir = Join-Path $projectRoot ('dist\smoke-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $extractDir | Out-Null
# 仅解包到项目内，不更改用户安装状态。
$extract = Start-Process -FilePath $package -ArgumentList '/Q', '/C', ('/T:"' + $extractDir + '"') -WindowStyle Hidden -Wait -PassThru
if ($extract.ExitCode -ne 0) { throw 'Installer extraction failed.' }
foreach ($name in @('CrystalMix.exe', 'Install.ps1', 'Uninstall.ps1', 'Install.cmd', 'Uninstall.cmd')) {
    $actual = Join-Path $extractDir $name
    $expected = Join-Path $projectRoot ('dist\installer-payload\' + $name)
    if ((Get-FileHash -LiteralPath $actual).Hash -ne (Get-FileHash -LiteralPath $expected).Hash) { throw "Payload mismatch: $name" }
}
Write-Output 'PASS: all five extracted files match the release payload.'
Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class CrystalMixWindowProbe {
    public delegate bool Callback(IntPtr window, IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumWindows(Callback callback, IntPtr data);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr window, StringBuilder text, int count);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr window, uint message, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] public static extern bool IsHungAppWindow(IntPtr window);
    public static IntPtr Find(int processId) {
        IntPtr result = IntPtr.Zero;
        EnumWindows((window, data) => {
            uint pid; GetWindowThreadProcessId(window, out pid);
            if (pid == processId) {
                var title = new StringBuilder(512); GetWindowText(window, title, title.Capacity);
                if (title.ToString().StartsWith("CrystalMix")) { result = window; return false; }
            }
            return true;
        }, IntPtr.Zero);
        return result;
    }
}
"@
$app = Start-Process -FilePath (Join-Path $extractDir 'CrystalMix.exe') -WindowStyle Hidden -PassThru
try {
    if (-not $app.WaitForInputIdle(20000)) { throw 'Application startup timed out.' }
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
        Start-Sleep -Milliseconds 250
        $app.Refresh()
        $window = [CrystalMixWindowProbe]::Find($app.Id)
    } while (-not $app.HasExited -and $window -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $deadline)
    if ($app.HasExited -or [CrystalMixWindowProbe]::IsHungAppWindow($window) -or $window -eq [IntPtr]::Zero) { throw ('Application window unavailable. Exited=' + $app.HasExited) }
    Write-Output 'PASS: self-contained x64 application window exists and responds (hidden startup).'
    $null = [CrystalMixWindowProbe]::PostMessage($window, 0x10, [IntPtr]::Zero, [IntPtr]::Zero)
    if (-not $app.WaitForExit(10000)) { throw 'Application did not close normally.' }
    if ($app.ExitCode -ne 0) { throw 'Application returned a nonzero exit code.' }
} finally { if (-not $app.HasExited) { $app.Kill() } }

# 检查真实安装确认框的 Unicode 文本，再选择“否”，不执行安装。
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$prompt = Start-Process powershell.exe -ArgumentList '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ('"' + (Join-Path $extractDir 'Install.ps1') + '"') -WindowStyle Hidden -PassThru
try {
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $prompt.Id)
    $dialog = $null
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    while (-not $dialog -and [DateTime]::UtcNow -lt $deadline) {
        $dialog = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
        if (-not $dialog) { Start-Sleep -Milliseconds 250 }
    }
    if (-not $dialog) { throw 'Installation confirmation dialog missing.' }
    $children = $dialog.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $text = ($children | ForEach-Object { $_.Current.Name }) -join ' '
    if (-not $text.Contains('是否安装 CrystalMix？')) { throw "Incorrect Unicode prompt: $text" }
    $noCondition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, '7')
    $no = $dialog.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $noCondition)
    if (-not $no) { throw 'No button missing.' }
    $null = [CrystalMixWindowProbe]::PostMessage([IntPtr]$dialog.Current.NativeWindowHandle, 0x111, [IntPtr]7, [IntPtr]$no.Current.NativeWindowHandle)
    if (-not $prompt.WaitForExit(10000) -or $prompt.ExitCode -ne 0) { throw 'Cancel installation failed.' }
    Write-Output 'PASS: Chinese installation prompt renders correctly; cancellation exits cleanly.'
} finally { if (-not $prompt.HasExited) { $prompt.Kill() } }
Get-Item -LiteralPath $package | Select-Object FullName, Length
Get-FileHash -LiteralPath $package -Algorithm SHA256

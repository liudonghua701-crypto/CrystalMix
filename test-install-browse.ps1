$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'installer\InstallSupport.ps1')
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class BrowseDialogProbe {
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
}
"@
$script:failure = $null
foreach ($accept in @($true, $false)) {
    $script:acceptBrowse = $accept
    $folderTimer = New-Object Windows.Forms.Timer
    $folderTimer.Interval = 500
    $folderTimer.Add_Tick({
        try {
            $byProcess = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $PID)
            $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, $byProcess)
            $byClass = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ClassNameProperty, '#32770')
            $folder = $null
            foreach ($owner in $windows) {
                $folder = $owner.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $byClass)
                if ($folder) { break }
            }
            if ($folder) {
                $folderTimer.Stop()
                $action = if ($script:acceptBrowse) { 1 } else { 2 }
                $null = [BrowseDialogProbe]::PostMessage([IntPtr]$folder.Current.NativeWindowHandle, 0x111, [IntPtr]$action, [IntPtr]::Zero)
            }
        } catch { $script:failure = $_; $folderTimer.Stop() }
    })
    $mainTimer = New-Object Windows.Forms.Timer
    $mainTimer.Interval = 400
    $mainTimer.Add_Tick({
        $mainTimer.Stop()
        $window = [Windows.Forms.Application]::OpenForms | Where-Object { $_.Text -like 'CrystalMix 安装位置*' } | Select-Object -First 1
        try {
            $buttons = $window.Controls[0].Controls | Where-Object { $_ -is [Windows.Forms.FlowLayoutPanel] }
            $browse = $buttons.Controls | Where-Object { $_.Text -eq '浏览 / Browse' } | Select-Object -First 1
            if (-not $browse) { throw 'Browse button missing' }
            $folderTimer.Start()
            $browse.PerformClick()
            $window.DialogResult = 'OK'
        } catch { $script:failure = $_; $window.DialogResult = 'Cancel' }
    })
    try {
        $mainTimer.Start()
        $result = Select-CrystalMixFolder 'D:\'
        if ($script:failure) { throw $script:failure }
        $expected = if ($accept) { 'D:\CrystalMix' } else { 'D:\' }
        if ($result -ne $expected) { throw "Expected $expected; received $result" }
        "PASS: real Browse dialog accept=$accept returns $result"
    } finally { $mainTimer.Dispose(); $folderTimer.Dispose() }
}

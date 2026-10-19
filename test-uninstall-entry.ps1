$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class UninstallPromptProbe {
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr window, uint message, IntPtr w, IntPtr l);
}
"@
$key='HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CrystalMix'
$entry=Get-ItemProperty -LiteralPath $key
if ($entry.UninstallString -notmatch '^"([^"]+)" (.*)$') { throw 'Invalid uninstall command' }
$process=Start-Process -FilePath $Matches[1] -ArgumentList $Matches[2] -WindowStyle Hidden -PassThru
try {
 $condition=[System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ProcessIdProperty,$process.Id)
 $deadline=[DateTime]::UtcNow.AddSeconds(20); $dialog=$null
 while(-not $dialog -and [DateTime]::UtcNow -lt $deadline){
  $dialog=[System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children,$condition)
  if(-not $dialog){Start-Sleep -Milliseconds 250}
 }
 if(-not $dialog){throw 'Uninstall dialog missing'}
 $children=$dialog.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
 $text=($children | ForEach-Object {$_.Current.Name}) -join ' '
 if(-not $text.Contains('是否从此电脑卸载 CrystalMix？')){throw 'Incorrect uninstall prompt encoding'}
 $null=[UninstallPromptProbe]::PostMessage([IntPtr]$dialog.Current.NativeWindowHandle,0x111,[IntPtr]7,[IntPtr]::Zero)
 if(-not $process.WaitForExit(10000) -or $process.ExitCode -ne 0){throw 'Cancel uninstall failed'}
 if(-not (Test-Path -LiteralPath (Join-Path $entry.InstallLocation 'CrystalMix.exe')) -or -not (Test-Path -LiteralPath $key)){throw 'Cancel changed installation'}
 'PASS: repaired registered uninstall entry opens correct Chinese prompt; cancel preserves installation'
} finally {if(-not $process.HasExited){$process.Kill()}}

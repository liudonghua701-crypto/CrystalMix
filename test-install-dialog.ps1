$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'installer\InstallSupport.ps1')
Add-Type -AssemblyName System.Windows.Forms
$script:chosen = 'E:\自定义 路径\CrystalMix'
$script:failure = $null
$timer = New-Object Windows.Forms.Timer
$timer.Interval = 400
$timer.Add_Tick({
    $timer.Stop()
    try {
        $form = [Windows.Forms.Application]::OpenForms | Where-Object { $_.Text -like 'CrystalMix 安装位置*' } | Select-Object -First 1
        if (-not $form) { throw 'Folder dialog not found' }
        $panel = $form.Controls[0]
        $pathTextBox = $panel.Controls | Where-Object { $_ -is [Windows.Forms.TextBox] } | Select-Object -First 1
        if (-not $pathTextBox) { throw 'Path input missing' }
        $pathTextBox.Text = $script:chosen
        $bitmap = New-Object Drawing.Bitmap($form.Width, $form.Height)
        try {
            $form.DrawToBitmap($bitmap, (New-Object Drawing.Rectangle(0,0,$form.Width,$form.Height)))
            $bitmap.Save((Join-Path $PSScriptRoot 'dist\install-path-dialog.png'))
        } finally { $bitmap.Dispose() }
        $form.DialogResult = 'OK'
    } catch { $script:failure = $_; foreach($f in [Windows.Forms.Application]::OpenForms){$f.DialogResult='Cancel'} }
})
try {
    $timer.Start()
    $result = Select-CrystalMixFolder 'C:\Example\CrystalMix'
    if ($script:failure) { throw $script:failure }
    if ($result -ne $script:chosen) { throw 'Selected path was not returned' }
    'PASS: installation folder dialog returns the entered Unicode path'
} finally { $timer.Dispose() }

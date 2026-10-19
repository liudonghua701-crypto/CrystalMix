# 安装路径验证和选择界面；不改变注册表或文件。
function Resolve-CrystalMixPath([string]$Path, [string]$SourceDirectory) {
    if ([string]::IsNullOrWhiteSpace($Path) -or $Path -notmatch '^[A-Za-z]:[\\/]') {
        throw '请选择本地磁盘上的绝对路径 / Choose an absolute path on a local drive.'
    }
    if ($Path -match '[<>"|?*]' -or $Path.Substring(2).Contains(':')) { throw '路径包含无效字符 / Invalid path characters.' }
    $full = [IO.Path]::GetFullPath($Path).TrimEnd('\', '/')
    if ($full.Length -le 3 -or $full -eq [IO.Path]::GetFullPath($SourceDirectory).TrimEnd('\')) {
        throw '不能安装到磁盘根目录或安装源目录 / Cannot use a drive root or the installer source.'
    }
    $check = $full
    while ($check) {
        if ((Test-Path -LiteralPath $check) -and ((Get-Item -LiteralPath $check -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw '安装路径不能经过目录链接 / Installation through directory links is not supported.'
        }
        $check = Split-Path -Parent $check
    }
    if ((Test-Path -LiteralPath $full) -and -not (Test-Path -LiteralPath $full -PathType Container)) { throw '目标不是文件夹 / Target is not a folder.' }
    if ((Test-Path -LiteralPath $full) -and (Get-ChildItem -LiteralPath $full -Force | Select-Object -First 1) -and
        -not (Test-Path -LiteralPath (Join-Path $full 'CrystalMix.exe'))) {
        throw '请选择空文件夹或已有 CrystalMix 安装目录 / Choose an empty folder or an existing CrystalMix installation.'
    }
    return $full
}

function Select-CrystalMixFolder([string]$DefaultPath) {
    Add-Type -AssemblyName System.Windows.Forms
    $form = New-Object Windows.Forms.Form
    $form.Text = 'CrystalMix 安装位置 / Installation folder'
    $form.StartPosition = 'CenterScreen'; $form.Size = New-Object Drawing.Size(720, 270)
    $form.MinimumSize = $form.Size; $form.AutoScaleMode = 'Font'
    $form.Font = New-Object Drawing.Font('Segoe UI', 10)
    $panel = New-Object Windows.Forms.TableLayoutPanel
    $panel.Dock = 'Fill'; $panel.Padding = 12; $panel.ColumnCount = 1; $panel.RowCount = 3
    $label = New-Object Windows.Forms.Label
    $label.Text = "请选择专用安装文件夹，可直接输入完整路径。`nChoose a dedicated installation folder or enter its full path."
    $label.AutoSize = $true; $label.Dock = 'Fill'
    # $input 是 PowerShell 自动变量，不能用作事件回调中的控件变量。
    $pathTextBox = New-Object Windows.Forms.TextBox
    $pathTextBox.Text = $DefaultPath; $pathTextBox.Dock = 'Top'
    $buttons = New-Object Windows.Forms.FlowLayoutPanel
    $buttons.AutoSize = $true; $buttons.Dock = 'Fill'
    $browse = New-Object Windows.Forms.Button
    $browse.Text = '浏览 / Browse'; $browse.AutoSize = $true
    $browse.Add_Click({
        $dialog = New-Object Windows.Forms.FolderBrowserDialog
        $dialog.Description = '选择 CrystalMix 专用文件夹 / Select a dedicated CrystalMix folder'
        $dialog.SelectedPath = $pathTextBox.Text
        try {
            if ($dialog.ShowDialog($form) -eq 'OK') {
                $selectedPath = $dialog.SelectedPath
                # 选择磁盘时使用专用子目录，避免把程序文件散放到根目录。
                if ($selectedPath.TrimEnd('\') -eq [IO.Path]::GetPathRoot($selectedPath).TrimEnd('\')) {
                    $selectedPath = Join-Path $selectedPath 'CrystalMix'
                }
                $pathTextBox.Text = $selectedPath
            }
        }
        finally { $dialog.Dispose() }
    }.GetNewClosure())
    $ok = New-Object Windows.Forms.Button
    $ok.Text = '安装 / Install'; $ok.AutoSize = $true; $ok.DialogResult = 'OK'
    $cancel = New-Object Windows.Forms.Button
    $cancel.Text = '取消 / Cancel'; $cancel.AutoSize = $true; $cancel.DialogResult = 'Cancel'
    $buttons.Controls.AddRange(@($browse, $ok, $cancel))
    $panel.Controls.Add($label); $panel.Controls.Add($pathTextBox); $panel.Controls.Add($buttons)
    $form.Controls.Add($panel); $form.AcceptButton = $ok; $form.CancelButton = $cancel
    try { if ($form.ShowDialog() -eq 'OK') { return $pathTextBox.Text }; return $null }
    finally { $form.Dispose() }
}

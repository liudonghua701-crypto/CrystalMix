namespace CrystalMix;

internal sealed class HelpDialog : Form
{
    public HelpDialog(Localizer localizer)
    {
        Text = localizer["helpTitle"]; Font = new Font("Segoe UI", 11);
        AutoScaleMode = AutoScaleMode.Font; StartPosition = FormStartPosition.CenterParent;
        Size = new Size(800, 700); MinimumSize = new Size(460, 360);
        Icon = AppIcon.Load();
        var close = new Button { Text = localizer.Language == UiLanguage.English ? "Close" : "关闭", AutoSize = true, DialogResult = DialogResult.OK };
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8), FlowDirection = FlowDirection.RightToLeft };
        footer.Controls.Add(close);
        var text = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
            ScrollBars = ScrollBars.Vertical, WordWrap = true, BackColor = Color.White,
            BorderStyle = BorderStyle.None, Text = (localizer["helpBody"] + AdvancedHelp.Get(localizer.Language)).Replace("\n", Environment.NewLine) };
        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
        body.Controls.Add(text); Controls.Add(body); Controls.Add(footer); CancelButton = close;
        Shown += (_, _) => { text.SelectionStart = 0; text.SelectionLength = 0; close.Focus(); };
    }
}

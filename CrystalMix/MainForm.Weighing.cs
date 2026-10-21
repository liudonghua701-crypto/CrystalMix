using System.Globalization;
using CrystalMix.Calculation;

namespace CrystalMix;

public sealed partial class MainForm
{
    private readonly Button _weighingButton = new();
    private string W(string en, string zh) => _localizer.Language == UiLanguage.English ? en : zh;
    private void ScaleFromActualMass()
    {
        _targetTimer.Stop();
        if (!Calculate(showErrors: true) || _lastResults is null) return;
        var old = _lastResults;
        var inputs = old.Select(r => new RecipeInput(r.Name, r.Formula, r.MolarRatio)).ToArray();
        TryParsePositive(_totalMassText.Text, out var total);
        using var dialog = new Form { Text = W("Scale from actual mass", "按实称质量缩放"),
            Font = Font, AutoScaleMode = AutoScaleMode.Font, StartPosition = FormStartPosition.CenterParent,
            Size = new Size(620, 620), MinimumSize = new Size(440, 440), MinimizeBox = false, MaximizeBox = false };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(16) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i=0; i<6; i++) layout.RowStyles.Add(new RowStyle(i == 4 ? SizeType.Percent : SizeType.AutoSize, i == 4 ? 100 : 0));
        var note = new Label { AutoSize = true, Dock = DockStyle.Fill,
            Text = W("Choose one measured material. All molar ratios stay unchanged; other rows become new weighing targets. Other already-weighed materials are not locked.",
                "选择一种已称原料作为基准。摩尔配比保持不变，其余原料更新为新的应称质量；其他已称原料不会被锁定。") };
        layout.SizeChanged += (_, _) => note.MaximumSize = new Size(Math.Max(200, layout.ClientSize.Width - layout.Padding.Horizontal), 0);
        var choice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, Name = "weighingMaterial" };
        choice.Items.AddRange(old.Select((r,i) => (object)$"{i+1}. {r.Formula}  {r.Name} — {r.RequiredMass:G9} g").ToArray());
        choice.SelectedIndex = 0;
        var label = new Label { Text = W("Actual measured mass (g)", "实称质量（克）"), AutoSize = true };
        var actual = new TextBox { Dock = DockStyle.Top, Name = "actualMass" };
        var output = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill,
            BackColor = Color.White, Name = "weighingPreview" };
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var apply = new Button { Text = W("Apply scale", "应用缩放"), AutoSize = true, Enabled = false, Name = "applyScale" };
        var cancel = new Button { Text = W("Cancel", "取消"), AutoSize = true, DialogResult = DialogResult.Cancel };
        actions.Controls.Add(apply); actions.Controls.Add(cancel);
        layout.Controls.Add(note); layout.Controls.Add(choice); layout.Controls.Add(label);
        layout.Controls.Add(actual); layout.Controls.Add(output); layout.Controls.Add(actions); dialog.Controls.Add(layout);
        dialog.CancelButton = cancel; dialog.AcceptButton = apply;
        WeighingScaleResult? preview = null;
        void Refresh()
        {
            preview = null; apply.Enabled = false;
            if (!TryParsePositive(actual.Text, out var mass))
            { output.Text = W("Enter a finite positive measured mass in grams.", "请输入有限的正数实称质量，单位为克。"); return; }
            try
            {
                preview = WeighingScale.Calculate(inputs, total, choice.SelectedIndex, mass);
                output.Text = $"{W("Scale", "缩放倍数")}: {preview.ScaleFactor:G9}\r\n"
                    + $"{W("Batch total", "整批总质量")}: {total:G9} → {preview.TotalMass:G9} g\r\n\r\n"
                    + string.Join("\r\n", preview.Materials.Select((r,i) =>
                        $"{i+1}. {r.Formula}: {old[i].RequiredMass:G9} → {r.RequiredMass:G9} g"));
                apply.Enabled = true;
            }
            catch (ArgumentException) { output.Text = W("The values exceed the supported numeric range.", "数值超出可计算范围，请调整实称质量或摩尔份数。"); }
        }
        actual.TextChanged += (_, _) => Refresh(); choice.SelectedIndexChanged += (_, _) => Refresh();
        apply.Click += (_, _) => { if (preview is not null) dialog.DialogResult = DialogResult.OK; };
        Refresh();
        if (dialog.ShowDialog(this) == DialogResult.OK && preview is not null)
        {
            _totalMassText.Text = preview.TotalMass.ToString("R", CultureInfo.InvariantCulture);
            Calculate(showErrors: true);
        }
    }
}

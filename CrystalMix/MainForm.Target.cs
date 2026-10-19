using System.Globalization;
using CrystalMix.Chemistry;
using CrystalMix.Calculation;

namespace CrystalMix;

public sealed partial class MainForm
{
    private readonly TextBox _targetFormula = new() { Width = 270, PlaceholderText = "Ba1-xKxFe2As2" };
    private readonly TextBox _variableX = new() { Width = 65, Text = "0.4" };
    private readonly TextBox _basis = new() { Width = 340, PlaceholderText = "BaFe2As2, KFe2As2" };
    private readonly Label _targetSummary = new() { AutoSize = true, MaximumSize = new Size(950, 0) };
    private readonly System.Windows.Forms.Timer _targetTimer = new() { Interval = 650 };

    private Control BuildTargetPanel()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true,
            ColumnCount = 1, RowCount = 3, Padding = new Padding(0, 6, 0, 6) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var first = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
        first.Controls.Add(new Label { Text = "目标化学式 / Target", AutoSize = true, Anchor = AnchorStyles.Left });
        first.Controls.Add(_targetFormula);
        first.Controls.Add(new Label { Text = "x =", AutoSize = true, Anchor = AnchorStyles.Left });
        first.Controls.Add(_variableX);
        var second = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
        second.Controls.Add(new Label { Text = "选用原料 / Materials", AutoSize = true, Anchor = AnchorStyles.Left });
        second.Controls.Add(_basis);
        var apply = new Button { Text = "重新填写 / Refill", AutoSize = true };
        second.Controls.Add(apply);
        panel.Controls.Add(first); panel.Controls.Add(second); panel.Controls.Add(_targetSummary);
        _targetSummary.Text = "留空原料栏：建议元素份数；可填逗号分隔的母体化合物。表格始终可编辑。\nBlank materials: elemental basis. Or enter comma-separated precursors; the table remains editable.";
        foreach (var box in new[] { _targetFormula, _variableX, _basis })
            box.TextChanged += (_, _) => { _targetTimer.Stop(); _targetTimer.Start(); };
        _targetTimer.Tick += (_, _) => { _targetTimer.Stop(); FillTarget(); };
        apply.Click += (_, _) => { _targetTimer.Stop(); FillTarget(); };
        panel.SizeChanged += (_, _) => _targetSummary.MaximumSize = new Size(Math.Max(250, panel.ClientSize.Width - 12), 0);
        FormClosed += (_, _) => _targetTimer.Dispose();
        return panel;
    }

    private void FillTarget()
    {
        if (string.IsNullOrWhiteSpace(_targetFormula.Text)) { _targetSummary.Text = "尚未设置目标 / No target set"; return; }
        try
        {
            if (!double.TryParse(_variableX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                throw new FormatException("请输入有效的 x / Enter a valid x.");
            var target = TargetFormula.Evaluate(_targetFormula.Text, x);
            var materials = _basis.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var suggestion = TargetRecipe.Suggest(target, materials);
            _grid.EndEdit(); _grid.Rows.Clear();
            foreach (var input in suggestion)
                _grid.Rows.Add(input.Name, input.Formula, input.MolarRatio.ToString("G12", CultureInfo.InvariantCulture));
            _targetSummary.ForeColor = Ink;
            _targetSummary.Text = "目标元素组成 / Target: " + string.Join("; ", target.Composition.Select(p => $"{p.Key} = {p.Value:G6}"))
                + "\n以下为可编辑的实际原料；仅作计量建议，不代表唯一合成路线。 / Editable materials; stoichiometric suggestion, not a unique synthesis route.";
            Calculate(false);
        }
        catch (Exception ex) when (ex is FormulaException or FormatException or ArgumentException)
        {
            _targetSummary.ForeColor = Color.Firebrick;
            _targetSummary.Text = "目标未应用，保留原料表 / Target not applied; table retained: " + ex.Message;
        }
    }

    private void RecognizeMaterial(DataGridViewRow row)
    {
        var formula = CellText(row, 1).Trim();
        try
        {
            var description = ElementNames.Describe(formula);
            row.Cells[1].ToolTipText = description;
            var composition = FormulaParser.Parse(formula).Composition;
            var nameCell = row.Cells[0];
            var previous = nameCell.Tag as string;
            var current = CellText(row, 0);
            if (current.Length == 0 || current == previous)
            {
                var name = composition.Count == 1 && composition.Single().Value == 1
                    ? ElementNames.Get(composition.Single().Key) : "";
                nameCell.Value = name; nameCell.Tag = name;
            }
        }
        catch (FormulaException) { row.Cells[1].ToolTipText = ""; }
    }
}

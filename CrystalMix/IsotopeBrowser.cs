using CrystalMix.Chemistry;
using System.Globalization;

namespace CrystalMix;

public sealed class IsotopeBrowser : Form
{
    private readonly TextBox _search = new() { Width = 300, PlaceholderText = "Lu / 镥 / Lutetium / [175Lu]" };
    private readonly ComboBox _filter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 390 };
    private readonly DataGridView _table = new();
    private readonly Label _count = new() { AutoSize = true };
    public string? SelectedSpecies { get; private set; }

    public IsotopeBrowser()
    {
        Text = "同位素库 / Isotope catalog"; Size = new Size(1120, 680); MinimumSize = new Size(820, 500);
        Icon = AppIcon.Load();
        Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Font; StartPosition = FormStartPosition.CenterParent;
        var layout = new TableLayoutPanel { ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var note = new Label { AutoSize = true, Dock = DockStyle.Fill,
            Text = "默认：稳定核素及半衰期 ≥ 1 年。核稳定性不等于化合物稳定性；长寿命不等于无放射性。\nDefault: stable or half-life ≥ 1 year. Nuclear lifetime does not determine chemical stability. Snapshot: 2026-10-19." };
        var filters = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        _filter.Items.AddRange(new object[] { "稳定 + 半衰期 ≥ 1 年 / Stable + ≥ 1 year", "仅稳定核素 / Stable only", "全部已收录 / All available" });
        filters.Controls.AddRange(new Control[] { _search, _filter });
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        var add = new Button { AutoSize = true, Text = "加入原料表 / Add material" };
        var close = new Button { AutoSize = true, Text = "关闭 / Close", DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange(new Control[] { add, close, _count });
        _table.Dock = DockStyle.Fill; _table.ReadOnly = true; _table.AllowUserToAddRows = false;
        _table.AllowUserToDeleteRows = false; _table.RowHeadersVisible = false;
        _table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _table.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _table.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _table.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _table.MultiSelect = false;
        foreach (var header in new[] { "输入语法 / Syntax", "元素名称 / Element", "原子质量 / u", "半衰期 / Half-life", "质量来源 / Mass source", "寿命来源 / Lifetime source" })
            _table.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = header, MinimumWidth = 115, SortMode = DataGridViewColumnSortMode.NotSortable });
        layout.Controls.Add(note, 0, 0); layout.Controls.Add(filters, 0, 1); layout.Controls.Add(_table, 0, 2); layout.Controls.Add(buttons, 0, 3);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(layout); Controls.Add(scroll); CancelButton = close;
        void ResizeContent()
        {
            // 在大字体、小窗口下保留表格高度，其余内容可滚动访问。
            float scale = Font.Size / 10f;
            foreach (DataGridViewColumn column in _table.Columns) column.MinimumWidth = (int)(150 * scale);
            layout.Size = new Size(Math.Max(780, scroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth),
                Math.Max(scroll.ClientSize.Height, (int)(620 * scale)));
        }
        scroll.SizeChanged += (_, _) => ResizeContent();
        FontChanged += (_, _) => ResizeContent();
        ResizeContent();
        _search.TextChanged += (_, _) => RefreshRows(); _filter.SelectedIndexChanged += (_, _) => RefreshRows();
        add.Click += (_, _) => SelectCurrent();
        _table.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) SelectCurrent(); };
        _filter.SelectedIndex = 0;
    }

    public static IEnumerable<IsotopeInfo> Find(string query, int filter)
    {
        return IsotopeCatalog.All.Where(i => (filter == 2 || (filter == 1 ? i.Stable : i.IsStableOrLongLived())) &&
            (string.IsNullOrWhiteSpace(query) || i.Symbol.Equals(query, StringComparison.OrdinalIgnoreCase) ||
             i.Species.Contains(query, StringComparison.OrdinalIgnoreCase) ||
             (ElementNames.Get(i.Symbol)?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)));
    }

    private void RefreshRows()
    {
        _table.Rows.Clear();
        foreach (var isotope in Find(_search.Text.Trim(), _filter.SelectedIndex))
            _table.Rows.Add(isotope.Species, ElementNames.Get(isotope.Symbol),
                isotope.Mass.ToString("G12", CultureInfo.InvariantCulture) + (isotope.Estimated ? " (估算 / est.)" : ""),
                isotope.Stable ? "稳定 / Stable" : isotope.HalfLifeText,
                isotope.MassSource, isotope.LifeSource);
        if (_table.Rows.Count > 0) _table.CurrentCell = _table.Rows[0].Cells[0];
        _count.Text = $"{_table.Rows.Count} / {IsotopeCatalog.All.Count} 核素 / nuclides" +
            (_table.Rows.Count == 0 ? "；可切换全部查看 / Try All available" : "");
    }

    private void SelectCurrent()
    {
        if (_table.CurrentRow is null) return;
        SelectedSpecies = (string)_table.CurrentRow.Cells[0].Value;
        DialogResult = DialogResult.OK;
    }
}

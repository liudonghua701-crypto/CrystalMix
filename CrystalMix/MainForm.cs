using System.Globalization;
using System.Text;
using CrystalMix.Calculation;
using CrystalMix.Chemistry;

namespace CrystalMix;

public sealed class MainForm : Form
{
    private static readonly Color Ink = Color.FromArgb(28, 43, 56);
    private static readonly Color Muted = Color.FromArgb(94, 108, 120);
    private static readonly Color Accent = Color.FromArgb(0, 132, 124);
    private static readonly Color AccentDark = Color.FromArgb(0, 105, 98);
    private static readonly Color Surface = Color.FromArgb(247, 249, 250);
    private static readonly Color Border = Color.FromArgb(215, 222, 226);
    private static readonly Color ErrorBackground = Color.FromArgb(255, 232, 232);

    private readonly Localizer _localizer = new();
    private readonly Label _titleLabel = new();
    private readonly Label _taglineLabel = new();
    private readonly Label _languageLabel = new();
    private readonly ComboBox _languageCombo = new();
    private readonly DataGridView _grid = new();
    private readonly Button _addButton = new();
    private readonly Button _deleteButton = new();
    private readonly Button _exampleButton = new();
    private readonly Button _clearButton = new();
    private readonly Button _helpButton = new();
    private readonly Label _totalMassLabel = new();
    private readonly TextBox _totalMassText = new();
    private readonly Label _gramsLabel = new();
    private readonly Button _calculateButton = new();
    private readonly Button _copyButton = new();
    private readonly Button _exportButton = new();
    private readonly ToolStripStatusLabel _statusLabel = new();
    private IReadOnlyList<RecipeResult>? _lastResults;

    public MainForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10F);
        Text = "CrystalMix";
        MinimumSize = new Size(940, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1180, 720);
        BackColor = Surface;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Surface,
            Padding = new Padding(22, 0, 22, 0)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        Controls.Add(root);

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildToolbar(), 0, 1);
        ConfigureGrid();
        root.Controls.Add(_grid, 0, 2);
        root.Controls.Add(BuildFooter(), 0, 3);
        root.Controls.Add(BuildStatusStrip(), 0, 4);

        _languageCombo.SelectedIndexChanged += (_, _) =>
        {
            _localizer.Language = _languageCombo.SelectedIndex == 1
                ? UiLanguage.English : UiLanguage.Chinese;
            ApplyLanguage();
            Calculate(showErrors: false);
        };
        _addButton.Click += (_, _) => AddMaterial();
        _deleteButton.Click += (_, _) => RemoveSelectedRows();
        _exampleButton.Click += (_, _) => LoadExample();
        _clearButton.Click += (_, _) => ClearRecipe(confirm: true);
        _helpButton.Click += (_, _) => MessageBox.Show(this, _localizer["helpBody"],
            _localizer["helpTitle"], MessageBoxButtons.OK, MessageBoxIcon.Information);
        _calculateButton.Click += (_, _) => Calculate(showErrors: true);
        _copyButton.Click += (_, _) => CopyResults();
        _exportButton.Click += (_, _) => ExportResults();
        _grid.CellEndEdit += (_, _) => Calculate(showErrors: false);
        _grid.RowsRemoved += (_, _) => InvalidateResults();
        _totalMassText.TextChanged += (_, _) => InvalidateResults();
        KeyPreview = true;
        KeyDown += HandleShortcut;

        var uiCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _languageCombo.SelectedIndex = uiCulture.Equals("en", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        LoadExample();
    }

    private Control BuildHeader()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
        _titleLabel.AutoSize = true;
        _titleLabel.Font = new Font("Segoe UI Semibold", 23F, FontStyle.Bold);
        _titleLabel.ForeColor = Ink;
        _titleLabel.Location = new Point(0, 20);
        _taglineLabel.AutoSize = true;
        _taglineLabel.Font = new Font("Segoe UI", 10.5F);
        _taglineLabel.ForeColor = Muted;
        _taglineLabel.Location = new Point(3, 69);

        _languageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _languageCombo.Items.AddRange(new object[] { "简体中文", "English" });
        _languageCombo.Width = 130;
        _languageCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _languageCombo.Location = new Point(panel.Width - 132, 50);
        _languageLabel.AutoSize = true;
        _languageLabel.ForeColor = Muted;
        _languageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _languageLabel.Location = new Point(panel.Width - 132, 22);
        panel.Resize += (_, _) =>
        {
            _languageCombo.Left = panel.ClientSize.Width - _languageCombo.Width;
            _languageLabel.Left = panel.ClientSize.Width - _languageCombo.Width;
        };

        panel.Controls.AddRange(new Control[] { _titleLabel, _taglineLabel, _languageLabel, _languageCombo });
        return panel;
    }

    private Control BuildToolbar()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = Surface
        };
        StyleButton(_addButton, primary: true);
        StyleButton(_deleteButton);
        StyleButton(_exampleButton);
        StyleButton(_clearButton);
        StyleButton(_helpButton);
        _addButton.Width = 132;
        _deleteButton.Width = 122;
        _exampleButton.Width = 112;
        _clearButton.Width = 90;
        _helpButton.Width = 104;
        panel.Controls.AddRange(new Control[] { _addButton, _deleteButton, _exampleButton, _clearButton, _helpButton });
        return panel;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.FixedSingle;
        _grid.GridColor = Border;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = true;
        _grid.RowHeadersWidth = 48;
        _grid.RowTemplate.Height = 38;
        _grid.ColumnHeadersHeight = 42;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.MultiSelect = true;
        _grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Ink;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
        _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _grid.DefaultCellStyle.BackColor = Color.White;
        _grid.DefaultCellStyle.ForeColor = Ink;
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 238, 235);
        _grid.DefaultCellStyle.SelectionForeColor = Ink;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(251, 252, 252);
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        _grid.Columns.Add(TextColumn("name", 1.25F));
        _grid.Columns.Add(TextColumn("formula", 1.05F));
        _grid.Columns.Add(TextColumn("ratio", .8F));
        _grid.Columns.Add(TextColumn("molarMass", 1F, readOnly: true));
        _grid.Columns.Add(TextColumn("fraction", .9F, readOnly: true));
        _grid.Columns.Add(TextColumn("mass", 1F, readOnly: true));
        _grid.Columns.Add(TextColumn("rowStatus", .9F, readOnly: true));
        _grid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        for (var i = 3; i <= 5; i++)
            _grid.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        for (var i = 3; i < _grid.Columns.Count; i++)
            _grid.Columns[i].DefaultCellStyle.BackColor = Color.FromArgb(247, 249, 250);
        _grid.RowPostPaint += (_, e) =>
        {
            var number = (e.RowIndex + 1).ToString(CultureInfo.InvariantCulture);
            TextRenderer.DrawText(e.Graphics, number, _grid.RowHeadersDefaultCellStyle.Font ?? Font,
                new Rectangle(e.RowBounds.Left, e.RowBounds.Top, _grid.RowHeadersWidth - 8, e.RowBounds.Height),
                Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        };
    }

    private static DataGridViewTextBoxColumn TextColumn(string name, float weight, bool readOnly = false) =>
        new() { Name = name, FillWeight = weight * 100, SortMode = DataGridViewColumnSortMode.NotSortable, ReadOnly = readOnly };

    private Control BuildFooter()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 1,
            Padding = new Padding(0, 20, 0, 14),
            BackColor = Surface
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 158));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
        _totalMassLabel.AutoSize = true;
        _totalMassLabel.Anchor = AnchorStyles.Left;
        _totalMassLabel.Font = new Font("Segoe UI Semibold", 10F);
        _totalMassLabel.Margin = new Padding(0, 0, 12, 0);
        _totalMassText.Dock = DockStyle.Fill;
        _totalMassText.Text = "10";
        _totalMassText.TextAlign = HorizontalAlignment.Right;
        _totalMassText.Font = new Font("Segoe UI Semibold", 11F);
        _totalMassText.Margin = new Padding(0, 3, 8, 3);
        _gramsLabel.AutoSize = true;
        _gramsLabel.Anchor = AnchorStyles.Left;
        _gramsLabel.ForeColor = Muted;
        _gramsLabel.Margin = new Padding(0, 0, 10, 0);
        StyleButton(_copyButton);
        StyleButton(_exportButton);
        StyleButton(_calculateButton, primary: true);
        _copyButton.Dock = _exportButton.Dock = _calculateButton.Dock = DockStyle.Fill;
        panel.Controls.Add(_totalMassLabel, 0, 0);
        panel.Controls.Add(_totalMassText, 1, 0);
        panel.Controls.Add(_gramsLabel, 2, 0);
        panel.Controls.Add(_copyButton, 4, 0);
        panel.Controls.Add(_exportButton, 5, 0);
        panel.Controls.Add(_calculateButton, 6, 0);
        return panel;
    }

    private Control BuildStatusStrip()
    {
        var strip = new StatusStrip
        {
            Dock = DockStyle.Fill,
            SizingGrip = false,
            BackColor = Surface,
            Padding = new Padding(0, 0, 0, 2)
        };
        _statusLabel.ForeColor = Muted;
        _statusLabel.Spring = true;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        strip.Items.Add(_statusLabel);
        return strip;
    }

    private static void StyleButton(Button button, bool primary = false)
    {
        button.Height = 38;
        button.Margin = new Padding(0, 0, 10, 0);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI Semibold", 9.5F);
        if (primary)
        {
            button.BackColor = Accent;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = AccentDark;
            button.FlatAppearance.MouseOverBackColor = AccentDark;
        }
        else
        {
            button.BackColor = Color.White;
            button.ForeColor = Ink;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 243, 245);
        }
    }

    private void ApplyLanguage()
    {
        Text = _localizer["appTitle"];
        _titleLabel.Text = _localizer["appTitle"];
        _taglineLabel.Text = _localizer["tagline"];
        _languageLabel.Text = _localizer["language"];
        _addButton.Text = _localizer["add"];
        _deleteButton.Text = _localizer["delete"];
        _exampleButton.Text = _localizer["example"];
        _clearButton.Text = _localizer["clear"];
        _helpButton.Text = _localizer["help"];
        _totalMassLabel.Text = _localizer["totalMass"];
        _gramsLabel.Text = _localizer["grams"];
        _calculateButton.Text = _localizer["calculate"];
        _copyButton.Text = _localizer["copy"];
        _exportButton.Text = _localizer["export"];
        foreach (DataGridViewColumn column in _grid.Columns)
            column.HeaderText = _localizer[column.Name];
        _statusLabel.Text = _localizer["ready"];
    }

    private void AddMaterial(string name = "", string formula = "", string ratio = "1")
    {
        var index = _grid.Rows.Add(name, formula, ratio, "", "", "", "");
        _grid.CurrentCell = _grid.Rows[index].Cells[formula.Length == 0 ? 1 : 2];
        _grid.BeginEdit(true);
        InvalidateResults();
    }

    private void LoadExample()
    {
        _grid.Rows.Clear();
        AddMaterial("Iron / 铁", "Fe", "1");
        AddMaterial("Selenium / 硒", "Se", "1");
        _totalMassText.Text = "10";
        Calculate(showErrors: false);
    }

    private void RemoveSelectedRows()
    {
        var indexes = _grid.SelectedCells.Cast<DataGridViewCell>()
            .Select(c => c.RowIndex).Distinct().OrderByDescending(i => i).ToList();
        if (indexes.Count == 0 && _grid.CurrentCell is not null)
            indexes.Add(_grid.CurrentCell.RowIndex);
        foreach (var index in indexes)
            if (index >= 0 && index < _grid.Rows.Count)
                _grid.Rows.RemoveAt(index);
        InvalidateResults();
    }

    private void ClearRecipe(bool confirm)
    {
        if (confirm && _grid.Rows.Count > 0 && MessageBox.Show(this, _localizer["confirmClear"],
                _localizer["question"], MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        _grid.Rows.Clear();
        AddMaterial();
        _statusLabel.Text = _localizer["ready"];
    }

    private bool Calculate(bool showErrors)
    {
        _grid.EndEdit();
        ResetRowStyles();
        _lastResults = null;

        if (!TryParsePositive(_totalMassText.Text, out var totalMass))
        {
            _totalMassText.BackColor = ErrorBackground;
            return ReportError(_localizer["badTotal"], showErrors);
        }
        _totalMassText.BackColor = Color.White;

        var inputs = new List<RecipeInput>();
        var dataRows = new List<DataGridViewRow>();
        var hasError = false;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            var name = CellText(row, 0);
            var formula = CellText(row, 1).Trim();
            var ratioText = CellText(row, 2).Trim();
            if (name.Length == 0 && formula.Length == 0 && ratioText.Length == 0)
                continue;

            if (formula.Length == 0)
            {
                MarkCellError(row, 1, _localizer["emptyFormula"]);
                hasError = true;
                continue;
            }
            if (!TryParsePositive(ratioText, out var ratio))
            {
                MarkCellError(row, 2, _localizer["badRatio"]);
                hasError = true;
                continue;
            }
            try
            {
                FormulaParser.Parse(formula);
            }
            catch (FormulaException ex)
            {
                MarkCellError(row, 1, FormulaError(ex));
                hasError = true;
                continue;
            }

            inputs.Add(new RecipeInput(name, formula, ratio));
            dataRows.Add(row);
        }

        if (_grid.Rows.Count == 0 || inputs.Count == 0 && !hasError)
            return ReportError(_localizer["noRows"], showErrors);
        if (hasError)
            return ReportError(_localizer["fixRows"], showErrors);

        var results = RecipeCalculator.Calculate(inputs, totalMass);
        for (var i = 0; i < results.Count; i++)
        {
            var row = dataRows[i];
            var result = results[i];
            row.Cells[3].Value = result.MolarMass.ToString("0.######", CultureInfo.InvariantCulture);
            row.Cells[4].Value = result.MassFraction.ToString("P4", CultureInfo.CurrentCulture);
            row.Cells[5].Value = result.RequiredMass.ToString("0.######", CultureInfo.InvariantCulture);
            row.Cells[6].Value = "✓ " + _localizer["ok"];
            row.Cells[6].Style.ForeColor = AccentDark;
        }
        _lastResults = results;
        _statusLabel.Text = string.Format(_localizer["calculated"], results.Count,
            totalMass.ToString("0.######", CultureInfo.InvariantCulture));
        return true;
    }

    private void ResetRowStyles()
    {
        foreach (DataGridViewRow row in _grid.Rows)
        {
            for (var i = 0; i < row.Cells.Count; i++)
            {
                row.Cells[i].Style.BackColor = i >= 3
                    ? Color.FromArgb(247, 249, 250) : Color.Empty;
                row.Cells[i].Style.ForeColor = Color.Empty;
            }
            for (var i = 3; i < row.Cells.Count; i++)
                row.Cells[i].Value = string.Empty;
        }
    }

    private void MarkCellError(DataGridViewRow row, int cellIndex, string message)
    {
        row.Cells[cellIndex].Style.BackColor = ErrorBackground;
        row.Cells[6].Value = message;
        row.Cells[6].Style.ForeColor = Color.FromArgb(176, 32, 37);
    }

    private string FormulaError(FormulaException ex)
    {
        var key = "formula." + ex.Code;
        var template = _localizer[key];
        return ex.Code switch
        {
            "unknownElement" => string.Format(template, ex.Detail),
            "missingBracket" => string.Format(template, ex.Detail),
            "expectedElement" or "mismatchedBracket" or "unexpected" =>
                string.Format(template, ex.Position + 1),
            _ => template
        };
    }

    private bool ReportError(string message, bool showErrors)
    {
        _statusLabel.Text = message;
        if (showErrors)
            MessageBox.Show(this, message, _localizer["error"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
    }

    private void InvalidateResults()
    {
        _lastResults = null;
        _statusLabel.Text = _localizer["ready"];
    }

    private void CopyResults()
    {
        if (!Calculate(showErrors: true) || _lastResults is null)
            return;
        Clipboard.SetText(BuildDelimited('\t'));
        _statusLabel.Text = _localizer["copied"];
    }

    private void ExportResults()
    {
        if (!Calculate(showErrors: true) || _lastResults is null)
            return;
        using var dialog = new SaveFileDialog
        {
            Filter = _localizer["csvFilter"],
            FileName = $"CrystalMix_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            AddExtension = true,
            DefaultExt = "csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        File.WriteAllText(dialog.FileName, "\uFEFF" + BuildDelimited(','), new UTF8Encoding(false));
        _statusLabel.Text = string.Format(_localizer["exported"], dialog.FileName);
    }

    private string BuildDelimited(char separator)
    {
        var sb = new StringBuilder();
        var headers = new[] { "name", "formula", "ratio", "molarMass", "fraction", "mass" };
        sb.AppendLine(string.Join(separator, headers.Select(h => Quote(_localizer[h], separator))));
        foreach (var result in _lastResults ?? Array.Empty<RecipeResult>())
        {
            var values = new[]
            {
                result.Name, result.Formula,
                result.MolarRatio.ToString("0.########", CultureInfo.InvariantCulture),
                result.MolarMass.ToString("0.######", CultureInfo.InvariantCulture),
                result.MassFraction.ToString("0.########", CultureInfo.InvariantCulture),
                result.RequiredMass.ToString("0.######", CultureInfo.InvariantCulture)
            };
            sb.AppendLine(string.Join(separator, values.Select(v => Quote(v, separator))));
        }
        return sb.ToString();
    }

    private static string Quote(string value, char separator)
    {
        if (!value.Contains(separator) && !value.Contains('"') && !value.Contains('\n'))
            return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static string CellText(DataGridViewRow row, int index) =>
        Convert.ToString(row.Cells[index].Value, CultureInfo.CurrentCulture) ?? string.Empty;

    private static bool TryParsePositive(string text, out double value)
    {
        var styles = NumberStyles.Float | NumberStyles.AllowThousands;
        var parsed = double.TryParse(text, styles, CultureInfo.CurrentCulture, out value) ||
                     double.TryParse(text, styles, CultureInfo.InvariantCulture, out value);
        return parsed && double.IsFinite(value) && value > 0;
    }

    private void HandleShortcut(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Enter)
        {
            Calculate(showErrors: true);
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.N)
        {
            AddMaterial();
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Delete && !_grid.IsCurrentCellInEditMode && _grid.Focused)
        {
            RemoveSelectedRows();
            e.SuppressKeyPress = true;
        }
    }
}

using System.Reflection;
using CrystalMix;

internal static class UiChecks
{
    public static void Run()
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { Check(); } catch (Exception ex) { failure = ex; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (failure != null) throw failure;
    }
    private static void Check()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Directory.CreateDirectory("dist/ui-checks");
        foreach (var scale in new[] { 1f, 1.5f, 2f })
        {
            using var form = new MainForm();
            form.Show(); Application.DoEvents();
            T Field<T>(string name) => (T)typeof(MainForm).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(form)!;
            void Fill() => typeof(MainForm).GetMethod("FillTarget", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(form, null);
            Field<TextBox>("_targetFormula").Text = "Ba1-xKxFe2As2";
            Field<TextBox>("_basis").Text = "BaFe2As2, KFe2As2";
            Fill();
            var grid = Field<DataGridView>("_grid");
            if (grid.Rows.Count != 2 || Math.Abs(Convert.ToDouble(grid.Rows[0].Cells[2].Value) - .6) > 1e-8)
                throw new Exception("Target refill failed");
            grid.Rows[0].Cells[0].Value = ""; grid.Rows[0].Cells[1].Value = "[57Fe]";
            var recognize = typeof(MainForm).GetMethod("RecognizeMaterial", BindingFlags.NonPublic | BindingFlags.Instance)!;
            recognize.Invoke(form, new object[] { grid.Rows[0] });
            if (Convert.ToString(grid.Rows[0].Cells[0].Value) != "铁 / Iron") throw new Exception("Automatic name recognition failed");
            grid.Rows[0].Cells[0].Value = "My sample"; grid.Rows[0].Cells[1].Value = "Se";
            recognize.Invoke(form, new object[] { grid.Rows[0] });
            if (Convert.ToString(grid.Rows[0].Cells[0].Value) != "My sample") throw new Exception("Custom name overwritten");
            Fill();
            grid.Rows[0].Cells[2].Value = "1e308";
            var calculate = typeof(MainForm).GetMethod("Calculate", BindingFlags.NonPublic | BindingFlags.Instance)!;
            if ((bool)calculate.Invoke(form, new object[] { false })!) throw new Exception("Overflow accepted");
            Fill();
            grid.Rows[0].Cells[2].Value = "0.7";
            if (Field<TextBox>("_targetFormula").Text != "Ba1-xKxFe2As2") throw new Exception("Target changed by editing table");
            Field<TextBox>("_targetFormula").Text = "Bad?"; Fill();
            if (Convert.ToString(grid.Rows[0].Cells[2].Value) != "0.7") throw new Exception("Invalid target overwrote table");
            Field<TextBox>("_targetFormula").Text = "Ba1-xKxFe2As2"; Fill();
            if (scale != 1) form.Font = new Font(form.Font.FontFamily, 10 * scale);
            foreach (var language in new[] { 0, 1 })
            {
                Field<ComboBox>("_languageCombo").SelectedIndex = language;
                form.Size = new Size((int)(1000 * scale), (int)(850 * scale));
                form.PerformLayout(); Application.DoEvents();
                if (grid.ColumnHeadersHeight < grid.ColumnHeadersDefaultCellStyle.Font.Height) throw new Exception("Header height clipped");
                foreach (var button in Descendants(form).OfType<Button>())
                    if (button.Height < button.GetPreferredSize(Size.Empty).Height) throw new Exception("Clipped button: " + button.Text);
                using var bitmap = new Bitmap(form.Width, form.Height);
                form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                bitmap.Save($"dist/ui-checks/layout-{scale}-{language}.png");
            }
            form.Size = form.MinimumSize; form.PerformLayout(); Application.DoEvents();
            using var small = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(small, new Rectangle(Point.Empty, small.Size));
            small.Save($"dist/ui-checks/small-{scale}.png");
            form.Close();
        }
    }
    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control c in root.Controls) { yield return c; foreach (var child in Descendants(c)) yield return child; }
    }
}

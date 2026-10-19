using System.Reflection;
using CrystalMix;

internal static class IsotopeUiChecks
{
    public static void Run()
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { Check(); } catch (Exception e) { failure = e; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (failure is not null) throw failure;
    }
    private static void Check()
    {
        Directory.CreateDirectory("dist/ui-checks");
        foreach (float scale in new[] { 1f, 1.5f, 2f })
        {
            using var form = new IsotopeBrowser();
            if (scale != 1) form.Font = new Font(form.Font.FontFamily, 10 * scale);
            form.Show(); form.Size = form.MinimumSize; Application.DoEvents();
            T Field<T>(string name) => (T)typeof(IsotopeBrowser).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            if (Field<ComboBox>("_filter").SelectedIndex != 0) throw new Exception("Wrong default filter");
            Field<TextBox>("_search").Text = "[175Lu]"; Application.DoEvents();
            var table = Field<DataGridView>("_table");
            if (table.Rows.Count != 1) throw new Exception("Lu search failed");
            if (table.Height < 100) throw new Exception("Catalog table collapsed at large font");
            if (table.Columns.Cast<DataGridViewColumn>().Any(c => c.Width < 150 * scale)) throw new Exception("Column width ignores font scale");
            var scroll = (Panel)form.Controls[0];
            if (scale > 1 && !scroll.VerticalScroll.Visible) throw new Exception("Large font content cannot scroll");
            scroll.ScrollControlIntoView(table); Application.DoEvents();
            if (table.ColumnHeadersHeight < table.Font.Height || table.Rows[0].Height < table.Font.Height) throw new Exception("Catalog text clipped");
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save($"dist/ui-checks/isotope-catalog-{scale}.png");
            typeof(IsotopeBrowser).GetMethod("SelectCurrent", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null);
            if (form.SelectedSpecies != "[175Lu]" || form.DialogResult != DialogResult.OK) throw new Exception("Material selection failed");
            form.Close();
        }
    }
}

using System.Reflection;
using CrystalMix;
using CrystalMix.Calculation;

internal static class WeighingUiChecks
{
    public static void Run()
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { Check(); } catch (Exception ex) { failure = ex; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (failure is not null) throw failure;
    }
    private static void Check()
    {
        using var form = new MainForm(); form.Show(); Application.DoEvents();
        T Field<T>(string name) => (T)typeof(MainForm).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(form)!;
        var grid = Field<DataGridView>("_grid"); grid.Rows.Clear();
        grid.Rows.Add("chunk", "Fe", "1"); grid.Rows.Add("powder", "Se", "1");
        Field<TextBox>("_totalMassText").Text = "10";
        Exception? failure = null;
        void Open(bool cancel)
        {
            using var timer = new System.Windows.Forms.Timer { Interval = 100 };
            timer.Tick += (_, _) =>
            {
                var dialog = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f != form);
                if (dialog is null) return;
                timer.Stop();
                try
                {
                    T Find<T>(string name) where T : Control => (T)dialog.Controls.Find(name, true).Single();
                    var input = Find<TextBox>("actualMass"); var apply = Find<Button>("applyScale");
                    input.Text = "0"; if (apply.Enabled) throw new Exception("Zero mass accepted");
                    input.Text = "NaN"; if (apply.Enabled) throw new Exception("NaN accepted");
                    input.Text = "5"; if (!apply.Enabled) throw new Exception("Valid mass rejected");
                    if (!Find<TextBox>("weighingPreview").Text.Contains("Se")) throw new Exception("Other material preview missing");
                    var bounds = dialog.RectangleToClient(apply.RectangleToScreen(apply.ClientRectangle));
                    if (!dialog.ClientRectangle.Contains(bounds)) throw new Exception("Apply button outside dialog");
                    Directory.CreateDirectory("dist/ui-checks");
                    using var bitmap = new Bitmap(dialog.Width, dialog.Height);
                    dialog.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                    bitmap.Save("dist/ui-checks/weighing-preview.png");
                    if (cancel) dialog.DialogResult = DialogResult.Cancel; else apply.PerformClick();
                }
                catch (Exception ex) { failure = ex; dialog.Close(); }
            };
            timer.Start(); Field<Button>("_weighingButton").PerformClick();
            if (failure is not null) throw failure;
        }
        Open(true);
        if (Field<TextBox>("_totalMassText").Text != "10") throw new Exception("Cancel changed total");
        Open(false);
        var rows = Field<IReadOnlyList<RecipeResult>>("_lastResults");
        if (Math.Abs(rows[0].RequiredMass - 5) > 1e-9 || Math.Abs(rows[1].RequiredMass-5*78.971/55.845)>1e-9)
            throw new Exception("Applied scale incorrect");
        if (Convert.ToString(grid.Rows[0].Cells[2].Value) != "1" || Convert.ToString(grid.Rows[1].Cells[2].Value) != "1")
            throw new Exception("Molar ratios changed");
        form.Close();
    }
}

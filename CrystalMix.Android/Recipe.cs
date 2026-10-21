using Android.Widget;
using CrystalMix.Chemistry;
using System.Globalization;
using System.Text;

namespace CrystalMix.Mobile;

public partial class MainActivity
{
    private TextView recipeOutput = null!;
    private string pendingCsv = string.Empty;
    private void ShowRecipe()
    {
        content.AddView(Text(L("Your weighing plan", "原料称量方案"), 22));
        Input(content, L("Total batch mass · g", "原料总质量 · 克"), state.Total, s => { state.Total = s; ClearResults(); }, true);
        materialResults.Clear();
        for (int i = 0; i < state.Materials.Count; i++) MaterialRow(state.Materials[i], i);
        recipeOutput = Text(""); recipeOutput.SetTextIsSelectable(true); content.AddView(recipeOutput);



    }
    private void ClearResults() { if (!building) Calculate(); }
    private void Calculate()
    {
        try
        {
            var results = Results();
            recipeOutput.Text = "";
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                materialResults[state.Materials[i]].Text = $"{r.RequiredMass:G8} g   ·   {r.MassFraction:P3}";
            }
            batchSummary.Text = $"{L("Total", "合计")}: {results.Sum(r => r.RequiredMass):G10} g  ·  {results.Count} {L("materials", "种原料")}";
            Save();
        }
        catch (Exception ex)
        {
            foreach (var result in materialResults.Values) result.Text = "—";
            batchSummary.Text = L("Complete the recipe to calculate", "完善原料输入后自动计算");
            recipeOutput.Text = Error(ex);
        }
    }
    protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Android.Content.Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode != 100 || resultCode != Android.App.Result.Ok || data?.Data is null) return;
        try
        {
            if (pendingCsv.Length == 0) { Message(L("Please export again after reopening the app.", "重新打开应用后请再次导出。")); return; }
            using var stream = ContentResolver!.OpenOutputStream(data.Data)!;
            using var writer = new StreamWriter(stream, new UTF8Encoding(true)); writer.Write(pendingCsv);
            Toast.MakeText(this, L("CSV saved", "CSV 已保存"), ToastLength.Short)!.Show();
        }
        catch (Exception ex) { Message(Error(ex)); }
    }
    private void Export(bool share, bool saveFile = false)
    {
        try
        {
            static string Csv(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
            var csv = "Name,Formula,Molar parts,Molar mass (g/mol),Mass fraction,Required mass (g)\r\n" +
                string.Join("\r\n", Results().Select(r => string.Join(",", Csv(r.Name), Csv(r.Formula),
                    r.MolarRatio.ToString("R", CultureInfo.InvariantCulture), r.MolarMass.ToString("R", CultureInfo.InvariantCulture),
                    r.MassFraction.ToString("R", CultureInfo.InvariantCulture), r.RequiredMass.ToString("R", CultureInfo.InvariantCulture))));
            if (saveFile)
            {
                pendingCsv = csv;
                var save = new Android.Content.Intent(Android.Content.Intent.ActionCreateDocument);
                save.AddCategory(Android.Content.Intent.CategoryOpenable); save.SetType("text/csv");
                save.PutExtra(Android.Content.Intent.ExtraTitle, "CrystalMix-recipe.csv");
                StartActivityForResult(save, 100); return;
            }
            if (share)
            {
                var intent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
                intent.SetType("text/plain"); intent.PutExtra(Android.Content.Intent.ExtraText, csv);
                intent.PutExtra(Android.Content.Intent.ExtraSubject, "CrystalMix recipe.csv");
                StartActivity(Android.Content.Intent.CreateChooser(intent, L("Share CSV text", "分享 CSV 文本")));
            }
            else
            {
                var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService)!;
                clipboard.PrimaryClip = Android.Content.ClipData.NewPlainText("CrystalMix", csv);
                Toast.MakeText(this, L("Results copied", "结果已复制"), ToastLength.Short)!.Show();
            }
        }
        catch (Exception ex) { Message(Error(ex)); }
    }
}

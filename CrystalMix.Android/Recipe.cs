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
        foreach (var material in state.Materials.ToArray())
        {
            var card = Stack(); card.SetPadding(Dp(12), Dp(8), Dp(12), Dp(8));
            card.SetBackgroundColor(Android.Graphics.Color.White);
            var name = Text(ElementNames.Get(material.Formula) ?? "", 13);
            Input(card, L("Material formula", "原料化学式"), material.Formula, s =>
            { material.Formula = s; name.Text = ElementNames.Get(s) ?? ""; ClearResults(); });
            card.AddView(name);
            Input(card, L("Custom label (optional)", "自定义名称（可选）"), material.Name, s => { material.Name = s; ClearResults(); });
            Input(card, L("Molar parts", "摩尔份数"), material.Parts, s => { material.Parts = s; ClearResults(); }, true);
            card.AddView(Button(L("Remove material", "删除此原料"), () => { state.Materials.Remove(material); ShowPage(0); Save(); }));
            content.AddView(card); content.AddView(Text("", 4));
        }
        content.AddView(Button(L("+ Add material", "+ 添加原料"), () =>
        { state.Materials.Add(new() { Formula = "", Parts = "1" }); ShowPage(0); Save(); }));
        content.AddView(Button(L("Calculate masses", "计算称量质量"), Calculate));
        recipeOutput = Text(""); recipeOutput.SetTextIsSelectable(true); content.AddView(recipeOutput);
        content.AddView(Button(L("Copy results", "复制结果"), () => Export(false)));
        content.AddView(Button(L("Share CSV", "分享 CSV"), () => Export(true)));
        content.AddView(Button(L("Save CSV file", "保存 CSV 文件"), () => Export(true, true)));
    }
    private void ClearResults() { if (recipeOutput is not null) recipeOutput.Text = ""; }
    private void Calculate()
    {
        try
        {
            var results = Results();
            recipeOutput.Text = string.Join("\n\n", results.Select(r =>
                $"{r.Formula}  {r.Name}\n{r.RequiredMass:G8} g   ·   {r.MassFraction:P3}\n{L("Molar mass", "摩尔质量")}: {r.MolarMass:G9} g/mol"))
                + $"\n\n{L("Total", "合计")}: {results.Sum(r => r.RequiredMass):G10} g";
            Save();
        }
        catch (Exception ex) { recipeOutput.Text = Error(ex); }
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

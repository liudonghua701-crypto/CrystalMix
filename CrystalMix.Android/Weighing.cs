using Android.App;
using Android.Widget;
using CrystalMix.Calculation;
using System.Globalization;

namespace CrystalMix.Mobile;

public partial class MainActivity
{
    private void UseActualMass(MaterialState material)
    {
        try
        {
            var inputs = state.Materials.Select(m => new RecipeInput(m.Name, m.Formula, Number(m.Parts))).ToArray();
            double total = Number(state.Total);
            var old = RecipeCalculator.Calculate(inputs, total);
            int anchor = state.Materials.IndexOf(material);
            var body = Stack(); body.SetPadding(Dp(16), 0, Dp(16), 0);
            body.AddView(Text($"{anchor+1}. {material.Formula}  {material.Name}", 20));
            body.AddView(Text(L("Planned", "原计划") + $": {old[anchor].RequiredMass:G9} g"));
            body.AddView(Text(L("Keep molar ratios; resize the entire batch using this one measured material. Other materials are new targets, not locked measurements.",
                "保持摩尔配比，以这一种原料的实称质量调整整批。其他原料显示新的应称量，不视为已固定的实称量。"), 14));
            string actual = "";
            WeighingScaleResult? preview = null;
            Button? apply = null;
            var output = Text(""); output.SetTextIsSelectable(true);
            void Refresh()
            {
                preview = null;
                try
                {
                    preview = WeighingScale.Calculate(inputs, total, anchor, Number(actual));
                    output.Text = $"{L("Scale", "缩放倍数")}: {preview.ScaleFactor:G9}\n"
                        + $"{L("Batch total", "整批总质量")}: {total:G9} → {preview.TotalMass:G9} g\n\n"
                        + string.Join("\n", preview.Materials.Select((r, i) =>
                            $"{i+1}. {r.Formula}: {old[i].RequiredMass:G9} → {r.RequiredMass:G9} g"));
                }
                catch (Exception ex) { output.Text = actual.Length == 0 ? L("Enter the measured mass in grams.", "请输入实称质量，单位为克。") : Error(ex); }
                if (apply is not null) apply.Enabled = preview is not null;
            }
            Input(body, L("Actual mass · g", "实称质量 · 克"), actual, s => { actual = s; Refresh(); }, true);
            body.AddView(output);
            var scroll = new ScrollView(this); scroll.AddView(body);
            var dialog = new AlertDialog.Builder(this)!.SetTitle(L("Scale from actual mass", "按实称质量缩放"))!
                .SetView(scroll)!.SetNegativeButton(L("Cancel", "取消"), (_, _) => { })!
                .SetPositiveButton(L("Apply scale", "应用缩放"), (_, _) =>
                {
                    if (preview is null) return;
                    state.Total = preview.TotalMass.ToString("R", CultureInfo.InvariantCulture);
                    RebuildRecipe();
                })!.Create()!;
            dialog.Show();
            dialog.Window!.SetLayout(-1, (int)(Resources!.DisplayMetrics!.HeightPixels * .85));
            apply = dialog.GetButton((int)Android.Content.DialogButtonType.Positive); Refresh();
        }
        catch (Exception ex) { Message(Error(ex)); }
    }
}

using Android.Widget;
using CrystalMix.Chemistry;
using CrystalMix.Calculation;
using System.Globalization;

namespace CrystalMix.Mobile;

public partial class MainActivity
{
    private void ShowTarget()
    {
        content.AddView(Text(L("Target composition", "目标元素组成"), 22));
        content.AddView(Text(L("A composition balance is not a synthesis route. Your actual materials remain editable.",
            "组成配平不代表合成路线。生成后仍可自由编辑实际原料。"), 14));
        var output = Text("");
        void Preview()
        {
            try
            {
                var target = TargetFormula.Evaluate(state.Target, Number(state.X));
                output.Text = string.Join(" · ", target.Composition.Select(p => $"{p.Key}: {p.Value:G6}")) + $"\n{target.MolarMass:G9} g/mol";
            }
            catch (Exception ex) { output.Text = Error(ex); }
        }
        Input(content, L("Target formula", "目标化学式"), state.Target, s => { state.Target = s; Preview(); });
        Input(content, "x (0–1)", state.X, s => { state.X = s; Preview(); }, true);
        content.AddView(output); Preview();
        Input(content, L("Actual materials · comma separated; blank = elements", "实际原料 · 逗号分隔；留空采用元素单质"), state.Basis, s => state.Basis = s);
        content.AddView(Button(L("Generate editable recipe", "生成可编辑配料表"), () =>
        {
            try
            {
                var target = TargetFormula.Evaluate(state.Target, Number(state.X));
                var basis = state.Basis.Split(new[] { ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var recipe = TargetRecipe.Suggest(target, basis);
                void Apply()
                {
                    state.Materials = recipe.Select(r => new MaterialState { Formula = r.Formula,
                        Parts = r.MolarRatio.ToString("G12", CultureInfo.InvariantCulture) }).ToList();
                    state.TargetExpanded = false;
                    ShowPage(0); Save();
                }
                new Android.App.AlertDialog.Builder(this)!.SetTitle(L("Replace the current recipe?", "替换当前配料表？"))!
                    .SetMessage(L("This fills the table with the calculated molar parts. Existing manual edits will be replaced.", "将按计算结果填写原料及摩尔比，替换现有手工编辑。"))!
                    .SetNegativeButton(L("Cancel", "取消"), (_, _) => { })!.SetPositiveButton(L("Use recipe", "使用此配比"), (_, _) => Apply())!.Show();
            }
            catch (Exception ex) { Message(Error(ex)); }
        }));
    }
    private Localizer Localizer => new() { Language = state.English ? UiLanguage.English : UiLanguage.Chinese };
    private string Error(Exception exception)
    {
        if (exception is FormulaException formula)
        {
            var arg = formula.Detail ?? formula.Position.ToString();
            try { return string.Format(Localizer["formula." + formula.Code], arg); }
            catch { return L("Invalid formula", "无效化学式") + ": " + arg; }
        }
        if (exception is ArgumentOutOfRangeException || exception is OverflowException)
            return L("Enter finite, positive amounts and a valid total mass.", "请输入有限的正数摩尔份数和有效总质量。");
        if (exception is FormatException)
            return L("Check numbers and expressions. Use a decimal point; x must be 0–1.", "请检查数值及表达式，使用小数点；x 必须在 0–1 之间。");
        return exception.Message;
    }
    private void ShowGuide()
    {
        content.AddView(Text(L("A small guide to precise batches", "配料计算使用说明"), 22));
        content.AddView(Text("CrystalMix Android 1.2.2", 13));
        content.AddView(Text(L("Actual materials starts empty. Leave it blank to generate elemental molar parts, or enter comma-separated material formulas to balance using those materials. Clear the field to return to elemental proportions. No switch is required.",
            "实际原料初始留空。留空时按目标生成元素单质配比；输入逗号分隔的原料化学式后，按所填原料配平。清空即可恢复元素配比，无需开关。")));
        content.AddView(Text(L("One workspace: expand Target composition to generate a recipe. Edit formulas, molar parts and total mass; weighing results update in each row. Use the bottom bar to add materials or isotopes. More contains exports and this guide. Target edits only change the preview until you confirm replacement.",
            "单页工作台：展开目标组成可生成配料表。修改化学式、摩尔份数或总质量，每行称量结果即时更新。底部可添加原料或同位素；更多菜单提供导出与帮助。目标编辑仅更新预览，确认替换后才修改原料。")));
        var instructions = AdvancedHelp.Get(Localizer.Language)
            .Replace("Changing the target, x or material selection refills the table.", "On Android, use Generate editable recipe to apply the target after confirmation.")
            .Replace("修改目标、x 或选用原料后自动重新填写；", "安卓端点击生成配料表并确认后填写；");
        var help = Text(Localizer["helpBody"] + instructions);
        help.SetTextIsSelectable(true); content.AddView(help);
        content.AddView(Text(L("On Android, target edits update the composition preview. Use Generate editable recipe to replace the recipe after confirmation. Drafts are stored on this device. Share CSV sends text through Android's share sheet; Save CSV file opens the system file picker.",
            "安卓端修改目标后更新组成预览；点击生成配料表并确认后才替换实际原料。草稿保存在本机。分享 CSV 通过安卓分享菜单发送文本；保存 CSV 文件可选择文件位置。")));
    }
}

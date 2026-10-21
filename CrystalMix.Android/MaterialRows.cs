using Android.App;
using Android.Widget;
using CrystalMix.Chemistry;

namespace CrystalMix.Mobile;

public partial class MainActivity
{
    private readonly Dictionary<MaterialState, TextView> materialResults = new();
    private void MaterialRow(MaterialState material, int index)
    {
        var card = Stack(); card.SetPadding(Dp(12), Dp(4), Dp(12), Dp(8));
        card.SetBackgroundColor(Android.Graphics.Color.White);
        var line = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        var formula = Stack(); var parts = Stack();
        var name = Text(ElementNames.Get(material.Formula) ?? "", 12);
        Input(formula, L("Material", "原料") + $" {index + 1}", material.Formula, s =>
        { material.Formula = s; name.Text = ElementNames.Get(s) ?? ""; ClearResults(); });
        Input(parts, L("Molar parts", "摩尔份数") + $" {index + 1}", material.Parts,
            s => { material.Parts = s; ClearResults(); }, true);
        line.AddView(formula, new LinearLayout.LayoutParams(0, -2, 2));
        line.AddView(parts, new LinearLayout.LayoutParams(0, -2, 1));
        card.AddView(line); card.AddView(name);
        var result = Text("", 18); result.SetTextColor(accent); result.SetTextIsSelectable(true);
        materialResults[material] = result; card.AddView(result);
        card.AddView(Button(L("Use actual mass", "按实称质量调整"), () => UseActualMass(material)));
        card.AddView(Button(string.IsNullOrWhiteSpace(material.Name) ? L("Label / remove", "备注 / 删除") : material.Name,
            () => EditMaterial(material)));
        content.AddView(card); content.AddView(Text("", 2));
    }
    private void EditMaterial(MaterialState material)
    {
        var body = Stack(); body.SetPadding(Dp(20), 0, Dp(20), 0);
        var label = material.Name;
        Input(body, L("Custom label", "自定义名称"), label, s => label = s);
        new AlertDialog.Builder(this)!.SetTitle(material.Formula)!.SetView(body)!
            .SetPositiveButton(L("Save", "保存"), (_, _) => { material.Name = label; RebuildRecipe(); })!
            .SetNeutralButton(L("Remove", "删除"), (_, _) => { state.Materials.Remove(material); RebuildRecipe(); })!
            .SetNegativeButton(L("Cancel", "取消"), (_, _) => { })!.Show();
    }
    private void RebuildRecipe()
    {
        int offset = workspaceScroll.ScrollY; ShowPage(0); Save();
        workspaceScroll.Post(() => workspaceScroll.ScrollTo(0, offset));
    }
}

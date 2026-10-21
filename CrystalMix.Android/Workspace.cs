using Android.App;
using Android.Views;
using Android.Widget;

namespace CrystalMix.Mobile;

public partial class MainActivity
{
    private ScrollView workspaceScroll = null!;
    private AlertDialog? toolDialog;
    private TextView batchSummary = null!;
    private bool building;
    private void ShowWorkspace()
    {
        building = true;
        var toggle = Button(L("Target composition ▾", "目标组成 ▾"), () => { });
        var targetPanel = Stack();
        toggle.Click += (_, _) =>
        {
            state.TargetExpanded = !state.TargetExpanded;
            targetPanel.Visibility = state.TargetExpanded ? ViewStates.Visible : ViewStates.Gone;
            Save();
        };
        content.AddView(toggle);
        var workspace = content; content = targetPanel; ShowTarget(); content = workspace;
        targetPanel.Visibility = state.TargetExpanded ? ViewStates.Visible : ViewStates.Gone;
        content.AddView(targetPanel);
        ShowRecipe(); building = false; Calculate();
    }
    private void BuildActionBar()
    {
        batchSummary = Text("", 14); root.AddView(batchSummary);
        var bar = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        foreach (var item in new (string Label, Action Run)[] {
            (L("+ Material", "+ 原料"), AddMaterial),
            (L("Isotopes", "同位素"), () => OpenTool(ShowCatalog)),
            (L("More", "更多"), ShowActions) })
            bar.AddView(Button(item.Label, item.Run), new LinearLayout.LayoutParams(0, -2, 1));
        root.AddView(bar);
    }
    private void AddMaterial()
    {
        state.Materials.Add(new() { Formula = "", Parts = "1" });
        ShowPage(0); Save();
        workspaceScroll.Post(() => workspaceScroll.FullScroll(FocusSearchDirection.Down));
    }
    private void ShowActions() => new AlertDialog.Builder(this)!
        .SetTitle(L("Recipe actions", "配料操作"))!
        .SetItems(new[] { L("Copy results", "复制结果"), L("Share CSV", "分享 CSV"),
            L("Save CSV file", "保存 CSV 文件"), L("Guide", "使用说明") }, (_, e) =>
        { if (e.Which == 3) OpenTool(ShowGuide); else Export(e.Which != 0, e.Which == 2); })!.Show();
    private void OpenTool(Action build)
    {
        var workspace = content;
        var body = Stack(); body.SetPadding(Dp(16), 0, Dp(16), 0);
        content = body;
        try { build(); } finally { content = workspace; }
        var scroll = new ScrollView(this); scroll.AddView(body);
        toolDialog = new AlertDialog.Builder(this)!.SetView(scroll)!
            .SetPositiveButton(L("Back to recipe", "返回配料"), (_, _) => { })!.Create();
        toolDialog!.Show();
        toolDialog.Window!.SetLayout(-1, (int)(Resources!.DisplayMetrics!.HeightPixels * .88));
    }
}

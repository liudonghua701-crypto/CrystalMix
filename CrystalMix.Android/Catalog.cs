using Android.Widget;
using CrystalMix.Chemistry;

namespace CrystalMix.Mobile;

public partial class MainActivity
{
    private void ShowCatalog()
    {
        content.AddView(Text(L("Find your isotope", "查找指定核素"), 22));
        content.AddView(Text(L("Nuclear lifetime is not chemical stability. 3,498 source-backed records, available offline.",
            "核寿命不等于化学稳定性。3,498 条带来源记录，完全离线可用。"), 14));
        string query = ""; int filter = 0, offset = 0;
        var list = Stack(); var count = Text("");
        void Refresh()
        {
            list.RemoveAllViews();
            var matches = IsotopeCatalog.All.Where(i =>
                (filter == 2 || (filter == 1 ? i.Stable : i.IsStableOrLongLived())) &&
                (query.Length == 0 || i.Species.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (ElementNames.Get(i.Symbol)?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))).ToArray();
            count.Text = $"{matches.Length} / {IsotopeCatalog.All.Count} " + L("records", "条记录");
            foreach (var isotope in matches.Skip(offset).Take(20))
            {
                var label = $"{isotope.Species}  {ElementNames.Get(isotope.Symbol)}\n"
                    + $"{isotope.Mass:G12} u{(isotope.Estimated ? L(" (estimated)", "（估算）") : "")}\n"
                    + $"{L("Half-life", "半衰期")}: {(isotope.Stable ? L("Stable", "稳定") : isotope.HalfLifeText)}\n"
                    + $"{isotope.MassSource} · {isotope.LifeSource}";
                list.AddView(Text(label));
                list.AddView(Button(L("Add ", "添加 ") + isotope.Species, () =>
                {
                    state.Materials.Add(new() { Formula = isotope.Species, Parts = "1" }); Save();
                    toolDialog?.Dismiss(); ShowPage(0);
                    workspaceScroll.Post(() => workspaceScroll.FullScroll(Android.Views.FocusSearchDirection.Down));
                }));
            }
            if (offset > 0) list.AddView(Button(L("Previous 20", "上一页"), () => { offset -= 20; Refresh(); }));
            if (offset + 20 < matches.Length) list.AddView(Button(L("Next 20", "下一页"), () => { offset += 20; Refresh(); }));
        }
        Input(content, L("Search · Lu, Lutetium, [175Lu]", "搜索 · 镥、Lu、[175Lu]"), "", s => { query = s.Trim(); offset = 0; Refresh(); });
        var picker = new Spinner(this);
        picker.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
            new[] { L("Stable + half-life ≥ 1 year", "稳定 + 半衰期至少 1 年"), L("Stable only", "仅稳定核素"), L("All available", "全部已收录") });
        picker.ItemSelected += (_, e) => { filter = e.Position; offset = 0; Refresh(); };
        content.AddView(picker); content.AddView(count); content.AddView(list); Refresh();
    }
}

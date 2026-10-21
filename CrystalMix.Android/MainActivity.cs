using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using System.Globalization;

namespace CrystalMix.Mobile;

[Activity(Label = "CrystalMix", MainLauncher = true, Exported = true,
    Theme = "@android:style/Theme.Material.Light.NoActionBar", WindowSoftInputMode = SoftInput.AdjustResize)]
public partial class MainActivity : Activity
{
    private MobileState state = new();
    private LinearLayout root = null!, content = null!;
    private readonly Color ink = Color.Rgb(22, 41, 66), accent = Color.Rgb(0, 113, 128);
    private string L(string en, string zh) => state.English ? en : zh;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Restore(); ShowPage(state.Page);
    }
    protected override void OnPause() { Save(); base.OnPause(); }
    private void ShowPage(int page)
    {
        state.Page = page;
        root = new LinearLayout(this) { Orientation = Orientation.Vertical };
        root.SetBackgroundColor(Color.Rgb(243, 246, 251));
        root.SetPadding(Dp(16), Dp(12), Dp(16), Dp(8));
        var header = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        var title = Text("CrystalMix", 25); title.SetTypeface(null, TypefaceStyle.Bold);
        header.AddView(title, new LinearLayout.LayoutParams(0, -2, 1));
        header.AddView(Button(state.English ? "中文" : "EN", () => { state.English = !state.English; ShowPage(page); Save(); }));
        root.AddView(header);
        root.AddView(Text(L("OFFLINE · CRYSTAL GROWTH", "离线使用 · 晶体配料"), 12));
        workspaceScroll = new ScrollView(this) { FillViewport = true };
        var scroll = workspaceScroll;
        content = Stack(); scroll.AddView(content);
        root.AddView(scroll, new LinearLayout.LayoutParams(-1, 0, 1));
        BuildActionBar(); SetContentView(root);
        state.Page = 0;
        ShowWorkspace();
    }
    private int Dp(float value) => (int)(value * Resources!.DisplayMetrics!.Density + .5f);
    private LinearLayout Stack() => new(this) { Orientation = Orientation.Vertical };
    private TextView Text(string value, float size = 16)
    {
        var text = new TextView(this) { Text = value, TextSize = size };
        text.SetTextColor(ink); text.SetPadding(0, Dp(8), 0, Dp(8)); return text;
    }
    private Button Button(string value, Action action)
    {
        var button = new Button(this) { Text = value }; button.SetAllCaps(false);
        button.SetMinHeight(Dp(48)); button.SetTextColor(accent);
        button.Click += (_, _) => action(); return button;
    }
    private EditText Input(LinearLayout parent, string label, string value, Action<string> changed, bool number = false)
    {
        parent.AddView(Text(label, 13));
        var edit = new EditText(this) { Text = value, TextSize = 16 };
        edit.SetSingleLine(true);
        edit.ContentDescription = label;
        edit.SetTextColor(ink);
        edit.InputType = number ? Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal | Android.Text.InputTypes.NumberFlagSigned
            : Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagNoSuggestions;
        edit.SetSelectAllOnFocus(true); edit.TextChanged += (_, _) => changed(edit.Text ?? "");
        parent.AddView(edit); return edit;
    }
    private static double Number(string value) => double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
}

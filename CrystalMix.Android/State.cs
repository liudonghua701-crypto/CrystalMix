using System.Text.Json;
using CrystalMix.Calculation;

namespace CrystalMix.Mobile;

public sealed class MaterialState
{
    public string Formula { get; set; } = "Fe";
    public string Parts { get; set; } = "1";
    public string Name { get; set; } = "";
}
public sealed class MobileState
{
    public bool English { get; set; } = !System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("zh");
    public int Page { get; set; }
    public string Total { get; set; } = "10";
    public string Target { get; set; } = "Ba1-xKxFe2As2";
    public string X { get; set; } = "0.4";
    public string Basis { get; set; } = "BaAs, Fe2As, KAs";
    public List<MaterialState> Materials { get; set; } = new() { new() { Formula = "Fe" }, new() { Formula = "Se" } };
}
public partial class MainActivity
{
    private void Restore()
    {
        try { state = JsonSerializer.Deserialize<MobileState>(GetPreferences(Android.Content.FileCreationMode.Private)!.GetString("recipe", "")!) ?? new(); }
        catch { state = new(); }
        state.Page = Math.Clamp(state.Page, 0, 3);
    }
    private void Save() => GetPreferences(Android.Content.FileCreationMode.Private)!.Edit()!.PutString("recipe", JsonSerializer.Serialize(state))!.Apply();
    private IReadOnlyList<RecipeResult> Results() => RecipeCalculator.Calculate(state.Materials.Select(m =>
        new RecipeInput(m.Name, m.Formula, Number(m.Parts))).ToArray(), Number(state.Total));
    private void Message(string text) => new Android.App.AlertDialog.Builder(this)!.SetTitle("CrystalMix")!.SetMessage(text)!
        .SetPositiveButton(L("OK", "确定"), (_, _) => { })!.Show();
}

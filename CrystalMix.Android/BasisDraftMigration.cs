using System.Text.Json;

namespace CrystalMix.Mobile;

public static class BasisDraftMigration
{
    public static string Restore(string json, string basis)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.TryGetProperty("BasisSchemaVersion", out _)) return basis;
        if (root.TryGetProperty("UseActualMaterials", out var enabled))
            return enabled.ValueKind == JsonValueKind.True ? basis : "";
        // 清除旧版自动预填内容，保留其他自定义原料及原有配料表。
        return basis == "BaAs, Fe2As, KAs" ? "" : basis;
    }
}

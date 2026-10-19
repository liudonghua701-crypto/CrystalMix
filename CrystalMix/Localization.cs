namespace CrystalMix;

internal enum UiLanguage { Chinese, English }

internal sealed class Localizer
{
    private readonly Dictionary<string, (string Zh, string En)> _strings = new()
    {
        ["appTitle"] = ("CrystalMix 晶体配料计算器", "CrystalMix Ratio Calculator"),
        ["tagline"] = ("从任意摩尔比快速换算原料质量", "Convert any molar ratio into precise raw-material masses"),
        ["language"] = ("界面语言", "Language"),
        ["add"] = ("＋ 添加原料", "+ Add material"),
        ["delete"] = ("删除选中", "Remove selected"),
        ["example"] = ("载入示例", "Load example"),
        ["clear"] = ("清空", "Clear"),
        ["help"] = ("使用说明", "Help"),
        ["name"] = ("原料名称（可选）", "Material name (optional)"),
        ["formula"] = ("化学式", "Formula"),
        ["ratio"] = ("摩尔份数", "Molar parts"),
        ["molarMass"] = ("摩尔质量 (g/mol)", "Molar mass (g/mol)"),
        ["fraction"] = ("质量占比", "Mass fraction"),
        ["mass"] = ("所需质量 (g)", "Required mass (g)"),
        ["rowStatus"] = ("状态", "Status"),
        ["totalMass"] = ("原料总质量", "Total batch mass"),
        ["grams"] = ("克 (g)", "grams (g)"),
        ["calculate"] = ("计算配比", "Calculate recipe"),
        ["copy"] = ("复制结果", "Copy results"),
        ["export"] = ("导出 CSV", "Export CSV"),
        ["ready"] = ("就绪。请输入化学式和摩尔份数。", "Ready. Enter formulas and molar parts."),
        ["calculated"] = ("计算完成：{0} 种原料，总质量 {1} g。", "Calculated {0} materials; total mass {1} g."),
        ["ok"] = ("有效", "Valid"),
        ["emptyFormula"] = ("请输入化学式", "Formula is required"),
        ["badRatio"] = ("摩尔份数必须为正数", "Molar parts must be a positive number"),
        ["badTotal"] = ("总质量必须为正数。", "Total batch mass must be a positive number."),
        ["noRows"] = ("请至少添加一种原料。", "Add at least one material."),
        ["fixRows"] = ("请修正标红的输入后重试。", "Correct the highlighted input and try again."),
        ["copied"] = ("结果已复制到剪贴板。", "Results copied to the clipboard."),
        ["nothingToExport"] = ("请先完成一次有效计算。", "Run a valid calculation first."),
        ["exported"] = ("CSV 已导出：{0}", "CSV exported: {0}"),
        ["csvFilter"] = ("CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*", "CSV files (*.csv)|*.csv|All files (*.*)|*.*"),
        ["confirmClear"] = ("确定要清空当前配方吗？", "Clear the current recipe?"),
        ["question"] = ("确认", "Confirm"),
        ["error"] = ("输入错误", "Input error"),
        ["helpTitle"] = ("CrystalMix 使用说明", "CrystalMix Help"),
        ["helpBody"] = (
            "1. 每行输入一种原料的化学式与摩尔份数。\n2. 输入原料总质量（克），点击“计算配比”。\n3. 质量占比按 n × M 归一化，所需质量之和等于总质量。\n\n支持：元素、化合物、嵌套括号、小数下标，以及用中点表示的结晶水，例如 Fe、FeSe、Ca(OH)2、Fe1.03Te、CuSO4·5H2O。\n\n提示：化学式区分大小写。放射性元素使用代表同位素质量数；高精度实验请按原料证书中的同位素组成与纯度修正。",
            "1. Enter one raw material formula and its molar parts per row.\n2. Enter the total batch mass in grams, then select Calculate recipe.\n3. Mass fractions are normalized from n × M, and required masses sum to the batch total.\n\nSupported: elements, compounds, nested brackets, decimal subscripts, and hydrate middle dots—for example Fe, FeSe, Ca(OH)2, Fe1.03Te, and CuSO4·5H2O.\n\nFormula symbols are case-sensitive. Radioactive elements use representative isotope mass numbers; for high-precision work, correct for the certified isotope composition and purity of your source material."),
        ["formula.empty"] = ("化学式不能为空", "Formula cannot be empty"),
        ["formula.unknownIsotope"] = ("尚未收录同位素：{0}。请在使用说明中查看已支持的核素。", "Isotope not yet included: {0}. See Help for supported nuclides."),
        ["formula.unknownElement"] = ("未知元素：{0}", "Unknown element: {0}"),
        ["formula.expectedElement"] = ("第 {0} 位应为元素符号或左括号", "Expected an element or opening bracket at position {0}"),
        ["formula.mismatchedBracket"] = ("第 {0} 位括号不匹配", "Mismatched bracket at position {0}"),
        ["formula.missingBracket"] = ("缺少右括号 {0}", "Missing closing bracket {0}"),
        ["formula.badNumber"] = ("化学计量数必须为正数", "Stoichiometric numbers must be positive"),
        ["formula.unexpected"] = ("第 {0} 位包含无法识别的字符", "Unexpected character at position {0}"),
    };

    public UiLanguage Language { get; set; } = UiLanguage.Chinese;

    public string this[string key]
    {
        get
        {
            var item = _strings[key];
            return Language == UiLanguage.Chinese ? item.Zh : item.En;
        }
    }
}

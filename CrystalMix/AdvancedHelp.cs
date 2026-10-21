using CrystalMix.Chemistry;
namespace CrystalMix;
internal static class AdvancedHelp
{
    public static string Get(UiLanguage language) => language == UiLanguage.English ? English : Chinese;
    private static string English => "\n\nISOTOPES\nUse a mass number followed by the element symbol inside square brackets: [57Fe], [57Fe]2O3, [2H]2O, [175Lu], [175Lu]2O3. Unsupported nuclides are rejected.\n"
        + $"The catalog contains {IsotopeCatalog.All.Count:N0} records. Open Isotopes to search, inspect half-lives and add a selected nuclide to your material table.\n"
        + "Isomers: [180mTa], [178m2Hf]. The default filter includes stable nuclides and half-lives of at least one year. Other catalog records remain available. Nuclear lifetime does not establish chemical stability.\n\n"
        + "TARGETS AND VARIABLES\nExample: Ba1-xKxFe2As2 with x = 0.4. For arithmetic subscripts, use braces: Ba{1-x}K{x}Fe2As2 or Fe{(1+x)/2}. Addition, subtraction, multiplication, division and parentheses are supported. x must be between 0 and 1; zero components are omitted.\n"
        + "Select BaFe2As2 and KFe2As2 as materials to obtain molar parts 0.6:0.4. Alternatively, BaAs, Fe2As and KAs balance the same target at 0.6:1:0.4.\n"
        + "Ba1-xKxFeAs cannot balance those three arsenides: matching Ba, K and Fe supplies 1.5 As per target unit instead of 1. Check the exact target formula.\n"
        + "Suggestions balance only your selected materials; they do not establish reaction feasibility or a unique synthesis route. Changing the target, x or material selection refills the table. Manual edits leave the target composition unchanged.\n\n"
        + "ACTUAL WEIGHING\nUse actual mass: choose one material and enter its measured mass in grams. Preview and apply to scale every weighing target by measured/planned mass, keeping molar parts unchanged. The batch total changes too. Other already-weighed materials are not locked; recheck them against the updated targets. Cancel leaves the recipe unchanged.\n\n"
        + "RESULTS\nCopy results or export a UTF-8 CSV after a valid calculation. Molar parts need not sum to one. Purity, losses and enrichment fractions are not corrected automatically.";
    private static string Chinese => "\n\n实际称量\n点击按实称质量调整，选择一种原料并输入实称质量（克）。预览确认后，以实称质量除以原计划质量的倍数调整整批，摩尔份数保持不变，总质量同步改变。其他已称原料不会锁定，请核对其是否满足新的应称质量。取消不会修改配料表。\n\n同位素： [57Fe], [57Fe]2O3, [2H]2O, [175Lu], [175Lu]2O3.\n"
        + "方括号内是质量数和元素符号；不支持的核素会报错。\n"
        + $"已收录 {IsotopeCatalog.All.Count} 个核素。点击同位素库搜索、查看寿命及加入原料。"
        + "\n核异构体： [180mTa], [178m2Hf]. 默认筛选稳定或半衰期 ≥ 1 年，其他已收录核素仍可输入。\n\n目标： Ba1-xKxFe2As2; x = 0.4.\n"
        + "复杂下标： Ba{1-x}K{x}Fe2As2; Fe{(1+x)/2}.\n"
        + "支持 + - * / 和括号，x 限 0–1，零含量会省略。\n"
        + "选用原料栏可填 BaFe2As2, KFe2As2，得到 0.6:0.4。\n"
        + "仅对用户选定的原料作计量配平，不判断反应是否可行。\n"
        + "修改目标、x 或选用原料后自动重新填写；手工编辑表格不会修改目标组成。\n同一目标也可选 BaAs、Fe2As、KAs，得到 0.6:1:0.4。若目标为 Ba1-xKxFeAs，这三种原料匹配其他元素时带入的砷为 1.5，而目标为 1，因此无法配平。\n核寿命不代表化学稳定性。\n\n结果\n有效计算后可复制结果或导出 CSV。摩尔份数之和不必为一；纯度、损失和富集比例不会自动修正。";
}

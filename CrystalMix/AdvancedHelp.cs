using CrystalMix.Chemistry;
namespace CrystalMix;
internal static class AdvancedHelp
{
    public static string Text => "\n\n同位素 / Isotopes: [57Fe], [57Fe]2O3, [2H]2O.\n"
        + "方括号内是质量数和元素符号；不支持的核素会报错。\nUse mass number then symbol; unsupported nuclides are rejected.\n"
        + "已收录 / Supported: " + AtomicSpecies.SupportedIsotopes
        + "\n\n目标 / Target: Ba1-xKxFe2As2; x = 0.4.\n"
        + "复杂下标 / Expressions: Ba{1-x}K{x}Fe2As2; Fe{(1+x)/2}.\n"
        + "支持 + - * / 和括号，x 限 0–1，零含量会省略。\nSupports arithmetic and parentheses; x is 0–1, zero components are omitted.\n"
        + "选用原料栏可填 BaFe2As2, KFe2As2，得到 0.6:0.4。\nMaterials may be BaFe2As2, KFe2As2, yielding 0.6:0.4.\n"
        + "仅对用户选定的原料作计量配平，不判断反应是否可行。\nBalance applies only to your selected materials, not reaction feasibility.\n"
        + "修改目标、x 或选用原料后自动重新填写；手工编辑表格不会修改目标组成。\nChanging target, x or materials refills the table; manual table edits leave the target unchanged.";
}

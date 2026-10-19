# CrystalMix 晶体配料计算器

CrystalMix 是一个面向晶体生长实验的 Windows 桌面应用。它把任意原料摩尔比换算为质量比，并按照给定的原料总质量计算每种原料的称量质量。界面支持简体中文与英语即时切换。

CrystalMix is a bilingual Windows desktop application for converting arbitrary molar ratios into raw-material mass fractions and weighing masses for a specified total batch mass.

## 主要功能 / Features

- 任意数量的元素单质或化合物，以及任意正数摩尔份数
- 自动计算摩尔质量、质量占比和每种原料所需质量
- 支持括号、嵌套括号、小数化学计量数和结晶水
- 表格内错误定位、结果复制和 UTF-8 CSV 导出
- 中文/英语界面切换
- x64 自包含发布，目标电脑无需预装 .NET
- 单文件安装程序、桌面与开始菜单快捷方式、标准卸载入口

## 使用方法 / Usage

1. 每行输入一种原料的名称（可选）、化学式和摩尔份数。
2. 输入原料总质量，单位为克。
3. 点击“计算配比”。质量占比按 `摩尔份数 × 摩尔质量` 归一化。
4. 可复制结果，或导出 CSV 用于实验记录。

Enter one material per row, provide its formula and molar parts, set the total mass in grams, and select **Calculate recipe**. CrystalMix normalizes each `molar parts × molar mass` contribution.

支持的例子：`Fe`、`FeSe`、`Ca(OH)2`、`K4[Fe(CN)6]`、`Fe1.03Te`、`CuSO4·5H2O`。元素符号区分大小写；结晶水请使用中点 `·`，句点 `.` 用作小数点。

Supported examples: `Fe`, `FeSe`, `Ca(OH)2`, `K4[Fe(CN)6]`, `Fe1.03Te`, and `CuSO4·5H2O`. Element symbols are case-sensitive. Use a middle dot `·` for hydrates; the period `.` is reserved as a decimal point.

## 安装与分发 / Install and distribute

直接运行 `dist\CrystalMix-Setup-x64.exe`。安装仅作用于当前 Windows 用户，无需管理员权限。安装程序会创建快捷方式，并在 Windows“已安装的应用”中注册卸载入口。

Run `dist\CrystalMix-Setup-x64.exe`. Installation is per-user and does not require administrator privileges.

## 从源码构建 / Build from source

需要 Windows 和 .NET 8 SDK。在 PowerShell 中运行：

```powershell
.\build-release.ps1
```

测试项目不依赖第三方测试框架：

```powershell
dotnet run --project .\CrystalMix.Tests\CrystalMix.Tests.csproj -c Release --configfile .\NuGet.Config
```

## 科学说明 / Scientific note

原子量采用常规标准原子量；放射性元素使用周期表常用的代表同位素质量数。软件计算的是理论配比，不会自动修正原料纯度、氧化、吸湿、挥发损失或同位素富集。高精度实验应根据原料证书和具体生长方案修正。

Atomic weights are conventional standard values; radioactive elements use representative isotope mass numbers. Results are theoretical and do not automatically correct for purity, oxidation, moisture, volatilization, or isotope enrichment.

## License

MIT License. See [LICENSE](LICENSE).

## 目标组成、同位素与掺杂 / Targets, isotopes and doping

- 输入元素符号后显示中英文名称；化合物单元格的提示列出所含元素。
- 同位素采用 `[质量数元素符号]`，例如 `[57Fe]`、`[57Fe]2O3`、`[2H]2O`。普通 Fe 与指定的 `[57Fe]` 在组成中分别保留。
- 当前核素库收录 H-1/2/3、B-10/11、C-12/13/14、O-16/17/18、Fe-54/56/57/58、Lu-175/176。未收录核素明确报错，不用整数质量数估算。相对质量来源：[NIST](https://physics.nist.gov/PhysRefData/Compositions/index.html)。
- 目标栏支持 `Ba1-xKxFe2As2`，x 在 0–1 之间。复杂下标使用花括号，如 `Ba{1-x}K{x}Fe2As2`；支持加减乘除与括号，例如 `Fe{(1+x)/2}`，乘法须写 `*`。
- 目标、x 或选用原料改变后，延迟约 0.65 秒自动求值并重新填写表格；非法目标不会覆盖原料表。表格可自由编辑，修改表格不会反向修改目标。
- “选用原料”留空时，建议按元素或核素组成填写。也可输入逗号分隔的母体化合物，例如 `BaFe2As2, KFe2As2`，x=0.4 时建议摩尔比为 0.6:0.4。
- 配平只适用于用户选定的独立原料组合。无法配平、有负用量或不唯一的组合会报错。它不判断反应、挥发、助熔剂、副产物和合成路线的可行性。

Use `[57Fe]` for a specified isotope and braces for arithmetic subscripts. The target composition is separate from the editable material table. A blank material basis suggests elemental amounts; a supplied precursor basis is balanced only when it has an independent, nonnegative solution. These are stoichiometric suggestions, not unique synthesis recipes. Changing target inputs refills the table.

## 验证与编码 / Validation and encoding

`dotnet run --project CrystalMix.Tests -c Release -- --ui` 还会验证自动填写与中英文布局，并在 `dist/ui-checks` 保存字体缩放 100%、150%、200% 的截图。字体缩放模拟不等同于所有物理显示器 DPI 组合。

源代码和说明使用 UTF-8；Windows PowerShell 脚本使用 UTF-8 BOM；IExpress 配置与 CMD 启动器保持 ASCII。中文安装和卸载弹窗由 PowerShell 的 Unicode 字符串显示，不依赖系统 ANSI 代码页。

Git 仓库位于当前 E 盘项目的 `.git`；构建缓存、发布产物和截图不提交。首次提交保存开发前的代码基线。

发布后可运行 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\smoke-release.ps1`。此检查会解包并校验全部文件、启动与正常关闭应用、验证安装确认框中文并选择“否”；不会实际安装或更改现有安装。解包目录保留在 `dist/smoke-*` 供核对。

## 安装路径和卸载修复 / Installation folder and uninstall

安装确认后会显示“安装位置”窗口，可直接输入完整路径或点击“浏览”。支持中文和空格路径；请选择专用文件夹，不支持磁盘根目录、安装源目录或目录链接。已有安装默认使用原目录；如需换目录，请先卸载旧版本，再运行安装包。

The installer lets you enter or browse for a dedicated installation folder. Existing installations default to their current folder. Uninstall the old location before moving to another folder.

卸载入口直接启动 PowerShell，不再依赖 CMD 延迟删除。卸载会校验注册目录，仅关闭该目录内的 CrystalMix 进程，只删除程序文件，保留用户文件；遇到文件占用会报错并保留入口以便重试。不要手动移动已安装目录。

维护人员可从已解包的负载运行 `Install.ps1 -InstallDir "E:\Apps\CrystalMix" -Unattended -NoLaunch`。安装器回归测试为 `test-installer.ps1`，会在项目内进行真实安装和卸载，并在 finally 中恢复原有注册信息与快捷方式；`test-install-dialog.ps1` 验证路径输入界面，`test-uninstall-entry.ps1` 验证当前卸载入口并取消，不删除现有安装。

独立 Git for Windows 已安装；仓库及全部提交仍位于 E 盘当前项目的 `.git` 中。可使用 `git status` 和 `git log --oneline` 查看状态与历史。

路径选择修复：点击“浏览”选择 D 盘等磁盘根目录时，会自动填写专用子目录（例如 `D:\CrystalMix`）；取消浏览保留原路径。已修复 PowerShell 自动变量 `$input` 与文本框变量冲突造成的 `Text` 属性错误。`test-install-browse.ps1` 覆盖真实浏览按钮的确认与取消；可运行 `test-installer.ps1 -TestBase 'D:\CrystalMix-Validation'` 复测 D 盘实际安装和卸载。

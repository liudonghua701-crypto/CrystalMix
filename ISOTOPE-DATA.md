# 核素数据说明 / Isotope data

数据快照日期：2026-10-19。共 3,498 条记录：3,486 条基态质量记录及 12 条长寿命核异构体记录，覆盖现有 118 元素符号。覆盖元素不代表每种元素都有稳定核素；这也不是所有激发态和未来新测核素的全集。

## 默认筛选与输入

默认显示来源标记为 STABLE 的核素，以及半衰期至少 1 个儒略年（31,557,600 秒）的核素。上限值 LT/LE、未知寿命及带 # 的推测寿命不进入默认筛选；下限与近似值保留来源标记。稳定标记按本次来源快照解释，不代表绝对不会衰变。全部视图显示所有已收录记录，含短寿命及未知寿命记录。

使用 `[175Lu]`、`[175Lu]2O3` 指定同位素；`[180mTa]` 和 `[178m2Hf]` 指定核异构体，m 表示第一异构态，m2 表示第二异构态。普通元素仍使用原有标准原子量；指定核素使用其原子质量，两者在组成中分别保留。核寿命与化学稳定性不是同一概念，程序不根据寿命判断化合物是否存在或合成路线是否可行。

## 来源与转换

- [NIST Atomic Weights and Isotopic Compositions](https://physics.nist.gov/PhysRefData/Compositions/index.html)：3,352 条质量记录。优先保留这些数值，兼容原有计算结果；不声称它们全部是最新评价值。
- [IAEA LiveChart API](https://iaea-nds.github.io/lc_api_notebook/)：补充 134 条 AME2020 质量记录，并提供 ENSDF 基态寿命信息。atomic_mass 单位为微原子质量单位，除以 1,000,000 后使用。
- [NUBASE2020](https://www-nds.iaea.org/amdc/ame2020/nubase_3.mas20.txt)：补充 12 个寿命至少一年的核异构体。原子质量由 A + 质量盈余 / 931494.10242 得到，质量盈余单位 keV，转换常数采用 CODATA2018。短寿命核异构体不在本次导入范围。

Ta-180 基态寿命约 8 小时，与长寿命 Ta-180m 分开。NUBASE 的 Ta-180m 下限信息被保存为下限，未改写为稳定标记。质量来源的 # 或系统学估算标记保存在 Estimated 字段，并在界面显示“估算 / est.”。程序保存中心值用于称量计算，不传播数据评价不确定度；原始快照保留来源原文。

## 离线复现

`data-sources/isotope-sources.zip` 保存三个原始快照，`data-sources/manifest.json` 记录各自 URL、下载日期和 SHA-256。运行 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/import-isotopes.ps1`，会校验来源哈希后生成嵌入式 JSON；缺少默认缓存目录时自动从快照解包。固定使用 Windows PowerShell 5.1 序列化，保证生成结果可重复。

数据直接嵌入应用，运行时无需联网。导入复用现有 PeriodicTable 的元素符号，未重新生成或改写 118 元素标准原子量表。更新来源时须同时审核清单、导入结果与快照计数测试，不能静默混入来源不明的数据。

English: The default catalog includes source-labeled stable nuclides and half-lives of at least one year. All available ground-state records and twelve long-lived isomers are included in the embedded snapshot. Nuclear lifetime does not establish chemical stability. Source labels, bounds and estimated-mass flags remain visible. Mass uncertainty is not propagated.

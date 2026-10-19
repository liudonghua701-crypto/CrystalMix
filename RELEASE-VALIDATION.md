# CrystalMix 发布验证记录

本文保存首轮发布的历史验证结果；当前安装包及卸载修复见 [安装器验证记录](INSTALLER-VALIDATION.md)。

日期：2026-10-19。

## 已完成

- 保留初始源代码基线，并在 E 盘项目内建立 Git 仓库。
- 118 种元素的中英文名称识别；保留原有原子量表。
- 普通元素与指定同位素统一计算，内置 15 种核素的 NIST 质量；未收录核素明确拒绝。
- 目标组成自动解析、变量 x 求值、元素原料建议和用户指定母体化合物的配平。
- 目标组成与实际原料分开；用户仍可编辑原料名称、化学式和摩尔份数。
- 表头换行、自动高度、最小列宽和小窗口滚动；修复中文安装弹窗编码。

## 测试结果

- 原有基线：11/11 通过。
- 最终核心计算与回归：30/30 通过。
- 含真实 WinForms 界面流程：31/31 通过。
- 界面覆盖中英文、字体缩放 100%/150%/200%、最小窗口、自动填写、手工编辑、自定义名称保护、无效目标保留原料表及溢出错误提示。
- 截图位于 dist/ui-checks；已目视检查正常窗口、小窗口和 200% 字体窗口。未在多个物理显示器 DPI 组合上测试。
- 发布脚本的测试、PowerShell 语法、UTF-8 BOM 和 ASCII 配置检查通过。
- Windows x64 自包含单文件已发布；PE Machine = 0x8664。
- 安装包提取的 5 个文件与发布负载逐一 SHA-256 一致。
- 从安装包解出的应用成功启动、窗口响应并正常关闭，退出码 0。
- 真实安装确认框包含正确中文“是否安装 CrystalMix？”，取消后正常退出。
- 本次未进行实际安装与卸载；未更改现有用户安装。

## 安装包

路径：dist/CrystalMix-Setup-x64.exe（根目录另有相同副本）。

大小：65,204,224 字节（62.18 MiB）。

SHA-256：B95FB8C85347BDADAFDD323C1C09D521523B31E5B262DAC000A53DEE15D69FE5

## 复现

- 核心与界面：dotnet run --project CrystalMix.Tests -c Release -- --ui
- 自包含发布与打包：powershell.exe -NoProfile -ExecutionPolicy Bypass -File build-release.ps1
- 安装包与启动检查：powershell.exe -NoProfile -ExecutionPolicy Bypass -File smoke-release.ps1

Git 历史保存在当前 E 盘项目的 .git 中。因执行环境账户与目录所有者不同，已为当前用户添加此项目的精确 safe.directory 配置；配置不包含仓库历史。

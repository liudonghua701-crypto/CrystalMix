# 安装路径与卸载修复验证

日期：2026-10-19。

## 本次变更

- 已安装独立 Git for Windows 2.55.0.windows.5，位置为 C:\Program Files\Git；系统 PATH 已配置。
- 仓库与历史继续保存在 E:\Thomas Ph.D\element ratio calculator\.git。
- 安装时新增路径输入和浏览窗口，支持中文、空格及其他合法路径字符。
- 验证专用目录，拒绝相对路径、磁盘根目录、安装源目录和目录链接。
- 已安装版本默认使用原目录；更换目录前先卸载旧版本，避免遗留多份安装。
- 卸载入口直接调用 PowerShell；按注册目录校验和按进程路径停止应用。
- 文件删除失败显示错误，保留可重试入口；不递归删除用户文件。

## 现有旧版本修复

位置：C:\Users\86133\AppData\Local\Programs\CrystalMix。

已更新卸载脚本和 Windows 卸载入口，原应用文件的 SHA-256 保持不变。已实际从注册入口打开正确的中文确认框，再取消；现有安装保留，可由用户在 Windows 已安装应用中卸载。

修复前备份：.tools\repair-backup-4f9be1a925154c5eb96a0e17f14deeae。

## 自动测试

- 30/30 计算回归测试通过。
- test-installer.ps1：16 项检查通过，含真实自定义中文路径安装/卸载、快捷方式、注册入口、错误目录拒绝、文件占用后可重试、保留用户文件及删除空目录。
- 测试临时修改的旧安装记录和快捷方式已恢复；未卸载用户正在保留的旧版本。
- test-install-dialog.ps1：真实路径选择窗口返回中文路径，截图见 dist/install-path-dialog.png。
- test-uninstall-entry.ps1：修复后的旧版本卸载入口正常打开并取消。
- 所有安装脚本语法与 UTF-8 BOM/ASCII 编码检查通过。

## 最终安装包

路径：dist/CrystalMix-Setup-x64.exe，根目录有相同副本。

大小：65204224 字节。

SHA-256：8B4A3B0630186C384EA91052B1CA0BF23AFA984DC2E7B5293EE811D9CDD2B963

最终安装包烟雾测试通过：6 个解包文件与发布负载哈希一致；自包含 x64 应用启动、响应和正常关闭；中文安装确认框显示正确并可取消。

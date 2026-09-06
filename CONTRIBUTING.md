# Contributing to ProjectGenesis | 贡献指南

## 许可 | License
- 代码与游戏数据（src/、data/、packer/、preloader/、devtools/）的贡献：默认视为同意按 **GPL-3.0-only** 授权（根目录 LICENSE）。
- 美术资产（assets/）的贡献：默认视为同意按 **assets/LICENSE.assets** 的条款执行——本项目资产保留所有权利；涉及《戴森球计划》官方素材或其衍生的部分版权归官方（Gamera Games / 柚子猫工作室）；第三方素材权利归其原始作者。
- 如不希望按以上条款授权，请勿提交对应内容。

## 分支与提交 | Branches & Commits
- 功能开发在 `3.x` 系列分支上进行，`main` 为发布主线。
- 提交信息参考现有风格：中文、一段话概括改动（含定位或修复要点）。
- 请勿提交以下内容（已在 .gitignore）：`lib/Assembly-CSharp-publicized.dll`、`DefaultPath.props`、`bin/`、`obj/` 等构建产物。

## 变更检查清单 | PR Checklist
- [ ] 修改数值/配方/物品/科技时，同步更新 `data/*.json` 与 `data/strings.json`（含英文），并核对 sprite 图标引用名的对应关系。
- [ ] 新增物品/建筑时，检查 ProtoID、BuildIndex 冲突（参考 commit 历史中的类似修复）。
- [ ] 运行时勿依赖未复用的公共字段/私有成员，保持与 publicized Assembly-CSharp 的兼容；如遇原版方法签名变化，先验证再提交。
- [ ] 更新 `CHANGELOG.md`。
- [ ] 涉及玩家体验变化的，在 README 的"Mod内容速览/Modified"处同步描述。

## 反馈与讨论 | Feedback
- QQ 群与 Discord 链接见 README；Bug 请附游戏版本、Mod 版本与日志。

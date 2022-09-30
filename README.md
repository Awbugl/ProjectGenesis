# GenesisBook | 创世之书

一个修改了原版大部分物品、配方和科技的mod.

A mod that modifies most of the items, recipes and technologies of the vanilla game.

----

## A glimpse | 一瞥

![img.png](https://webapi.awbugl.top/dsp-genesisbook/1.jpg)
![img.png](https://webapi.awbugl.top/dsp-genesisbook/2.jpg)
![img.png](https://webapi.awbugl.top/dsp-genesisbook/3.jpg)

## Installation | 安装方法

### With Mod Manager | 使用Mod管理器

只需要简单的打开mod管理器（如果你还没安装可以[点此安装](https://dsp.thunderstore.io/package/ebkr/r2modman/)，选择 **GenesisBook**，下载即可）。

如果弹窗提示需要下载前置，点击确定即可。


Simply open the mod manager (if you don't have it install it [here](https://dsp.thunderstore.io/package/ebkr/r2modman/))
, select **GenesisBook**, then **Download**.

If prompted to download with dependencies, select `Yes`.

Then just click **Start modded**, and the game will run with the mod installed.

### Install manually | 手动安装

在下述地址安装框架和几个前置mod，然后将本mod解压到`BepInEx/plugins/`目录（包括 `GenesisBook-Release-${version}.dll`, `Newtonsoft.Json.dll` 和 `texpack` 文件）。如果开始游戏后能看到新的logo，那么mod就安装成功了。


Install BepInEx from [here](https://dsp.thunderstore.io/package/xiaoye97/BepInEx/)<br/>
Install LDBTool from [here](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)<br/>
Install DSPModSave from [here](https://dsp.thunderstore.io/package/CommonAPI/DSPModSave/)<br/>
Install CommonAPI from [here](https://dsp.thunderstore.io/package/CommonAPI/CommonAPI/)<br/>
Install NebulaMultiplayerModApi from [here](https://dsp.thunderstore.io/package/nebula/NebulaMultiplayerModApi/)<br/>

Then download the mod manully and unzip into `BepInEx/plugins/` (including the `GenesisBook-Release-${version}.dll`, `Newtonsoft.Json.dll` and `texpack` file).

If you can see the new **GenesisBook** logo, then the mod is installed successfully.

## Feedback and suggestions | 意见和反馈

如果您对本Mod有宝贵的意见或建议，请在[github](https://github.com/Awbugl/ProjectGenesis)发issue，或者加QQ群`991895539`向管理员反馈。

If you have any feedback or suggestions, please open a issue via [github](https://github.com/Awbugl/ProjectGenesis), or contact me in Discord `HiddenCirno#4535`.

# Intro | 介绍

> 如果你玩过 Minecraft 的 GregTech mod, 可能会看到一些熟悉的东西.
>
> If you've played Minecraft's GregTech mod, you may have seen something familiar.

## Modified | 修改

- 科技树重做：
  + 对原版科技树进行了大量修改，添加了大量新科技。
  + 化学能源线
  + 丰富的化工线
  + 核子能源线
  + 石油精炼线
  + 矿物处理线
  + 信息科技线
  + 终极目标线 (*非必须)


- 配方类型细化：
  + 添加 电路蚀刻、高精度加工、矿物处理、精密组装、高分子化工 等配方类型以及对应的机器。


- 物品与配方修改：
  + 对原版物品进行了大量修改，添加了大量新物品及对应配方。
  + 钨 —— 高级机器的原料
  + 齿轮、线圈升级 —— 各类物品
  + 基础机械组件、先进机械组件、尖端机械组件 —— 各类建筑
  + 电路基板、硅晶圆、IC晶片 —— 处理器
  + 聚乙烯、环氧树脂、聚苯并咪唑 —— 化工产物
  + 煤焦油、柴油、偏二甲肼 —— 石化能源


- 机器修改：
  + 大部分机器的制造速度修改为2x。
  + 巨型建筑 —— 使用行星运输站模型, 使用传送带运输物料, 以及40x的生产效率



- Technology tree rework.
    + Extensive changes to the original tech tree, adding a large number of new technologies.
    + Chemical energy line
    + Rich chemical line
    + Nuclear energy line
    + Oil refining line
    + Mineral processing line
    + Information Technology Line
    + Ultimate Goal Line (*Not required)


- Recipe type refinement.
    + Add recipe types such as Circuit Etching, High Precision Processing, Mineral Processing, Precision Assembly,
      Polymer Chemical, etc. and corresponding machines.


- Item and recipe modifications.
    + Extensive modifications to the original items, adding a large number of new items and corresponding recipes.
    + Tungsten - raw material for advanced machines
    + Gear and coil upgrades - various items
    + Basic mechanical components, advanced mechanical components, cutting-edge mechanical components -- various types of buildings
    + Circuit substrates, silicon wafers, IC wafers -- processors
    + Polyethylene, epoxy resins, polybenzimidazole - chemical products
    + Coal tar, diesel fuel, methyldimethylhydrazine -- petrochemical energy


- Machine modifications.
    + Manufacturing speed modified to 2x for most machines.
    + Giant buildings -- use planetary transport station model, use conveyor belts to transport materials, and 40x
      productivity


# Credits | 致谢

- 特别感谢ckcz对本项目做出的贡献以及1.0版本的开发支持.
- 特别感谢Awbugl为项目优化和重构做出的工作以及2.0版本的开发支持.
- 特别感谢kremnev8在开发过程中提供的帮助.
- 特别感谢所有为本项目做出贡献和提出建议的人.


- Special thanks to ckcz for his contribution to this project and development support for version 1.0. Without him this mod will not be released.
- Special thanks to Awbugl for his work on the optimization and reconstruction of the project and development support for version 2.0.
- Special thanks to kremnev8 for his help during the development process. He developed tools such as CommonAPI and LDBTool which are very important.
- Special thanks to all those who contributed to this project or made suggestions.


# Update Log | 更新日志

- v2.1.0
  + 修复物流运输界面部分建筑、物品位置冲突的问题
  + 添加新配方：氯苯水解：氯苯 + 水 -> 苯酚 + 氢氯酸
  + 调整配方：石油裂化现在产出**1**氯苯(而不是**3**)
  + 调整配方：有机液体离心现在产出1**硫酸**(而不是**硝酸**)
  + 特性：戈壁星球添加硝酸海洋；海洋丛林海洋修改为盐水海洋

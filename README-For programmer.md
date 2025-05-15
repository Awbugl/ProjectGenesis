![img.png](https://assets.awbugl.top/dsp-genesisbook/icon.png)

构建真实宇宙，撰写创世之书。

Construct Real Universe. Then leave a GenesisBook.

## 意见和反馈 | Feedback and suggestions

如果您对本Mod有宝贵的意见或建议，
请加QQ群（如下）向管理员反馈，欢迎加群交流。

- 创世之书MOD一号节点 991895539
- 群问题答案：戴森球计划

If you have any problems, please contact us in [our Discord server](https://discord.gg/QwMEeaRZZR),
or open a issue via [github](https://github.com/Awbugl/ProjectGenesis).

## 如何开始

1. 在开始之前，请务必确认您对以下内容以及它们的用法都有一定了解：

* [C#语言](https://www.runoob.com/csharp/csharp-tutorial.html)
* [Harmony](https://harmony.pardeike.net/articles/intro.html)，修改游戏运行逻辑
* [BepInEx](https://thunderstore.io/c/dyson-sphere-program/p/xiaoye97/BepInEx/)
  ，5.4.18无法正确处理Harmony的部分Transpiler，由于创世大量使用Transpiler，强烈建议使用5.4.17
* [LDBTool](https://thunderstore.io/c/dyson-sphere-program/p/xiaoye97/LDBTool/)，向戴森球计划添加物品、配方、科技等
* [CommonAPI](https://thunderstore.io/c/dyson-sphere-program/p/CommonAPI/CommonAPI/)，向戴森球计划添加物品、配方、科技等

2. 在本项目下输入如下指令：`git update-index --assume-unchanged DefaultPath.props`，以屏蔽您本地的路径改动。
3. 打开本项目，修改`DefaultPath.props`中的BepinExPath和DSPAssemblyPath，使其与您的实际路径一致，然后关闭/卸载项目。
4. 在R2中打开与上一步一致的Profile，在ConfigEditor中搜索`BepInEx.cfg`，修改Preloader-DumpAssemblies为true。
5. 通过R2启动一次游戏（需要启用创世之书），将会自动生成`BepInEx\DumpedAssemblies\DSPGAME\Assembly-CSharp.dll`。
6. 重新打开本项目，会自动在项目内生成`lib\Assembly-CSharp-publicized.dll`。至此，准备工作已经完成。现在项目不应有任何报错。

## How to start

1. Before you begin, make sure you have some understanding of the following and how they are used:

* [C# Language](https://www.runoob.com/csharp/csharp-tutorial.html)
* [Harmony](https://harmony.pardeike.net/articles/intro.html), Modify the game's runtime logic
* [BepInEx](https://thunderstore.io/c/dyson-sphere-program/p/xiaoye97/BepInEx/), 5.4.18 can't handle some of Harmony's
  Transpiler correctly, so 5.4.17 is highly recommended due to lots of Transpilers in this project.
* [LDBTool](https://thunderstore.io/c/dyson-sphere-program/p/xiaoye97/LDBTool/), Adds items, recipes, technologies, etc.
  to the Dyson Sphere Program.
* [CommonAPI](https://thunderstore.io/c/dyson-sphere-program/p/CommonAPI/CommonAPI/), Add items, recipes, technologies,
  etc. to the Dyson Sphere Program.

2. Enter the following command under this project: `git update-index --assume-unchanged DefaultPath.props` to mask your local path changes.
3. Open this project, modify the BepinExPath and DSPAssemblyPath in `DefaultPath.props` to match your actual paths, then
   close/uninstall the project.
4. Open a Profile in R2 that matches the previous step, search for `BepInEx.cfg` in the ConfigEditor, and change
   Preloader-DumpAssemblies to true.
5. Launch the game once through R2 (need to enable Genesis Book), it will automatically generate
   `BepInEx\DumpedAssemblies\DSPGAME\Assembly-CSharp.dll`.
6. Re-open the project, it will automatically generate `lib\Assembly-CSharp-publicized.dll` in the project. So, the
   preparation work has been completed. The project should not report any errors now.

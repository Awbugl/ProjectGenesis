using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches
{
    public static class UITutorialWindowPatches
    {
        [HarmonyPatch(typeof(UITutorialWindow), nameof(UITutorialWindow.OnTutorialChange))] 
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UITutorialWindow_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);
            
            /*
                string layoutStr = UILayoutParserManager.GetLayoutStr(UITutorialWindow.textFolder, this.tutorialProto.LayoutFileName);

                Ldarg_0
                Ldfld tutorialProto
                Call IsGenesisBookLayout
                Brfalse_S originalLogicLabel
                
                Ldarg_0
                Ldfld tutorialProto
                Call GetGenesisBookLayoutStr
                Br_S endLabel

                IL_0027: ldsfld       string UITutorialWindow::textFolder // originalLogicLabel
                IL_002c: ldarg.0      // this
                IL_002d: ldfld        class TutorialProto UITutorialWindow::tutorialProto
                IL_0032: ldfld        string TutorialProto::LayoutFileName
                IL_0037: call         string UILayoutParserManager::GetLayoutStr(string, string)
                IL_003c: stloc.0      // layoutStr // endLabel

             */
            
            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_0));
            matcher.CreateLabelAt(matcher.Pos, out var endLabel);

            matcher.MatchBack(false, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(UITutorialWindow), nameof(UITutorialWindow.textFolder))));
            matcher.CreateLabelAt(matcher.Pos, out var originalLogicLabel);
            
            // 插入预加载和判断
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UITutorialWindow), nameof(UITutorialWindow.tutorialProto))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UITutorialWindowPatches), nameof(IsGenesisBookLayout))),
                new CodeInstruction(OpCodes.Brfalse_S, originalLogicLabel)
            );

            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UITutorialWindow), nameof(UITutorialWindow.tutorialProto))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UITutorialWindowPatches), nameof(GetGenesisBookLayoutStr))),
                new CodeInstruction(OpCodes.Br_S, endLabel)
            );

            return matcher.InstructionEnumeration();
        }

        public static bool IsGenesisBookLayout(TutorialProto proto)
        {
            var layoutFileName = proto.LayoutFileName;
            
            return !string.IsNullOrEmpty(layoutFileName) && layoutFileName.StartsWith("genesisbook-tutorials-");
        }

        public static string GetGenesisBookLayoutStr(TutorialProto proto)
        {
            const string preText =
                "{$Text|fontsize=16;linespacing=1.1;textalignment=0,1;color=#FFFFFF52;material=UI/Materials/widget-text-alpha-5x-thick;margins=20,20,20,30}\n";

            const string postText =
                "{$Text|fontsize=14;linespacing=1.1;textalignment=0,1;color=#FFFFFF52;material=UI/Materials/widget-text-alpha-5x-thick;margins=20,20,20,20}\n";

            string protoName = proto.Name;

            if (!protoName.EndsWith("标题")) return string.Empty;

            var text = protoName.Replace("标题", "前字");
            return $"{preText}{protoName.TranslateFromJson()}{postText}{text.TranslateFromJson()}";
        }
    }
}
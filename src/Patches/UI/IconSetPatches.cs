using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class IconSetPatches
    {
        /// <summary>
        /// 屏蔽所有ID>2000的科技图标
        /// </summary>
        [HarmonyPatch(typeof(IconSet), nameof(IconSet.Create))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IconSet_Create_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // 定位跳转目标 (IL_05f8: brfalse      IL_06ff)
            // 注意这个能匹配到两个地方，需要的刚好是第一个地方
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(TechProto), nameof(TechProto.iconSprite)))
            );
            // 抓取 brfalse 指向的 Label
            var targetLabel = matcher.Clone().Advance(5).Operand;
            // 拿到存储的 techProto 局部变量
            var techProtoLocal = matcher.Clone().Advance(-1).Operand;
            // 在IL_05f8之后插入：if (IsInvalidTech(techProto)) goto targetLabel;
            matcher.Advance(6).InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc, techProtoLocal), // 将 techProto 压栈
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IconSetPatches), nameof(IsInvalidTech))), // 调用静态方法
                new CodeInstruction(OpCodes.Brtrue, targetLabel) // 如果返回值为 true (ID > 2000)，则跳转
            );

            return matcher.InstructionEnumeration();
        }
        
        // 返回 true 表示需要跳转（即 ID > 2000）
        public static bool IsInvalidTech(TechProto proto)
        {
            if (proto == null) return false;
            return proto.ID > 2000;
        }
    }
}

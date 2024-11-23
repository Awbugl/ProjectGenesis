using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.UI.DisplayText
{
    /// <summary>
    ///     special thanks for
    ///     https://github.com/hubastard/nebula/tree/master/NebulaPatcher/Patches/Transpilers/UIVersionText_Transpiler.cs
    /// </summary>
    public static class DisplayNamePatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(UIVersionText), nameof(UIVersionText.Refresh))]
        public static IEnumerable<CodeInstruction> Refresh_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, generator).MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "userName"));

            if (codeMatcher.IsInvalid)

                // For XGP version
                codeMatcher.Start().MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_usernameAndSuffix"));

            return codeMatcher.Advance(1).InsertAndAdvance(Transpilers.EmitDelegate<Func<string, string>>(text =>
                    $"{ProjectGenesis.MODNAME.TranslateFromJson()} {ProjectGenesis.VERSION}{ProjectGenesis.DEBUGVERSION}\r\n{text}"))
               .InstructionEnumeration();
        }
    }
}

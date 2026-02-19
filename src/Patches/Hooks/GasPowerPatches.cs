using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class GasPowerPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlayerAction_Mine), nameof(PlayerAction_Mine.GameTick))]
        public static IEnumerable<CodeInstruction> PlayerAction_Mine_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerAction_Mine), nameof(PlayerAction_Mine.autoExtractGas))));

            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(GasPowerPatches), nameof(GasPowerGen_Patch)));

            matcher.SetOpcodeAndAdvance(OpCodes.Br);

            return matcher.InstructionEnumeration();
        }

        public static void GasPowerGen_Patch(PlayerAction_Mine playerAction)
        {
            if (!playerAction.autoExtractGas) return;

            playerAction.extractGasProgress[0] = 1;

            Mecha mecha = playerAction.player.mecha;

            double change = mecha.corePowerGen / 4;

            mecha.coreEnergy += change;

            if (mecha.coreEnergy > mecha.coreEnergyCap) mecha.coreEnergy = mecha.coreEnergyCap;

            mecha.MarkEnergyChange(2, change);
        }

        [HarmonyPatch(typeof(UIFunctionPanel), nameof(UIFunctionPanel._OnCreate))]
        [HarmonyPostfix]
        public static void UIFunctionPanel_OnCreate(UIFunctionPanel __instance)
        {
            Transform child = __instance.extractButton.transform.GetChild(3);

            Object.DestroyImmediate(child.GetComponent<Localizer>());
            Text component = child.GetComponent<Text>();
            component.text = "磁流体发电".TranslateFromJson();
        }
    }
}

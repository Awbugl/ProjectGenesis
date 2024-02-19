using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable Unity.UnknownResource

namespace ProjectGenesis.Patches.UI
{
    public static class UIPowerGeneratorWindowPatches
    {
        [HarmonyPatch(typeof(UIPowerGeneratorWindow), nameof(UIPowerGeneratorWindow._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetTargetCargoBytes_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.End().MatchBack(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Call), new CodeMatch(OpCodes.Ret));
            matcher.Advance(1).SetOperandAndAdvance(AccessTools.Method(typeof(UIPowerGeneratorWindowPatches), nameof(OnUpdate_Patch)));

            return matcher.InstructionEnumeration();
        }

        public static void OnUpdate_Patch(UIPowerGeneratorWindow window)
        {
            window.group0.gameObject.SetActive(true);
            window.group1.gameObject.SetActive(false);
            window.group2.gameObject.SetActive(false);

            if (!window.powerNetworkDesc.active) window.powerNetworkDesc._Open();

            window.group4_gammainfo.gameObject.SetActive(false);
            window.group5_product.gameObject.SetActive(false);
            window.group6_elec.gameObject.SetActive(true);
            Vector2 anchoredPosition = window.speedGroup.anchoredPosition;
            anchoredPosition = new Vector2(0.0f, anchoredPosition.y);
            window.speedGroup.anchoredPosition = anchoredPosition;
            window.elecGroup.anchoredPosition = new Vector2(-80f, anchoredPosition.y);
            window.needInventory = false;
            window.fuelIcon0.sprite = Resources.Load<Sprite>("Assets/texpack/原子能");
            window.fuelText0.text = "裂变能".TranslateFromJson();
            window.fuelText0.color = window.powerColor0;
            window.fuelCircle0.fillAmount = 1f;
        }
    }
}

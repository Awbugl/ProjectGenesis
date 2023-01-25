using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.UIPlanetBase
{
    public static class UIPlanetDetailExpand
    {
        private static UIButton _planetBaseBtn;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIGame), "_OnCreate")]
        public static void Init(UIGame __instance)
        {
            ProjectGenesis.planetBaseWindow = UIPlanetBaseWindow.CreateWindow();

            _planetBaseBtn = Util.CreateButton("星球基地".TranslateFromJson());
            Util.NormalizeRectWithTopLeft(_planetBaseBtn, 5, -40, __instance.planetDetail.rectTrans);
            _planetBaseBtn.onClick += obj => ProjectGenesis.planetBaseWindow.OpenWindow();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        public static void OnPlanetDataSet_Postfix(UIPlanetDetail __instance)
        {
            if (__instance.planet == null)
            {
                ProjectGenesis.planetBaseWindow._Close();
                return;
            }

            _planetBaseBtn.gameObject.SetActive(__instance.planet.type != EPlanetType.Gas);
            ProjectGenesis.planetBaseWindow.nameText.text = __instance.planet.displayName;
        }
    }
}

using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.UIPlanetFocus
{
    public static class UIPlanetDetailExpand
    {
        private static UIButton _planetFocusBtn;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIGame), "_OnCreate")]
        public static void Init(UIGame __instance)
        {
            ProjectGenesis.PlanetFocusWindow = UIPlanetFocusWindow.CreateWindow();

            _planetFocusBtn = Util.CreateButton("协调".TranslateFromJson());
            Util.NormalizeRectWithTopLeft(_planetFocusBtn, 5, -40, __instance.planetDetail.rectTrans);
            _planetFocusBtn.onClick += obj => ProjectGenesis.PlanetFocusWindow.OpenWindow();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        public static void OnPlanetDataSet_Postfix(UIPlanetDetail __instance)
        {
            if (__instance.planet == null)
            {
                ProjectGenesis.PlanetFocusWindow._Close();
                return;
            }

            var notgas = __instance.planet.type != EPlanetType.Gas;

            _planetFocusBtn.gameObject.SetActive(notgas);

            if (notgas)
            {
                ProjectGenesis.PlanetFocusWindow.nameText.text = __instance.planet.displayName + " - " + "星球倾向".TranslateFromJson();

                if (UIPlanetFocusWindow.CurPlanetId != __instance.planet.id)
                {
                    UIPlanetFocusWindow.CurPlanetId = __instance.planet.id;
                    ProjectGenesis.PlanetFocusWindow.OnPlanetChanged(UIPlanetFocusWindow.CurPlanetId);
                }
            }
        }
    }
}

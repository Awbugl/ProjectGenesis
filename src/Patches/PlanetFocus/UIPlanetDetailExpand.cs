using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class UIPlanetDetailExpand
    {
        private static UIButton _planetFocusBtn;

        [HarmonyPatch(typeof(UIGame), nameof(UIGame._OnInit))]
        [HarmonyPostfix]
        public static void Init(UIGame __instance)
        {
            if (_planetFocusBtn) return;

            ProjectGenesis.PlanetFocusWindow = UIPlanetFocusWindow.CreateWindow();

            UIButton button = __instance.planetDetail.planetDescBtn;

            _planetFocusBtn = Object.Instantiate(button, button.transform.parent);

            _planetFocusBtn.transform.localPosition = new Vector3(-36, -160, 0);

            UIUtil.RemovePersistentCalls(_planetFocusBtn.gameObject);

            _planetFocusBtn.button.onClick.AddListener(PlanetFocusBtnOnClick);

            Transform transform = _planetFocusBtn.transform.Find("text");

            Text btnText = transform.GetComponent<Text>();
            btnText.text = "星球基地".TranslateFromJson();
            Object.Destroy(transform.GetComponent<Localizer>());
        }

        private static void PlanetFocusBtnOnClick() => ProjectGenesis.PlanetFocusWindow.OpenWindow();

        [HarmonyPatch(typeof(UIPlanetDetail), nameof(UIPlanetDetail.OnPlanetDataSet))]
        [HarmonyPostfix]
        public static void OnPlanetDataSet_Postfix(UIPlanetDetail __instance)
        {
            if (__instance.planet == null)
            {
                ProjectGenesis.PlanetFocusWindow._Close();

                return;
            }

            bool notgas = __instance.planet.type != EPlanetType.Gas && GameMain.history.TechUnlocked(ProtoID.T行星协调中心);

            if (_planetFocusBtn) _planetFocusBtn.gameObject.SetActive(notgas);

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

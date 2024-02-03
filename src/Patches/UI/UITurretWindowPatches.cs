using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UITurretWindowPatches
    {
        private static Image handFillAmmoIcon3, handFillAmmoIcon4, handFillAmmoIcon5;
        private static UIButton handFillAmmoBtn3, handFillAmmoBtn4, handFillAmmoBtn5;

        [HarmonyPatch(typeof(UITurretWindow), nameof(UITurretWindow._OnInit))]
        [HarmonyPostfix]
        public static void UITurretWindow_OnInit(UITurretWindow __instance)
        {
            Transform transformParent = __instance.handFillAmmoBtn0.transform.parent;

            handFillAmmoBtn3 = Object.Instantiate(__instance.handFillAmmoBtn0, transformParent);
            handFillAmmoBtn3.data = 3;
            handFillAmmoBtn3.transform.localPosition = new Vector3(80, 32, 0);
            handFillAmmoIcon3 = handFillAmmoBtn3.GetComponent<Image>();

            handFillAmmoBtn4 = Object.Instantiate(__instance.handFillAmmoBtn0, transformParent);
            handFillAmmoBtn4.data = 4;
            handFillAmmoBtn4.transform.localPosition = new Vector3(80, 0, 0);
            handFillAmmoIcon4 = handFillAmmoBtn4.GetComponent<Image>();

            handFillAmmoBtn5 = Object.Instantiate(__instance.handFillAmmoBtn0, transformParent);
            handFillAmmoBtn5.data = 5;
            handFillAmmoBtn5.transform.localPosition = new Vector3(80, -32, 0);
            handFillAmmoIcon5 = handFillAmmoBtn5.GetComponent<Image>();
        }

        [HarmonyPatch(typeof(UITurretWindow), nameof(UITurretWindow._OnRegEvent))]
        [HarmonyPostfix]
        public static void UITurretWindow_OnRegEvent(UITurretWindow __instance)
        {
            handFillAmmoBtn3.onClick += __instance.OnHandFillAmmoButtonClick;
            handFillAmmoBtn4.onClick += __instance.OnHandFillAmmoButtonClick;
            handFillAmmoBtn5.onClick += __instance.OnHandFillAmmoButtonClick;
        }

        [HarmonyPatch(typeof(UITurretWindow), nameof(UITurretWindow._OnUnregEvent))]
        [HarmonyPostfix]
        public static void UITurretWindow_OnUnregEvent(UITurretWindow __instance)
        {
            handFillAmmoBtn3.onClick -= __instance.OnHandFillAmmoButtonClick;
            handFillAmmoBtn4.onClick -= __instance.OnHandFillAmmoButtonClick;
            handFillAmmoBtn5.onClick -= __instance.OnHandFillAmmoButtonClick;
        }

        [HarmonyPatch(typeof(UITurretWindow), nameof(UITurretWindow._OnUpdate))]
        [HarmonyPostfix]
        public static void UITurretWindow_OnUpdate(UITurretWindow __instance)
        {
            ref TurretComponent local1 = ref __instance.defenseSystem.turrets.buffer[__instance.turretId];

            if (local1.id != __instance.turretId) return;

            int[] turretNeed = ItemProto.turretNeeds[(int)local1.type];

            if (__instance.isLaser || turretNeed.Length == 0 || local1.itemCount != 0 || local1.bulletCount != 0)
            {
                handFillAmmoIcon3.gameObject.SetActive(false);
                handFillAmmoIcon4.gameObject.SetActive(false);
                handFillAmmoIcon5.gameObject.SetActive(false);
                return;
            }

            int tipsItemId = turretNeed[3];
            ItemProto itemProto3 = LDB.items.Select(tipsItemId);

            if (itemProto3 == null)
            {
                handFillAmmoIcon3.gameObject.SetActive(false);
            }
            else
            {
                handFillAmmoIcon3.gameObject.SetActive(__instance.history.ItemUnlocked(tipsItemId));
                handFillAmmoIcon3.sprite = itemProto3.iconSprite;
                handFillAmmoBtn3.tips.itemId = tipsItemId;
            }

            tipsItemId = turretNeed[4];
            ItemProto itemProto4 = LDB.items.Select(tipsItemId);

            if (itemProto4 == null)
            {
                handFillAmmoIcon4.gameObject.SetActive(false);
            }
            else
            {
                handFillAmmoIcon4.gameObject.SetActive(__instance.history.ItemUnlocked(tipsItemId));
                handFillAmmoIcon4.sprite = itemProto4.iconSprite;
                handFillAmmoBtn4.tips.itemId = tipsItemId;
            }

            tipsItemId = turretNeed[5];
            ItemProto itemProto5 = LDB.items.Select(tipsItemId);

            if (itemProto5 == null)
            {
                handFillAmmoIcon5.gameObject.SetActive(false);
            }
            else
            {
                handFillAmmoIcon5.gameObject.SetActive(__instance.history.ItemUnlocked(tipsItemId));
                handFillAmmoIcon5.sprite = itemProto5.iconSprite;
                handFillAmmoBtn5.tips.itemId = tipsItemId;
            }
        }
    }
}

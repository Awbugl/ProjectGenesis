using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using UnityEngine.UI;
using ERecipeType_1 = ERecipeType;

namespace ProjectGenesis.Patches
{
    class UIPatches
    {
        #region UITankWindow

        // Specify color of each fluid here, one per line.
        private static Dictionary<int, Color32> FLUID_COLOR = new Dictionary<int, Color32>
                                                              {
                                                                  { 新物品.硝酸, new Color32(61, 137, 224, 255) },
                                                                  { 新物品.双酚A, new Color32(176, 106, 85, 255) },
                                                                  { 新物品.三氯化铁, new Color32(116, 152, 99, 255) },
                                                                  { 新物品.氢燃料, new Color32(187, 217, 219, 255) },
                                                                  { 新物品.氢氯酸, new Color32(99, 179, 148, 255) },
                                                                  { 新物品.煤焦油, new Color32(167, 91, 0, 255) },
                                                                  { 新物品.氯苯, new Color32(226, 72, 86, 255) },
                                                                  { 新物品.邻苯二甲酸, new Color32(214, 39, 98, 255) },
                                                                  { 新物品.间苯二甲酸二苯酯, new Color32(51, 255, 173, 255) },
                                                                  { 新物品.环氧氯丙烷, new Color32(188, 149, 92, 255) },
                                                                  { 新物品.甘油, new Color32(218, 207, 147, 255) },
                                                                  { 新物品.反物质燃料, new Color32(33, 44, 65, 255) },
                                                                  { 新物品.二硝基氯苯, new Color32(147, 230, 43, 255) },
                                                                  { 新物品.二氯联苯胺, new Color32(109, 183, 101, 255) },
                                                                  { 新物品.二甲苯, new Color32(218, 127, 78, 255) },
                                                                  { 新物品.二氨基联苯胺, new Color32(158, 212, 68, 255) },
                                                                  { 新物品.氘核燃料, new Color32(117, 184, 41, 255) },
                                                                  { 新物品.丙酮, new Color32(115, 177, 74, 255) },
                                                                  { 新物品.苯酚, new Color32(119, 176, 123, 255) },
                                                                  { 新物品.氨, new Color32(216, 216, 216, 255) },
                                                                  { 6999, new Color32(236, 220, 219, 255) },
                                                                  { 新物品.有机液体, new Color32(66, 8, 89, 255) },
                                                                  { 新物品.乙烯, new Color32(185, 185, 185, 255) },
                                                                  { 新物品.盐水, new Color32(90, 126, 179, 255) },
                                                                  { 新物品.氧气, new Color32(170, 198, 255, 255) },
                                                                  { 6211, new Color32(10, 60, 16, 255) },
                                                                  { 6202, new Color32(223, 222, 31, 255) },
                                                                  { 6213, new Color32(29, 29, 135, 255) },
                                                                  { 6215, new Color32(255, 128, 52, 255) },
                                                                  { 6207, new Color32(116, 99, 22, 255) },
                                                                  { 6214, new Color32(142, 138, 60, 255) },
                                                                  { 6203, new Color32(202, 167, 27, 255) },
                                                                  { 6204, new Color32(224, 209, 23, 255) },
                                                                  { 6212, new Color32(222, 214, 0, 255) },
                                                                  { 6210, new Color32(138, 172, 164, 255) },
                                                                  { 6219, new Color32(193, 130, 58, 255) },
                                                                  { 6201, new Color32(241, 181, 37, 255) },
                                                                  { 6209, new Color32(230, 81, 21, 255) },
                                                                  { 6208, new Color32(220, 122, 29, 255) },
                                                                  { 6205, new Color32(131, 209, 255, 255) },
                                                              };

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITankWindow), "_OnUpdate")]
        public static void UITankWindow_OnUpdate(ref UITankWindow __instance)
        {
            TankComponent tankComponent = __instance.storage.tankPool[__instance.tankId];
            if (tankComponent.id != __instance.tankId)
            {
                return;
            }

            if (FLUID_COLOR.ContainsKey(tankComponent.fluidId))
            {
                __instance.exchangeAndColoring(FLUID_COLOR[tankComponent.fluidId]);
            }
        }

#endregion

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeProto), "madeFromString", MethodType.Getter)]

       
        public static void RecipeProto_madeFromString(ref RecipeProto __instance, ref string __result)
        {
            ERecipeType type = (ERecipeType)__instance.Type;
            if (type == ERecipeType.电路蚀刻)
            {
                __result = "电路蚀刻机".Translate();
            }
            else if (type == ERecipeType.高精度加工)
            {
                __result = "高精度装配线".Translate();
            }
            else if (type == ERecipeType.矿物处理)
            {
                __result = "矿物处理厂".Translate();
            }
            else if (type == ERecipeType.精密组装)
            {
                __result = "精密组装厂".Translate();
            }
            else if (type == ERecipeType.聚变生产)
            {
                __result = "紧凑式回旋加速器".Translate();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "fuelTypeString", MethodType.Getter)]

        public static void ItemProto_fuelTypeString(ref ItemProto __instance, ref string __result)
        {
            int type = (int)__instance.FuelType;
            if (type == 5)
            {
                __result = "裂变能".Translate();
            }
            if (type == 6)
            {
                __result = "聚变能".Translate();
            }
        }

        //制造时间修复
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIAssemblerWindow), "_OnUpdate")]
        public static void UIAssemblerWindow_OnUpdate(ref UIAssemblerWindow __instance, ref int __result) 
        {
            
            if (__instance.assemblerId == 0 || __instance.factory == null)
            {
                __instance._Close();
                return;
            }

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assembler.id != __instance.assemblerId)
            {
                __instance._Close();
                return;
            }

            if (assembler.recipeId != __instance.servingBoxRecipeId)
            {
                Debug.Log("serving box change");
                __instance.DeterminePlayerPackageVisible(assembler.recipeId);
                __instance.OnServingBoxChange();
            }

            __instance.RefreshIncUIs(ref assembler, lerp: true);
            bool flag = __instance.factory.entityPool[assembler.entityId].warningId > 0;
            __instance.alarmSwitchButton.highlighted = flag;
            __instance.alarmSwitchText.text = (flag ? "警报开" : "警报关").Translate();
            __instance.pasteButton.gameObject.SetActive(BuildingParameters.clipboard.CanPasteToFactoryObject(assembler.entityId, __instance.factory));
            __instance.windowTrans.sizeDelta = new Vector2(__instance.windowTrans.sizeDelta.x, (__instance.playerInventory.active ? (__instance.playerInventory.rowCount * 50 + 220) : 270) + (__instance.incInfoGroup.gameObject.activeSelf ? 40 : 0));
            PowerConsumerComponent powerConsumerComponent = __instance.powerSystem.consumerPool[assembler.pcId];
            int networkId = powerConsumerComponent.networkId;
            PowerNetwork powerNetwork = __instance.powerSystem.netPool[networkId];
            float num = (powerNetwork != null && networkId > 0) ? ((float)powerNetwork.consumerRatio) : 0f;
            long valuel = (long)((double)(powerConsumerComponent.requiredEnergy * 60) * (double)num);
            StringBuilderUtility.WriteKMG(__instance.powerServedSB, 8, valuel);
            StringBuilderUtility.WriteUInt(__instance.powerServedSB, 12, 3, (uint)(num * 100f));
            if (num == 1f)
            {
                __instance.powerText.text = __instance.powerServedSB.ToString();
                __instance.powerIcon.color = __instance.powerNormalIconColor;
                __instance.powerText.color = __instance.powerNormalColor;
            }
            else if (num > 0.1f)
            {
                __instance.powerText.text = __instance.powerServedSB.ToString();
                __instance.powerIcon.color = __instance.powerLowIconColor;
                __instance.powerText.color = __instance.powerLowColor;
            }
            else
            {
                __instance.powerText.text = "未供电".Translate();
                __instance.powerIcon.color = Color.clear;
                __instance.powerText.color = __instance.powerOffColor;
            }

            if (assembler.recipeId == 0)
            {
                __instance.recipeGroup.gameObject.SetActive(value: true);
                __instance.produceGroup.gameObject.SetActive(value: false);
                __instance.stateText.text = "待机".Translate();
                __instance.stateText.color = __instance.idleColor;
                __instance.productButton0.tips.itemId = 0;
                __instance.productButton1.tips.itemId = 0;
                __instance.productButton0.tips.itemInc = 0;
                __instance.productButton1.tips.itemInc = 0;
                __instance.productButton0.tips.itemCount = 0;
                __instance.productButton1.tips.itemCount = 0;
                __instance.productButton0.tips.type = UIButton.ItemTipType.Item;
                __instance.productButton1.tips.type = UIButton.ItemTipType.Item;
                return;
            }

            __instance.recipeGroup.gameObject.SetActive(value: false);
            __instance.produceGroup.gameObject.SetActive(value: true);
            if (assembler.products.Length == 1)
            {
                ItemProto itemProto = LDB.items.Select(assembler.products[0]);
                __instance.productIcon0.sprite = itemProto?.iconSprite;
                __instance.productCountText0.text = assembler.produced[0].ToString();
                __instance.productButton0.tips.itemId = assembler.products[0];
                __instance.productButton0.tips.itemInc = 0;
                __instance.productButton0.tips.itemCount = 0;
                __instance.productButton0.tips.type = UIButton.ItemTipType.Item;
                __instance.productButton0.tips.corner = 2;
                __instance.productButton0.tips.delay = 0.2f;
                __instance.productGroup.sizeDelta = new Vector2(64f, 64f);
                __instance.speedGroup.anchoredPosition = new Vector2(80f, __instance.speedGroup.anchoredPosition.y);
                __instance.servingGroup.anchoredPosition = new Vector2(160f, __instance.speedGroup.anchoredPosition.y);
                __instance.productButton0.button.interactable = (assembler.produced[0] > 0);
                __instance.productProgress1.gameObject.SetActive(value: false);
                __instance.extraProductProgress1.gameObject.SetActive(value: false);
                __instance.productIcon1.gameObject.SetActive(value: false);
                __instance.productCountText1.gameObject.SetActive(value: false);
            }
            else if (assembler.products.Length > 1)
            {
                ItemProto itemProto2 = LDB.items.Select(assembler.products[0]);
                ItemProto itemProto3 = LDB.items.Select(assembler.products[1]);
                __instance.productIcon0.sprite = itemProto2?.iconSprite;
                __instance.productIcon1.sprite = itemProto3?.iconSprite;
                __instance.productButton0.tips.itemId = assembler.products[0];
                __instance.productButton0.tips.itemInc = 0;
                __instance.productButton0.tips.itemCount = 0;
                __instance.productButton0.tips.type = UIButton.ItemTipType.Item;
                __instance.productButton0.tips.corner = 2;
                __instance.productButton0.tips.delay = 0.2f;
                __instance.productButton1.tips.itemId = assembler.products[1];
                __instance.productButton1.tips.itemInc = 0;
                __instance.productButton1.tips.itemCount = 0;
                __instance.productButton1.tips.type = UIButton.ItemTipType.Item;
                __instance.productButton1.tips.corner = 2;
                __instance.productButton1.tips.delay = 0.2f;
                __instance.productCountText0.text = assembler.produced[0].ToString();
                __instance.productCountText1.text = assembler.produced[1].ToString();
                __instance.productGroup.sizeDelta = new Vector2(128f, 64f);
                __instance.speedGroup.anchoredPosition = new Vector2(144f, __instance.speedGroup.anchoredPosition.y);
                __instance.servingGroup.anchoredPosition = new Vector2(224f, __instance.speedGroup.anchoredPosition.y);
                __instance.productButton0.button.interactable = (assembler.produced[0] > 0);
                __instance.productButton1.button.interactable = (assembler.produced[1] > 0);
                __instance.productProgress1.gameObject.SetActive(value: true);
                __instance.extraProductProgress1.gameObject.SetActive(value: true);
                __instance.productIcon1.gameObject.SetActive(value: true);
                __instance.productCountText1.gameObject.SetActive(value: true);
            }

            __instance.SyncServingStorage(ref assembler);
            float num2 = (float)((double)assembler.timeSpend / 60.0);
            float num3 = 60f / num2;
            float num4 = (float)(0.0001 * (double)assembler.speedOverride * (double)num);
            if (!assembler.replicating)
            {
                num4 = 0f;
            }

            float num5 = num3;
            float num6 = (num4 < 1E-05f) ? 0f : (num4 * (1f + (float)assembler.extraSpeed * 0.1f / (float)assembler.speedOverride));
            __instance.speedText.text = (num5 * num6).ToString("0.0") + "每分钟".Translate();
            float num7 = Mathf.Clamp01((float)((double)assembler.time / (double)assembler.timeSpend));
            float fillAmount = Mathf.Clamp01((float)((double)assembler.extraTime / (double)assembler.extraTimeSpend));
            __instance.productProgress0.fillAmount = num7;
            __instance.productProgress1.fillAmount = num7;
            __instance.extraProductProgress0.fillAmount = fillAmount;
            __instance.extraProductProgress1.fillAmount = fillAmount;
            int num8 = (int)(num7 * 0.999f * (float)__instance.speedArrows.Length) % __instance.speedArrows.Length;
            for (int i = 0; i < __instance.speedArrows.Length; i++)
            {
                __instance.speedArrows[i].color = ((i == num8 && assembler.replicating && num > 0.1f) ? __instance.marqueeOnColor : __instance.marqueeOffColor);
            }

            if (assembler.replicating)
            {
                if (num == 1f)
                {
                    __instance.stateText.text = "正常运转".Translate();
                    __instance.stateText.color = __instance.workNormalColor;
                }
                else if (num > 0.1f)
                {
                    __instance.stateText.text = "电力不足".Translate();
                    __instance.stateText.color = __instance.powerLowColor;
                }
                else
                {
                    __instance.stateText.text = "停止运转".Translate();
                    __instance.stateText.color = __instance.powerOffColor;
                }

                return;
            }

            if (assembler.time >= assembler.timeSpend)
            {
                __instance.stateText.text = "产物堆积".Translate();
                __instance.stateText.color = __instance.workStoppedColor;
                return;
            }

            bool flag2 = false;
            for (int j = 0; j < assembler.requireCounts.Length; j++)
            {
                if (assembler.served[j] < assembler.requireCounts[j])
                {
                    flag2 = true;
                    break;
                }
            }

            if (flag2)
            {
                __instance.stateText.text = "缺少原材料".Translate();
                __instance.stateText.color = __instance.workStoppedColor;
            }
            else if (num == 1f)
            {
                __instance.stateText.text = "待机".Translate();
                __instance.stateText.color = __instance.idleColor;
            }
            else if (num > 0.1f)
            {
                __instance.stateText.text = "电力不足".Translate();
                __instance.stateText.color = __instance.powerLowColor;
            }
            else
            {
                __instance.stateText.text = "停止运转".Translate();
                __instance.stateText.color = __instance.powerOffColor;
            }
            __result = 0;
            return;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssemblerComponent), "Import")]
        public static void AssemblerComponent_Import(ref AssemblerComponent __instance, ref BinaryReader r,ref int __result) 
        {
            
            int num = r.ReadInt32();
            bool flag = num >= 1;
            __instance.id = r.ReadInt32();
            __instance.entityId = r.ReadInt32();
            __instance.pcId = r.ReadInt32();
            __instance.replicating = r.ReadBoolean();
            r.ReadBoolean();
            __instance.speed = r.ReadInt32();
            __instance.time = r.ReadInt32();
            if (num >= 2)
            {
                __instance.speedOverride = r.ReadInt32();
                __instance.extraTime = r.ReadInt32();
                __instance.extraSpeed = r.ReadInt32();
                __instance.extraPowerRatio = r.ReadInt32();
                __instance.productive = r.ReadBoolean();
                if (num >= 3)
                {
                    __instance.forceAccMode = r.ReadBoolean();
                }
                else
                {
                    __instance.forceAccMode = false;
                }
            }
            else
            {
                __instance.speedOverride = __instance.speed;
                __instance.extraTime = 0;
                __instance.extraSpeed = 0;
                __instance.extraPowerRatio = 0;
                __instance.productive = false;
                __instance.forceAccMode = false;
            }

            if (flag)
            {
                __instance.recipeId = r.ReadInt16();
            }
            else
            {
                __instance.recipeId = r.ReadInt32();
            }

            if (__instance.recipeId > 0)
            {
                RecipeProto recipeProto = null;
                recipeProto = LDB.recipes.Select(__instance.recipeId);
                if (flag)
                {
                    __instance.recipeType = (global::ERecipeType)r.ReadByte();
                    __instance.timeSpend = r.ReadInt32();
                    if (num >= 2)
                    {
                        __instance.extraTimeSpend = r.ReadInt32();
                    }
                    else
                    {
                        __instance.extraTimeSpend = __instance.timeSpend * 10;
                    }

                    int num2 = 0;
                    num2 = r.ReadByte();
                    __instance.requires = new int[num2];
                    for (int i = 0; i < num2; i++)
                    {
                        __instance.requires[i] = r.ReadInt16();
                    }

                    num2 = r.ReadByte();
                    __instance.requireCounts = new int[num2];
                    for (int j = 0; j < num2; j++)
                    {
                        __instance.requireCounts[j] = r.ReadInt16();
                    }

                    num2 = r.ReadByte();
                    __instance.served = new int[num2];
                    for (int k = 0; k < num2; k++)
                    {
                        __instance.served[k] = r.ReadInt32();
                    }

                    if (num >= 2)
                    {
                        num2 = r.ReadByte();
                        __instance.incServed = new int[num2];
                        for (int l = 0; l < num2; l++)
                        {
                            __instance.incServed[l] = r.ReadInt32();
                        }
                    }
                    else
                    {
                        __instance.incServed = new int[__instance.served.Length];
                        for (int m = 0; m < __instance.served.Length; m++)
                        {
                            __instance.incServed[m] = 0;
                        }
                    }

                    num2 = r.ReadByte();
                    __instance.needs = new int[num2];
                    for (int n = 0; n < num2; n++)
                    {
                        __instance.needs[n] = r.ReadInt16();
                    }

                    num2 = r.ReadByte();
                    __instance.products = new int[num2];
                    for (int num3 = 0; num3 < num2; num3++)
                    {
                        __instance.products[num3] = r.ReadInt16();
                    }

                    num2 = r.ReadByte();
                    __instance.productCounts = new int[num2];
                    for (int num4 = 0; num4 < num2; num4++)
                    {
                        __instance.productCounts[num4] = r.ReadInt16();
                    }

                    num2 = r.ReadByte();
                    __instance.produced = new int[num2];
                    for (int num5 = 0; num5 < num2; num5++)
                    {
                        __instance.produced[num5] = r.ReadInt32();
                    }

                    if (recipeProto != null)
                    {
                        if (recipeProto.Items.Length == __instance.requires.Length)
                        {
                            Array.Copy(recipeProto.Items, __instance.requires, __instance.requires.Length);
                        }

                        if (recipeProto.ItemCounts.Length == __instance.requireCounts.Length)
                        {
                            Array.Copy(recipeProto.ItemCounts, __instance.requireCounts, __instance.requireCounts.Length);
                        }

                        if (recipeProto.Results.Length == __instance.products.Length)
                        {
                            Array.Copy(recipeProto.Results, __instance.products, __instance.products.Length);
                        }

                        if (recipeProto.ResultCounts.Length == __instance.productCounts.Length)
                        {
                            Array.Copy(recipeProto.ResultCounts, __instance.productCounts, __instance.productCounts.Length);
                        }

                        __instance.timeSpend = recipeProto.TimeSpend;
                    }
                }
                else
                {
                    __instance.recipeType = (global::ERecipeType)r.ReadInt32();
                    __instance.timeSpend = r.ReadInt32();
                    if (num >= 2)
                    {
                        __instance.extraTimeSpend = r.ReadInt32();
                    }
                    else
                    {
                        __instance.extraTimeSpend = __instance.timeSpend * 10;
                    }

                    int num6 = 0;
                    num6 = r.ReadInt32();
                    __instance.requires = new int[num6];
                    for (int num7 = 0; num7 < num6; num7++)
                    {
                        __instance.requires[num7] = r.ReadInt32();
                    }

                    num6 = r.ReadInt32();
                    __instance.requireCounts = new int[num6];
                    for (int num8 = 0; num8 < num6; num8++)
                    {
                        __instance.requireCounts[num8] = r.ReadInt32();
                    }

                    num6 = r.ReadInt32();
                    __instance.served = new int[num6];
                    for (int num9 = 0; num9 < num6; num9++)
                    {
                        __instance.served[num9] = r.ReadInt32();
                    }

                    if (num >= 2)
                    {
                        num6 = r.ReadByte();
                        __instance.incServed = new int[num6];
                        for (int num10 = 0; num10 < num6; num10++)
                        {
                            __instance.incServed[num10] = r.ReadInt32();
                        }
                    }
                    else
                    {
                        __instance.incServed = new int[__instance.served.Length];
                        for (int num11 = 0; num11 < __instance.served.Length; num11++)
                        {
                            __instance.incServed[num11] = 0;
                        }
                    }

                    num6 = r.ReadInt32();
                    __instance.needs = new int[num6];
                    for (int num12 = 0; num12 < num6; num12++)
                    {
                        __instance.needs[num12] = r.ReadInt32();
                    }

                    num6 = r.ReadInt32();
                    __instance.products = new int[num6];
                    for (int num13 = 0; num13 < num6; num13++)
                    {
                        __instance.products[num13] = r.ReadInt32();
                    }

                    num6 = r.ReadInt32();
                    __instance.productCounts = new int[num6];
                    for (int num14 = 0; num14 < num6; num14++)
                    {
                        __instance.productCounts[num14] = r.ReadInt32();
                    }

                    num6 = r.ReadInt32();
                    __instance.produced = new int[num6];
                    for (int num15 = 0; num15 < num6; num15++)
                    {
                        __instance.produced[num15] = r.ReadInt32();
                    }
                }
            }

            if (num <= 2)
            {
                __instance.CheckOldVersionProductive();
            }

            __result = 0;
            return;

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssemblerComponent), "SetRecipe")]
        public static void AssemblerComponent_SetRecipe(ref AssemblerComponent __instance, ref int recpId, ref SignData[] signPool, ref int __result) 
        {

            __instance.replicating = false;
            __instance.time = 0;
            __instance.extraTime = 0;
            __instance.extraSpeed = 0;
            __instance.productive = false;
            __instance.forceAccMode = false;
            RecipeProto recipeProto = null;
            if (recpId > 0)
            {
                recipeProto = LDB.recipes.Select(recpId);
            }

            if (recipeProto != null)
            {
                __instance.recipeId = recipeProto.ID;
                __instance.recipeType = recipeProto.Type;
                __instance.timeSpend = recipeProto.TimeSpend;
                __instance.extraTimeSpend = recipeProto.TimeSpend * 100000;
                __instance.speedOverride = __instance.speed;
                __instance.productive = recipeProto.productive;
                __instance.requires = new int[recipeProto.Items.Length];
                Array.Copy(recipeProto.Items, __instance.requires, __instance.requires.Length);
                __instance.requireCounts = new int[recipeProto.ItemCounts.Length];
                Array.Copy(recipeProto.ItemCounts, __instance.requireCounts, __instance.requireCounts.Length);
                __instance.served = new int[__instance.requireCounts.Length];
                __instance.incServed = new int[__instance.requireCounts.Length];
                Assert.True(__instance.requires.Length == __instance.requireCounts.Length);
                __instance.needs = new int[6];
                __instance.products = new int[recipeProto.Results.Length];
                Array.Copy(recipeProto.Results, __instance.products, __instance.products.Length);
                __instance.productCounts = new int[recipeProto.ResultCounts.Length];
                Array.Copy(recipeProto.ResultCounts, __instance.productCounts, __instance.productCounts.Length);
                __instance.produced = new int[__instance.productCounts.Length];
                Assert.True(__instance.products.Length == __instance.productCounts.Length);
                signPool[__instance.entityId].iconId0 = (uint)__instance.recipeId;
                signPool[__instance.entityId].iconType = 2u;
            }
            else
            {
                __instance.recipeId = 0;
                __instance.recipeType = (global::ERecipeType)ERecipeType.None;
                __instance.timeSpend = 0;
                __instance.extraTimeSpend = 0;
                __instance.speedOverride = __instance.speed;
                __instance.requires = null;
                __instance.requireCounts = null;
                __instance.served = null;
                __instance.incServed = null;
                __instance.needs = null;
                __instance.products = null;
                __instance.productCounts = null;
                __instance.produced = null;
                signPool[__instance.entityId].iconId0 = 0u;
                signPool[__instance.entityId].iconType = 0u;
            }
            __result = 0;
            return;
        }
        */

        //发电类型
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "GetPropValue")]
        public static void GetPropValuePatch(ref ItemProto __instance, int index, ref string __result) 
        {
            if ((ulong)index >= (ulong)((long)__instance.DescFields.Length))
            {
                Debug.Log("Genesis Book:Now Loading");
                __result = "";
                return;
            }
            switch (__instance.DescFields[index])
            {
                case 4:
                    if (__instance.prefabDesc.isPowerGen)
                    {
                        if (__instance.prefabDesc.fuelMask == 4)
                        {
                            __result = "质能转换";
                        }
                        if (__instance.prefabDesc.fuelMask == 5) {
                            __result = "裂变能";
                        }
                        if (__instance.prefabDesc.fuelMask == 6)
                        {
                            __result = "仿星器";
                        }
                    }
                    return;
            }


        }
        


        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate")]
        public static void AssemblerComponent_InternalUpdate(ref AssemblerComponent __instance, float power,
                                                             int[] productRegister, int[] consumeRegister)
        {
            if (GameMain.history.TechUnlocked(1814) && __instance.recipeType == (ERecipeType_1)ERecipeType.Chemical
                                                    && __instance.speed == 20000)
            {
                __instance.speed = 40000;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "GetPropValue")]
        public static void ItemProto_GetPropValue(ref ItemProto __instance, ref string __result, int index,
                                                  StringBuilder sb, int incLevel)
        {
            if (GameMain.history.TechUnlocked(1814) && __instance.Type == EItemType.Production
                                                    && __instance.prefabDesc.assemblerRecipeType
                                                    == (ERecipeType_1)ERecipeType.Chemical && index == 22)
                __result = "4x";
        }


#region UITechNode

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITechNode), "UpdateLayoutDynamic")]
        public static void UITechNode_UpdateLayoutDynamic(ref UITechNode __instance, bool forceUpdate = false,
                                                          bool forceReset = false)
        {
            float num4
                = Mathf.Max(__instance.unlockText.preferredWidth - 40f + __instance.unlockTextTrans.anchoredPosition.x,
                            Math.Min(__instance.techProto.unlockRecipeArray.Length, 3) * 46) + __instance.baseWidth;
            if (num4 < __instance.minWidth)
            {
                num4 = __instance.minWidth;
            }

            if (num4 > __instance.maxWidth) num4 = __instance.maxWidth;

            if (__instance.focusState < 1f)
            {
                __instance.panelRect.sizeDelta
                    = new Vector2(Mathf.Lerp(__instance.minWidth, num4, __instance.focusState),
                                  __instance.panelRect.sizeDelta.y);
            }
            else
            {
                __instance.panelRect.sizeDelta
                    = new Vector2(Mathf.Lerp(num4, __instance.maxWidth, __instance.focusState - 1f),
                                  __instance.panelRect.sizeDelta.y);
            }

            __instance.titleText.rectTransform.sizeDelta = new Vector2(__instance.panelRect.sizeDelta.x
                                                                       - ((GameMain.history
                                                                                   .TechState(__instance.techProto.ID)
                                                                                   .curLevel > 0)
                                                                           ? 65
                                                                           : 25), 24f);
        }

#endregion
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
// ReSharper disable Unity.UnknownResource
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static partial class AddVeinPatches
    {
        internal static readonly sbyte VeinTypeCount = 19;

        private static readonly Color32[] VeinColors =
        {
            new Color(0.538f, 0.538f, 0.538f), // Default
            new Color(0.288f, 0.587f, 0.858f), // Iron
            new Color(1.000f, 0.490f, 0.307f), // Copper
            new Color(0.214f, 0.745f, 0.531f), // Silicium
            new Color(1.000f, 1.000f, 1.000f), // Titanium
            new Color(0.483f, 0.461f, 0.444f), // Stone
            new Color(0.113f, 0.130f, 0.140f), // Coal
            new Color(0.000f, 0.000f, 0.000f), // Oil
            new Color(1.000f, 1.000f, 1.000f), // Fireice
            new Color(0.489f, 0.601f, 0.745f), // Diamond
            new Color(0.091f, 0.377f, 0.217f), // Fractal
            new Color(0.538f, 0.613f, 0.078f), // Crysrub
            new Color(0.575f, 0.270f, 0.830f), // Grat
            new Color(0.571f, 0.708f, 0.647f), // Bamboo
            new Color(0.349f, 0.222f, 0.247f), // Mag
            new Color(0.538f, 0.538f, 0.538f), // Aluminum
            new Color(0.685f, 0.792f, 0.000f), // Radioactive
            new Color(0.113f, 0.130f, 0.140f), // Tungsten
            new Color(0.965f, 0.867f, 0.352f)  // Sulfur
        };

        internal static void ModifyVeinData()
        {
            AddVeinProtos(NewVein(15, "铝矿脉", "I铝矿", "Assets/texpack/铝矿脉", ProtoID.I铝矿, 25, 1, 60),
                          NewVein(16, "放射性矿脉", "I放射性矿物", "Assets/texpack/放射晶体矿脉_新新", ProtoID.I放射性矿物, 35, 2, 90),
                          NewVein(17, "钨矿脉", "I钨矿", "Assets/texpack/钨矿脉", ProtoID.I钨矿, 34, 1, 120),
                          NewVein(18, "硫矿脉", "I硫矿", "Assets/texpack/硫矿脉_新", ProtoID.I硫矿, 36, 1, 90));

            VeinProto NewVein(
                int id,
                string name,
                string description,
                string iconPath,
                int miningItem,
                int miningEffect,
                int modelIndex,
                int miningTime)
                => new VeinProto
                {
                    ID = id,
                    Name = name,
                    Description = description,
                    IconPath = iconPath,
                    MiningItem = miningItem,
                    MiningEffect = miningEffect,
                    ModelIndex = modelIndex,
                    MiningTime = miningTime,
                    CircleRadius = 1,
                    MinerBaseModelIndex = 58,
                    MinerCircleModelIndex = 59,
                    MiningAudio = 122,
                    ModelCount = 1
                };
        }

        private static void AddVeinProtos(params VeinProto[] protos)
        {
            VeinProtoSet veins = LDB.veins;

            int dataArrayLength = veins.dataArray.Length;

            Array.Resize(ref veins.dataArray, dataArrayLength + protos.Length);

            for (int index = 0; index < protos.Length; ++index)
            {
                veins.dataArray[dataArrayLength + index] = protos[index];
            }

            veins.OnAfterDeserialize();
        }

        internal static void SetMinerMk2Color()
        {
            var texture = Resources.Load<Texture>("Assets/texpack/矿机渲染索引");

            ref PrefabDesc prefabDesc = ref LDB.models.Select(256).prefabDesc;
            prefabDesc.materials[0].SetTexture("_VeinColorTex", texture);
            ref Material[] prefabDescLODMaterial = ref prefabDesc.lodMaterials[0];
            prefabDescLODMaterial[0].SetTexture("_VeinColorTex", texture);
            prefabDescLODMaterial[1].SetTexture("_VeinColorTex", texture);
            prefabDescLODMaterial[2].SetTexture("_VeinColorTex", texture);

            prefabDesc = ref LDB.models.Select(59).prefabDesc;
            prefabDescLODMaterial = ref prefabDesc.lodMaterials[0];
            prefabDescLODMaterial[1].SetTexture("_VeinColorTex", texture);
            prefabDescLODMaterial[2].SetTexture("_VeinColorTex", texture);
            prefabDescLODMaterial[3].SetTexture("_VeinColorTex", texture);
        }

        internal static void SetChemicalRecipeFcol()
        {
            var texture = Resources.Load<Texture>("Assets/texpack/chemical-plant-recipe-fcol");

            ref PrefabDesc prefabDesc = ref LDB.models.Select(64).prefabDesc;
            prefabDesc.lodMaterials[0][1].SetTexture("_FluidTex", texture);
            prefabDesc.lodMaterials[1][1].SetTexture("_FluidTex", texture);

            prefabDesc = ref LDB.models.Select(376).prefabDesc;
            prefabDesc.lodMaterials[0][1].SetTexture("_FluidTex", texture);
            prefabDesc.lodMaterials[1][1].SetTexture("_FluidTex", texture);
        }

        [HarmonyPatch(typeof(UISandboxMenu), "StaticLoad")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISandboxMenu_StaticLoad_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_0),
                                 new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(LDB), nameof(LDB.veins))),
                                 new CodeMatch(OpCodes.Ldfld), new CodeMatch(OpCodes.Ldlen));

            matcher.Advance(1).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)14))
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISandboxMenu), "StaticLoad")]
        [HarmonyPostfix]
        public static void UISandboxMenu_StaticLoad_Postfix(ref VeinProto[,] ___veinProtos)
        {
            ___veinProtos[1, 7] = LDB.veins.Select(15);
            ___veinProtos[1, 8] = LDB.veins.Select(16);
        }

        [HarmonyPatch(typeof(PlanetAlgorithm), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm7), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm11), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm12), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm13), "GenerateVeins")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetAlgorithm_GenerateVeins_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15));
            matcher.SetOperandAndAdvance(19);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetAlgorithm), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm11), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm12), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm13), "GenerateVeins")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetAlgorithm_InitnalVeins_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            MethodBase original)
        {
            var matcher = new CodeMatcher(instructions);

            Type type = original.DeclaringType;

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldc_I4_2),
                                 new CodeMatch(OpCodes.Stfld, AccessTools.Field(type, "veinVectorCount")));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddVeinPatches), nameof(InitBirthVeinVectors))));

            matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Ldc_I4_3);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_2));

            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_3);

            return matcher.InstructionEnumeration();
        }

        public static void InitBirthVeinVectors(PlanetAlgorithm algorithm)
        {
            algorithm.veinVectorTypes[2] = (EVeinType)15;
            algorithm.veinVectors[2] = algorithm.planet.birthResourcePoint2;
        }

        internal static Vector2 Rotate(Vector2 v, float angle)
        {
            float delta = angle * Mathf.PI / 180;
            return new Vector2(v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta), v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta));
        }

        [HarmonyPatch(typeof(PlanetData), "GenBirthPoints", typeof(PlanetRawData), typeof(int))]
        [HarmonyPostfix]
        public static void PlanetData_GenBirthPoints_Postfix(PlanetData __instance, PlanetRawData rawData, int _birthSeed)
        {
            var dotNet35Random = new DotNet35Random(_birthSeed);
            Pose pose = __instance.PredictPose(85.0);
            Vector3 vector3_1 = Maths.QInvRotateLF(pose.rotation, __instance.star.uPosition - (VectorLF3)pose.position * 40000.0);
            vector3_1.Normalize();
            Vector3 normalized1 = Vector3.Cross(vector3_1, Vector3.up).normalized;
            Vector3 normalized2 = Vector3.Cross(normalized1, vector3_1).normalized;
            int num1 = 0;
            int num2;
            for (num2 = 256; num1 < num2; ++num1)
            {
                float num3 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.5f;
                float num4 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.5f;
                Vector3 vector3_2 = vector3_1 + num3 * normalized1 + num4 * normalized2;
                vector3_2.Normalize();
                __instance.birthPoint = vector3_2 * (float)(__instance.realRadius + 0.20000000298023224 + 1.4500000476837158);
                Vector3 vector3_3 = Vector3.Cross(vector3_2, Vector3.up);
                normalized1 = vector3_3.normalized;
                vector3_3 = Vector3.Cross(normalized1, vector3_2);
                normalized2 = vector3_3.normalized;
                bool flag = false;
                for (int index = 0; index < 10; ++index)
                {
                    Vector2 vector2_1
                        = new Vector2((float)(dotNet35Random.NextDouble() * 2.0 - 1.0), (float)(dotNet35Random.NextDouble() * 2.0 - 1.0)).normalized *
                          0.1f;
                    Vector2 vector2_2 = Rotate(vector2_1, 120);
                    float num5 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.006f;
                    float num6 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.006f;
                    vector2_2.x += num5;
                    vector2_2.y += num6;
                    Vector2 vector2_3 = Rotate(vector2_1, 240);
                    float num51 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.006f;
                    float num61 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.006f;
                    vector2_3.x += num51;
                    vector2_3.y += num61;
                    vector3_3 = vector3_2 + vector2_1.x * normalized1 + vector2_1.y * normalized2;
                    Vector3 normalized3 = vector3_3.normalized;
                    vector3_3 = vector3_2 + vector2_2.x * normalized1 + vector2_2.y * normalized2;
                    Vector3 normalized4 = vector3_3.normalized;
                    vector3_3 = vector3_2 + vector2_3.x * normalized1 + vector2_3.y * normalized2;
                    Vector3 normalized5 = vector3_3.normalized;
                    __instance.birthResourcePoint0 = normalized3.normalized;
                    __instance.birthResourcePoint1 = normalized4.normalized;
                    __instance.birthResourcePoint2 = normalized5.normalized;

                    float num7 = __instance.realRadius + 0.2f;
                    if (rawData.QueryHeight(vector3_2) > num7 &&
                        rawData.QueryHeight(normalized3) > num7 &&
                        rawData.QueryHeight(normalized4) > num7 &&
                        rawData.QueryHeight(normalized5) > num7)
                    {
                        Vector3 vpos1 = normalized3 + normalized1 * 0.03f;
                        Vector3 vpos2 = normalized3 - normalized1 * 0.03f;
                        Vector3 vpos3 = normalized3 + normalized2 * 0.03f;
                        Vector3 vpos4 = normalized3 - normalized2 * 0.03f;
                        Vector3 vpos5 = normalized4 + normalized1 * 0.03f;
                        Vector3 vpos6 = normalized4 - normalized1 * 0.03f;
                        Vector3 vpos7 = normalized4 + normalized2 * 0.03f;
                        Vector3 vpos8 = normalized4 - normalized2 * 0.03f;
                        Vector3 vpos9 = normalized5 + normalized1 * 0.03f;
                        Vector3 vpos10 = normalized5 - normalized1 * 0.03f;
                        Vector3 vpos11 = normalized5 + normalized2 * 0.03f;
                        Vector3 vpos12 = normalized5 - normalized2 * 0.03f;
                        if (rawData.QueryHeight(vpos1) > num7 &&
                            rawData.QueryHeight(vpos2) > num7 &&
                            rawData.QueryHeight(vpos3) > num7 &&
                            rawData.QueryHeight(vpos4) > num7 &&
                            rawData.QueryHeight(vpos5) > num7 &&
                            rawData.QueryHeight(vpos6) > num7 &&
                            rawData.QueryHeight(vpos7) > num7 &&
                            rawData.QueryHeight(vpos8) > num7 &&
                            rawData.QueryHeight(vpos9) > num7 &&
                            rawData.QueryHeight(vpos10) > num7 &&
                            rawData.QueryHeight(vpos11) > num7 &&
                            rawData.QueryHeight(vpos12) > num7)
                        {
                            flag = true;
                            break;
                        }
                    }
                }

                if (flag) break;
            }

            if (num1 < num2) return;
            __instance.birthPoint = new Vector3(0.0f, __instance.realRadius + 5f, 0.0f);
        }
    }
}

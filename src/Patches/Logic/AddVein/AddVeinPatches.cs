using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static partial class AddVeinPatches
    {
        public static readonly sbyte VeinTypeCount = 19;
        
        internal static void ModifyVeinData()
        {
            AddVeinProtos(NewVein(15, "铝矿脉", "I铝矿", "Assets/texpack/铝矿脉", 6202, 25),
                          NewVein(16, "放射性矿脉", "I放射性矿物", "Assets/texpack/放射晶体矿脉_新", 6222, 31, 4, 90),
                          NewVein(17, "钨矿脉", "I钨矿", "Assets/texpack/钨矿脉", 6201, 34, 1, 120),
                          NewVein(18, "硫矿脉", "I硫矿", "Assets/texpack/硫矿脉", 6207, 34, 4, 90));

            VeinProto NewVein(
                int id,
                string name,
                string description,
                string iconPath,
                int miningItem,
                int miningEffect,
                int modelIndex = 1,
                int miningTime = 60)
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
                    float num5 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.06f;
                    float num6 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.06f;
                    vector2_2.x += num5;
                    vector2_2.y += num6;
                    Vector2 vector2_3 = Rotate(vector2_2, 120);
                    float num51 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.06f;
                    float num61 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.06f;
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
                    if (rawData.QueryHeight(vector3_2) > (double)num7 &&
                        rawData.QueryHeight(normalized3) > (double)num7 &&
                        rawData.QueryHeight(normalized4) > (double)num7 &&
                        rawData.QueryHeight(normalized5) > (double)num7)
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
                        if (rawData.QueryHeight(vpos1) > (double)num7 &&
                            rawData.QueryHeight(vpos2) > (double)num7 &&
                            rawData.QueryHeight(vpos3) > (double)num7 &&
                            rawData.QueryHeight(vpos4) > (double)num7 &&
                            rawData.QueryHeight(vpos5) > (double)num7 &&
                            rawData.QueryHeight(vpos6) > (double)num7 &&
                            rawData.QueryHeight(vpos7) > (double)num7 &&
                            rawData.QueryHeight(vpos8) > (double)num7 &&
                            rawData.QueryHeight(vpos9) > (double)num7 &&
                            rawData.QueryHeight(vpos10) > (double)num7 &&
                            (double)rawData.QueryHeight(vpos11) > num7 &&
                            rawData.QueryHeight(vpos12) > (double)num7)
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

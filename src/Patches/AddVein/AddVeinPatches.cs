using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
// ReSharper disable Unity.UnknownResource

namespace ProjectGenesis.Patches
{
    public static partial class AddVeinPatches
    {
        internal const sbyte VeinTypeCount = 19;

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
            new Color(0.965f, 0.867f, 0.352f), // Sulfur
        };

        internal static void ModifyVeinData()
        {
            AddVeinProtos(NewVein(15, "铝矿脉", "I铝矿", "Assets/texpack/铝矿脉", ProtoID.I铝矿, 25, 1, 60),
                NewVein(16, "放射性矿脉", "I放射性矿物", "Assets/texpack/放射晶体矿脉_新新", ProtoID.I放射性矿物, 35, 2, 90),
                NewVein(17, "钨矿脉", "I钨矿", "Assets/texpack/钨矿脉", ProtoID.I钨矿, 34, 1, 120),
                NewVein(18, "硫矿脉", "I硫矿", "Assets/texpack/硫矿脉_新", ProtoID.I硫矿, 36, 1, 90));
            return;

            VeinProto NewVein(int id, string name, string description, string iconPath, int miningItem, int miningEffect, int modelIndex,
                int miningTime) =>
                new VeinProto
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
                    ModelCount = 1,
                };
        }

        private static void AddVeinProtos(params VeinProto[] protos)
        {
            VeinProtoSet veins = LDB.veins;

            int dataArrayLength = veins.dataArray.Length;

            Array.Resize(ref veins.dataArray, dataArrayLength + protos.Length);

            for (var index = 0; index < protos.Length; ++index) veins.dataArray[dataArrayLength + index] = protos[index];

            veins.OnAfterDeserialize();
        }

        internal static void SetMinerMk2Color()
        {
            Texture texture = TextureHelper.GetTexture("矿机渲染索引");
            int veinColorTex = Shader.PropertyToID("_VeinColorTex");

            ref PrefabDesc prefabDesc = ref LDB.models.Select(256).prefabDesc;
            prefabDesc.materials[0].SetTexture(veinColorTex, texture);
            ref Material[] prefabDescLODMaterial = ref prefabDesc.lodMaterials[0];
            prefabDescLODMaterial[0].SetTexture(veinColorTex, texture);
            prefabDescLODMaterial[1].SetTexture(veinColorTex, texture);
            prefabDescLODMaterial[2].SetTexture(veinColorTex, texture);

            prefabDesc = ref LDB.models.Select(59).prefabDesc;
            prefabDescLODMaterial = ref prefabDesc.lodMaterials[0];
            prefabDescLODMaterial[1].SetTexture(veinColorTex, texture);
            prefabDescLODMaterial[2].SetTexture(veinColorTex, texture);
            prefabDescLODMaterial[3].SetTexture(veinColorTex, texture);
        }

        [HarmonyPatch(typeof(VeinProto), nameof(VeinProto.Preload))]
        [HarmonyPostfix]
        public static void VeinProto_Preload_Postfix(VeinProto __instance)
        {
            if (__instance._iconSprite80px == null) __instance._iconSprite80px = __instance._iconSprite;
        }

        [HarmonyPatch(typeof(UISandboxMenu), nameof(UISandboxMenu.StaticLoad))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISandboxMenu_StaticLoad_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(LDB), nameof(LDB.veins))), new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldlen));

            matcher.Advance(1).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)14))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISandboxMenu), nameof(UISandboxMenu.StaticLoad))]
        [HarmonyPostfix]
        public static void UISandboxMenu_StaticLoad_Postfix(ref VeinProto[,] ___veinProtos)
        {
            ___veinProtos[1, 7] = LDB.veins.Select(15);
            ___veinProtos[1, 8] = LDB.veins.Select(16);
        }

        [HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm7), nameof(PlanetAlgorithm7.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm11), nameof(PlanetAlgorithm11.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm12), nameof(PlanetAlgorithm12.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm13), nameof(PlanetAlgorithm13.GenerateVeins))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetAlgorithm_GenerateVeins_ResizeVeinList_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15));
            matcher.SetOperandAndAdvance(19);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm7), nameof(PlanetAlgorithm7.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm11), nameof(PlanetAlgorithm11.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm12), nameof(PlanetAlgorithm12.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm13), nameof(PlanetAlgorithm13.GenerateVeins))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetAlgorithm_GenerateVeins_RemoveVeinPositionBias_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_7));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetOpcodeAndAdvance(OpCodes.Br_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm11), nameof(PlanetAlgorithm11.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm12), nameof(PlanetAlgorithm12.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm13), nameof(PlanetAlgorithm13.GenerateVeins))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetAlgorithm_GenerateVeins_RemoveHeightLimit_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.radius))),
                new CodeMatch
                {
                    opcodes = new List<OpCode>
                    {
                        OpCodes.Blt, OpCodes.Blt_S,
                    },
                });

            CodeMatcher matcher2 = matcher.Clone();
            matcher2.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_0), new CodeMatch(OpCodes.Stloc_S));
            Label label = matcher2.Labels.First();

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Pop));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Pop));
            matcher.SetAndAdvance(OpCodes.Br, label);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.waterItemId))));
            matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Br);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm11), nameof(PlanetAlgorithm11.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm12), nameof(PlanetAlgorithm12.GenerateVeins))]
        [HarmonyPatch(typeof(PlanetAlgorithm13), nameof(PlanetAlgorithm13.GenerateVeins))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetAlgorithm_InitialVeins_Transpiler(IEnumerable<CodeInstruction> instructions,
            MethodBase original)
        {
            var matcher = new CodeMatcher(instructions);

            Type type = original.DeclaringType;

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(type, "veinVectorCount")));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddVeinPatches), nameof(InitBirthVeinVectors))));

            matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Ldc_I4_5);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_2));

            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_0);

            return matcher.InstructionEnumeration();
        }

        public static void InitBirthVeinVectors(PlanetAlgorithm algorithm)
        {
            algorithm.veinVectorTypes[2] = EVeinType.Aluminum;
            algorithm.veinVectors[2] = algorithm.planet.birthResourcePoint2;
            algorithm.veinVectorTypes[3] = EVeinType.Coal;
            algorithm.veinVectors[3] = algorithm.planet.birthResourcePoint3;
            algorithm.veinVectorTypes[4] = EVeinType.Stone;
            algorithm.veinVectors[4] = algorithm.planet.birthResourcePoint4;
        }

        internal static Vector2 Rotate(Vector2 v, float angle)
        {
            float delta = angle * Mathf.PI / 180;

            return new Vector2(v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta), v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta));
        }

        [HarmonyPatch(typeof(PlanetData), nameof(PlanetData.GenBirthPoints), typeof(PlanetRawData), typeof(int))]
        [HarmonyPostfix]
        public static void PlanetData_GenBirthPoints_Postfix(PlanetData __instance, PlanetRawData rawData, int _birthSeed)
        {
            var dotNet35Random = new DotNet35Random(_birthSeed);
            Pose pose = __instance.PredictPose(85.0);
            Vector3 vector3 = Maths.QInvRotateLF(pose.rotation, __instance.star.uPosition - (VectorLF3)pose.position * 40000.0);
            vector3.Normalize();
            Vector3 x_direction = Vector3.Cross(vector3, Vector3.up).normalized;
            Vector3 y_direction = Vector3.Cross(x_direction, vector3).normalized;
            var num1 = 0;
            const int num2 = 512;

            for (; num1 < num2; ++num1)
            {
                float num3 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.5f;
                float num4 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.5f;
                Vector3 random = vector3 + num3 * x_direction + num4 * y_direction;
                random.Normalize();
                __instance.birthPoint = random * (float)(__instance.realRadius + 0.20000000298023224 + 1.4500000476837158);
                var tmpVector3 = Vector3.Cross(random, Vector3.up);
                x_direction = tmpVector3.normalized;
                tmpVector3 = Vector3.Cross(x_direction, random);
                y_direction = tmpVector3.normalized;

                for (var index = 0; index < 10; ++index)
                {
                    Vector2 rotate_0 = new Vector2((float)(dotNet35Random.NextDouble() * 2.0 - 1.0),
                        (float)(dotNet35Random.NextDouble() * 2.0 - 1.0)).normalized * 0.1f;
                    Vector2 rotate_1 = Rotate(rotate_0, 120);
                    Modify(dotNet35Random, ref rotate_1);
                    Vector2 rotate_2 = Rotate(rotate_0, 240);
                    Modify(dotNet35Random, ref rotate_2);
                    Vector2 rotate_3 = Rotate(rotate_0, 60);
                    Modify(dotNet35Random, ref rotate_3);

                    tmpVector3 = random + rotate_0.x * x_direction + rotate_0.y * y_direction;
                    __instance.birthResourcePoint0 = tmpVector3.normalized;

                    tmpVector3 = random + rotate_1.x * x_direction + rotate_1.y * y_direction;
                    __instance.birthResourcePoint1 = tmpVector3.normalized;

                    tmpVector3 = random + rotate_2.x * x_direction + rotate_2.y * y_direction;
                    __instance.birthResourcePoint2 = tmpVector3.normalized;

                    tmpVector3 = random + rotate_0.x * -2 * x_direction + rotate_0.y * -2 * y_direction;
                    __instance.birthResourcePoint3 = tmpVector3.normalized;

                    tmpVector3 = random + rotate_3.x * 2 * x_direction + rotate_3.y * 2 * y_direction;
                    __instance.birthResourcePoint4 = tmpVector3.normalized;

                    if (QueryHeightsNear(rawData, x_direction, y_direction, __instance.realRadius, random, __instance.birthResourcePoint0,
                        __instance.birthResourcePoint1, __instance.birthResourcePoint2, __instance.birthResourcePoint3,
                        __instance.birthResourcePoint4))
                        return;
                }
            }
        }

        private static void Modify(DotNet35Random dotNet35Random, ref Vector2 vector)
        {
            vector.x += (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.006f;
            vector.y += (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.006f;
        }

        private static bool QueryHeights(PlanetRawData rawData, float radius, params Vector3[] points) =>
            points.All(point => rawData.QueryHeight(point) > radius);

        private static bool QueryHeightsNear(PlanetRawData rawData, Vector3 x_vector, Vector3 y_vector, float radius,
            params Vector3[] points)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Vector3 point in points)
            {
                Vector3 pos1 = point + x_vector * 0.03f;
                Vector3 pos2 = point - x_vector * 0.03f;
                Vector3 pos3 = point + y_vector * 0.03f;
                Vector3 pos4 = point - y_vector * 0.03f;

                if (!QueryHeights(rawData, radius, point, pos1, pos2, pos3, pos4)) return false;
            }

            return true;
        }
    }
}

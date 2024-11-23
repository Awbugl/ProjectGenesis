using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace ProjectGenesis.Patches.UI
{
    public static class AdvancedLaserPatches
    {
        private static readonly DataPoolRenderer<LocalLaserContinuous> _turretAdvancedLaserContinuous =
            new DataPoolRenderer<LocalLaserContinuous>();

        private static RenderableObjectDesc _combatTurretAdvancedLaserContinuousDesc;

        private static readonly FieldInfo TurretComponent_projectileId_Field =
            AccessTools.Field(typeof(TurretComponent), nameof(TurretComponent.projectileId));

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.Init))]
        [HarmonyPostfix]
        public static void SkillSystem_Init()
        {
            if (!_combatTurretAdvancedLaserContinuousDesc)
            {
                RenderableObjectDesc laserContinuousDesc = Configs.combat.turretLaserContinuousDesc;
                _combatTurretAdvancedLaserContinuousDesc = laserContinuousDesc.gameObject.AddComponent<RenderableObjectDesc>();
                _combatTurretAdvancedLaserContinuousDesc.gpuWorkType = EGpuWorkEntry.Skill;
                _combatTurretAdvancedLaserContinuousDesc.meshProcedured = laserContinuousDesc.meshProcedured;

                var material = new Material(laserContinuousDesc.materials[0]);
                var color = new Color(0.8471f, 0.6078f, 1f);
                material.SetColor("_BeamColor1", color);
                material.SetColor("_MuzzleWaveColor", color);
                material.SetColor("_PulseColor", color);
                material.SetColor("_ShortWaveColor", color);
                color = new Color(0.6118f, 0.0f, 1f);
                material.SetColor("_BeamColor2", color);
                color = new Color(0.7922f, 0.4627f, 1f);
                material.SetColor("_LongWaveColor", color);
                color = new Color(0.7569f, 0.4078f, 1f);
                material.SetColor("_MuzzleFlareColor", color);
                _combatTurretAdvancedLaserContinuousDesc.materials = new Material[] { material, };
            }

            _turretAdvancedLaserContinuous.InitRenderer(_combatTurretAdvancedLaserContinuousDesc);
        }

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.StopContinuousSkill))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TurretComponent_StopContinuousSkill_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                new CodeMatch(OpCodes.Ldc_I4_0));
            matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Beq_S);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SkillSystem), nameof(SkillSystem.turretLaserContinuous))));

            matcher.Advance(1).CreateLabel(out Label label1);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Br, label1));
            matcher.Insert(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_Result_Method))));
            matcher.CreateLabel(out Label label2);
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Set_ProjectileId_Method))),
                new CodeInstruction(OpCodes.Stfld, TurretComponent_projectileId_Field));

            matcher.Advance(-9).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, TurretComponent_projectileId_Field), new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Blt, label2));

            return matcher.InstructionEnumeration();
        }

        public static int Set_ProjectileId_Method(int index) => -index;

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.Shoot_Laser))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TurretComponent_Shoot_Laser_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.skillSystem))),
                new CodeMatch(OpCodes.Ldfld));

            matcher.Advance(1).CreateLabel(out Label label1);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Br, label1));
            matcher.Insert(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_Result_Method))));
            matcher.CreateLabel(out Label label2);

            matcher.Advance(-3).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_Condition_Method))),
                new CodeInstruction(OpCodes.Brtrue, label2), new CodeInstruction(OpCodes.Ldarg_1));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                new CodeMatch(OpCodes.Br), new CodeMatch(OpCodes.Ldloc_S));

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_ProjectileId_Method))));

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Stfld, TurretComponent_projectileId_Field));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_ProjectileId_Method))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.InternalUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                new CodeMatch(OpCodes.Ldc_I4_0));
            matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Beq_S);

            return matcher.InstructionEnumeration();
        }

        private static bool Patch_Condition_Method(PlanetFactory factory, ref TurretComponent component) =>
            factory.entityPool[component.entityId].modelIndex == ProtoID.M高频激光塔MK2;

        public static int Patch_ProjectileId_Method(int index, PlanetFactory factory, ref TurretComponent component) =>
            Patch_Condition_Method(factory, ref component) ? -index : index;

        public static DataPoolRenderer<LocalLaserContinuous> Patch_Result_Method() => _turretAdvancedLaserContinuous;

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.GameTick))]
        [HarmonyPostfix]
        public static void SkillSystem_GameTick(SkillSystem __instance)
        {
            PlanetFactory[] factories = __instance.gameData.factories;
            int cursor = _turretAdvancedLaserContinuous.cursor;
            LocalLaserContinuous[] buffer = _turretAdvancedLaserContinuous.buffer;

            for (var id = 1; id < cursor; ++id)
            {
                if (buffer[id].id != id) continue;

                buffer[id].TickSkillLogic(__instance, factories);

                if (buffer[id].fade == 0.0) _turretAdvancedLaserContinuous.Remove(id);
            }

            if (_turretAdvancedLaserContinuous.count != 0 || _turretAdvancedLaserContinuous.capacity <= 256) return;

            _turretAdvancedLaserContinuous.Flush();
        }

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.RendererDraw))]
        [HarmonyPostfix]
        public static void SkillSystem_RendererDraw(SkillSystem __instance)
        {
            if (__instance.gameData.localPlanet == null) return;

            _turretAdvancedLaserContinuous.Render();
        }

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.Free))]
        [HarmonyPostfix]
        public static void SkillSystem_Free()
        {
            if (_turretAdvancedLaserContinuous == null) return;

            _turretAdvancedLaserContinuous.FreeRenderer();
        }

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.SetForNewGame))]
        [HarmonyPostfix]
        public static void SkillSystem_SetForNewGame() => ReInitAll();

        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.UpgradeEntityWithComponents))]
        [HarmonyPostfix]
        public static void PlanetFactory_UpgradeEntityWithComponents_Postfix(PlanetFactory __instance, int entityId, ItemProto newProto)
        {
            if (entityId == 0 || __instance.entityPool[entityId].id == 0) return;

            int turretId = __instance.entityPool[entityId].turretId;

            if (turretId <= 0) return;

            if (newProto.ModelIndex != ProtoID.M高频激光塔MK2 && newProto.ModelIndex != ProtoID.M高频激光塔) return;

            PlanetFactory planetFactory = __instance.planet.factory;

            planetFactory.defenseSystem.turrets.buffer[turretId].StopContinuousSkill(planetFactory.skillSystem);
        }


        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try { _turretAdvancedLaserContinuous.Import(r); }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void Export(BinaryWriter w)
        {
            lock (_turretAdvancedLaserContinuous)
            {
                _turretAdvancedLaserContinuous.Flush();
                _turretAdvancedLaserContinuous.Export(w);
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll() => _turretAdvancedLaserContinuous.ResetPool();
    }
}

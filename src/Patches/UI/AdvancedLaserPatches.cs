using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace ProjectGenesis.Patches
{
    public static class AdvancedLaserPatches
    {
        private static readonly DataPoolRenderer<LocalLaserContinuous> _turretAdvancedLaserContinuous =
            new DataPoolRenderer<LocalLaserContinuous>();

        private static RenderableObjectDesc _combatTurretAdvancedLaserContinuousDesc;

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

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SkillSystem), nameof(SkillSystem.turretLaserContinuous))),
                new CodeMatch(OpCodes.Stloc_0));

            matcher.Advance(1).SetAndAdvance(OpCodes.Ldarg_0, null);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(AdvancedLaserPatches), nameof(GetLaserContinuousSkillSystem))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.Shoot_Laser))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TurretComponent_Shoot_Laser_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.skillSystem))),
                new CodeMatch(OpCodes.Stloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Stloc_S));

            matcher.Advance(3).SetAndAdvance(OpCodes.Ldarg_1, null).SetAndAdvance(OpCodes.Ldarg_0, null);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(AdvancedLaserPatches), nameof(GetLaserContinuousPlanetFactory))));

            return matcher.InstructionEnumeration();
        }

        private static bool CheckAdvancedLaser(PlanetFactory factory, ref TurretComponent component) =>
            factory.entityPool[component.entityId].modelIndex == ProtoID.M高频激光塔MK2;

        public static DataPoolRenderer<LocalLaserContinuous>
            GetLaserContinuousPlanetFactory(PlanetFactory factory, ref TurretComponent component) =>
            CheckAdvancedLaser(factory, ref component) ? _turretAdvancedLaserContinuous : factory.skillSystem.turretLaserContinuous;

        public static DataPoolRenderer<LocalLaserContinuous> GetLaserContinuousSkillSystem(SkillSystem skillSystem,
            ref TurretComponent component)
        {
            PlanetFactory factory = skillSystem.gameData.factories[component.target.astroId];
            return GetLaserContinuousPlanetFactory(factory, ref component);
        }

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

            planetFactory.defenseSystem.turrets.buffer[turretId].StopContinuousSkill(planetFactory.skillSystem, false);
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

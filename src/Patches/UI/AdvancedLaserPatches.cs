using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        public static readonly DataPoolRenderer<LocalLaserContinuous> TurretAdvancedLaserContinuous =
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

            TurretAdvancedLaserContinuous.InitRenderer(_combatTurretAdvancedLaserContinuousDesc);
        }

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.StopContinuousSkill))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TurretComponent_StopContinuousSkill_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            // change condition from projectileId>0 to projectileId!=0
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                new CodeMatch(OpCodes.Ldc_I4_0));
            matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Beq_S);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SkillSystem), nameof(SkillSystem.turretLaserContinuous))));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(AdvancedLaserPatches), nameof(GetLaserContinuousByProjectileId))));

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field));
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Abs))));

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field));
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Abs))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.Shoot_Laser))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TurretComponent_Shoot_Laser_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            // this part does:
            // turretLaserContinuous = IsAdvancedLaser ? _turretAdvancedLaserContinuous : skillSystem.turretLaserContinuous;
            /*
                ldarg.1      // factory
                ldfld        PlanetFactory.skillSystem
                stloc.s      skillSystem
                ldarg.1      // factory
                Ldarg_0      // TurretComponent&
                Call IsAdvancedLaser
                Brtrue label2
                ldloc.s      skillSystem
                ldfld        SkillSystem.turretLaserContinuous
                br label1
                Call GetAdvancedLaserContinuous [label2]
                stloc.s      turretLaserContinuous [label1]
            */
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.skillSystem))),
                new CodeMatch(OpCodes.Stloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldfld));

            matcher.Advance(1).CreateLabel(out Label label1);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Br, label1));
            matcher.Insert(new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(AdvancedLaserPatches), nameof(TurretAdvancedLaserContinuous))));
            matcher.CreateLabel(out Label label2);

            matcher.Advance(-3).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AdvancedLaserPatches), nameof(IsAdvancedLaser))),
                new CodeInstruction(OpCodes.Brtrue, label2));

            // change condition from projectileId>0 to projectileId>0 (normal) || projectileId<0 (advanced)
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                new CodeMatch(OpCodes.Brtrue), new CodeMatch(OpCodes.Ldloc_S));

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Abs))));

            // patch when shot if TurretComponent is advanced then set TurretComponent.ProjectileId = -TurretComponent.ProjectileId
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                new CodeMatch(OpCodes.Ldelema), new CodeMatch(OpCodes.Dup));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AdvancedLaserPatches), nameof(RewriteProjectileIdIfAdvanced))));

            // patch when get TurretComponent.ProjectileId back to -TurretComponent.ProjectileId if advanced 
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                new CodeMatch(OpCodes.Ldelema), new CodeMatch(OpCodes.Dup));

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Abs))));

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

        private static bool IsAdvancedLaser(PlanetFactory factory, ref TurretComponent component) =>
            factory.entityPool[component.entityId].modelIndex == ProtoID.M高频激光塔MK2;

        public static int Abs(int index) => index >= 0 ? index : -index;

        public static void RewriteProjectileIdIfAdvanced(PlanetFactory factory, ref TurretComponent component)
        {
            if (IsAdvancedLaser(factory, ref component)) component.projectileId = -component.projectileId;
        }

        public static DataPoolRenderer<LocalLaserContinuous> GetLaserContinuousByProjectileId(DataPoolRenderer<LocalLaserContinuous> ori,
            ref TurretComponent component) =>
            component.projectileId >= 0 ? ori : TurretAdvancedLaserContinuous;

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.GameTick))]
        [HarmonyPostfix]
        public static void SkillSystem_GameTick(SkillSystem __instance)
        {
            PlanetFactory[] factories = __instance.gameData.factories;
            int cursor = TurretAdvancedLaserContinuous.cursor;
            LocalLaserContinuous[] buffer = TurretAdvancedLaserContinuous.buffer;

            for (var id = 1; id < cursor; ++id)
            {
                if (buffer[id].id != id) continue;

                buffer[id].TickSkillLogic(__instance, factories);
                if (buffer[id].fade == 0.0) TurretAdvancedLaserContinuous.Remove(id);
            }

            if (TurretAdvancedLaserContinuous.count == 0 && TurretAdvancedLaserContinuous.capacity > 256)
                TurretAdvancedLaserContinuous.Flush();
        }

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.RendererDraw))]
        [HarmonyPostfix]
        public static void SkillSystem_RendererDraw(SkillSystem __instance)
        {
            if (__instance.gameData.localPlanet == null) return;

            TurretAdvancedLaserContinuous.Render();
        }

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.Free))]
        [HarmonyPostfix]
        public static void SkillSystem_Free() => TurretAdvancedLaserContinuous?.FreeRenderer();

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

            try { TurretAdvancedLaserContinuous.Import(r); }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void Export(BinaryWriter w)
        {
            lock (TurretAdvancedLaserContinuous)
            {
                TurretAdvancedLaserContinuous.Flush();
                TurretAdvancedLaserContinuous.Export(w);
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll() => TurretAdvancedLaserContinuous.ResetPool();
    }
}

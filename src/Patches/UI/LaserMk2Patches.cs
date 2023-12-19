using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace ProjectGenesis.Patches.UI
{
    public static class LaserMk2Patches
    {
        private static DataPoolRenderer<LocalLaserContinuous> _turretLaserMk2Continuous;
        private static RenderableObjectDesc _combatTurretLaserMk2ContinuousDesc;

        [HarmonyPatch(typeof(SkillSystem), "Init")]
        [HarmonyPostfix]
        public static void SkillSystem_Init()
        {
            RenderableObjectDesc turretLaserContinuousDesc = Configs.combat.turretLaserContinuousDesc;
            _combatTurretLaserMk2ContinuousDesc = turretLaserContinuousDesc.gameObject.AddComponent<RenderableObjectDesc>();

            _combatTurretLaserMk2ContinuousDesc.gpuWorkType = EGpuWorkEntry.Skill;
            _combatTurretLaserMk2ContinuousDesc.meshProcedured = turretLaserContinuousDesc.meshProcedured;

            var material = new Material(turretLaserContinuousDesc.materials[0]);

            var value = new Color(0.8471f, 0.6078f, 1.0000f);
            material.SetColor("_BeamColor1", value);
            material.SetColor("_MuzzleWaveColor", value);
            material.SetColor("_PulseColor", value);
            material.SetColor("_ShortWaveColor", value);

            value = new Color(0.6118f, 0.0000f, 1.0000f);
            material.SetColor("_BeamColor2", value);

            value = new Color(0.7922f, 0.4627f, 1.0000f);
            material.SetColor("_LongWaveColor", value);

            value = new Color(0.7569f, 0.4078f, 1.0000f);
            material.SetColor("_MuzzleFlareColor", value);

            _combatTurretLaserMk2ContinuousDesc.materials = new[] { material };

            _turretLaserMk2Continuous = new DataPoolRenderer<LocalLaserContinuous>();
            _turretLaserMk2Continuous.InitRenderer(_combatTurretLaserMk2ContinuousDesc);
        }

        [HarmonyPatch(typeof(TurretComponent), "StopContinuousSkill")]
        [HarmonyPrefix]
        public static bool TurretComponent_StopContinuousSkill(TurretComponent __instance, SkillSystem skillSystem)
        {
            if (__instance.projectileId <= 0) return false;

            if (__instance.type != ETurretType.Laser) return true;

            ref LocalLaserContinuous local = ref _turretLaserMk2Continuous.buffer[__instance.projectileId];

            if (local.id == __instance.projectileId)
            {
                local.Stop(skillSystem);
                __instance.projectileId = 0;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(TurretComponent), "Shoot_Laser")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TurretComponent_Shoot_Laser_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator).MatchForward(true, new CodeMatch(OpCodes.Ldarg_1),
                                                                                        new CodeMatch(OpCodes.Ldfld,
                                                                                                      AccessTools.Field(typeof(PlanetFactory),
                                                                                                                        nameof(PlanetFactory
                                                                                                                                  .skillSystem))),
                                                                                        new CodeMatch(OpCodes.Ldfld));

            matcher.Advance(1).CreateLabelAt(matcher.Pos, out Label label2);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Br, label2));
            matcher.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LaserMk2Patches), nameof(Patch_Result_Method))));
            matcher.CreateLabelAt(matcher.Pos, out Label label1);

            matcher.Advance(-3).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                 new CodeInstruction(OpCodes.Call,
                                                                     AccessTools.Method(typeof(LaserMk2Patches), nameof(Patch_Condition_Method))),
                                                 new CodeInstruction(OpCodes.Brtrue, label1), new CodeInstruction(OpCodes.Ldarg_1));

            return matcher.InstructionEnumeration();
        }

        public static bool Patch_Condition_Method(PlanetFactory factory, ref TurretComponent component)
        {
            EntityData data = factory.entityPool[component.entityId];
            return data.modelIndex == ProtoIDUsedByPatches.M高频激光塔MK2;
        }

        public static DataPoolRenderer<LocalLaserContinuous> Patch_Result_Method() => _turretLaserMk2Continuous;

        [HarmonyPatch(typeof(SkillSystem), "GameTick")]
        [HarmonyPostfix]
        public static void SkillSystem_GameTick(SkillSystem __instance)
        {
            int cursor30 = _turretLaserMk2Continuous.cursor;
            LocalLaserContinuous[] buffer30 = _turretLaserMk2Continuous.buffer;
            for (int id = 1; id < cursor30; ++id)
            {
                if (buffer30[id].id == id)
                {
                    buffer30[id].TickSkillLogic(__instance, __instance.gameData.factories);
                    if (buffer30[id].fade == 0.0) _turretLaserMk2Continuous.Remove(id);
                }
            }

            if (_turretLaserMk2Continuous.count == 0 && _turretLaserMk2Continuous.capacity > 256) _turretLaserMk2Continuous.Flush();
        }

        [HarmonyPatch(typeof(SkillSystem), "RendererDraw")]
        [HarmonyPostfix]
        public static void SkillSystem_RendererDraw(SkillSystem __instance)
        {
            if (__instance.gameData.localPlanet == null) return;
            _turretLaserMk2Continuous.Render();
        }

        [HarmonyPatch(typeof(SkillSystem), "Free")]
        [HarmonyPostfix]
        public static void SkillSystem_Free()
        {
            if (_turretLaserMk2Continuous != null)
            {
                _turretLaserMk2Continuous.FreeRenderer();
                _turretLaserMk2Continuous = null;
            }
        }

        [HarmonyPatch(typeof(SkillSystem), "SetForNewGame")]
        [HarmonyPostfix]
        public static void SkillSystem_SetForNewGame() => _turretLaserMk2Continuous.ResetPool();

        internal static void Import(BinaryReader r)
        {
            SkillSystem_SetForNewGame();
            _turretLaserMk2Continuous.Import(r);
        }

        internal static void Export(BinaryWriter w) => _turretLaserMk2Continuous.Export(w);

        internal static void IntoOtherSave() => SkillSystem_SetForNewGame();
    }
}

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
        private static DataPoolRenderer<LocalLaserContinuous> _turretAdvancedLaserContinuous = new DataPoolRenderer<LocalLaserContinuous>();
        private static RenderableObjectDesc _combatTurretAdvancedLaserContinuousDesc;

        private static readonly FieldInfo TurretComponent_projectileId_Field = AccessTools.Field(typeof(TurretComponent), "projectileId");

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.Init))]
        [HarmonyPostfix]
        public static void SkillSystem_Init()
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
            _combatTurretAdvancedLaserContinuousDesc.materials = new Material[] { material };
            _turretAdvancedLaserContinuous.InitRenderer(_combatTurretAdvancedLaserContinuousDesc);
        }

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.StopContinuousSkill))]
        [HarmonyPrefix]
        public static void TurretComponent_StopContinuousSkill(ref TurretComponent __instance, SkillSystem skillSystem)
        {
            if (__instance.projectileId >= 0 || __instance.type != ETurretType.Laser) return;

            int index = -__instance.projectileId;

            ref LocalLaserContinuous local = ref _turretAdvancedLaserContinuous.buffer[index];

            if (local.id != index)
            {
                ProjectGenesis.LogInfo("local.id != projectileId ! local.id: " + local.id + "projectileId: " + index);
                return;
            }

            local.Stop(skillSystem);
            __instance.projectileId = 0;
        }

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.Shoot_Laser))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TurretComponent_Shoot_Laser_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, generator).MatchForward(
                true, new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.skillSystem))),
                new CodeMatch(OpCodes.Ldfld));

            codeMatcher.Advance(1).CreateLabelAt(codeMatcher.Pos, out Label label1);

            codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Br, label1));
            codeMatcher.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_Result_Method))));
            codeMatcher.CreateLabelAt(codeMatcher.Pos, out Label label2);

            codeMatcher.Advance(-3).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                     new CodeInstruction(
                                                         OpCodes.Call,
                                                         AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_Condition_Method))),
                                                     new CodeInstruction(OpCodes.Brtrue, label2), new CodeInstruction(OpCodes.Ldarg_1));

            codeMatcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                                     new CodeMatch(OpCodes.Br), new CodeMatch(OpCodes.Ldloc_S));

            codeMatcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_0),
                                                    new CodeInstruction(
                                                        OpCodes.Call,
                                                        AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_ProjectileId_Method))));

            codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldloc_S),
                                     new CodeMatch(OpCodes.Stfld, TurretComponent_projectileId_Field));

            codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_0),
                                         new CodeInstruction(
                                             OpCodes.Call, AccessTools.Method(typeof(AdvancedLaserPatches), nameof(Patch_ProjectileId_Method))));

            return codeMatcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(TurretComponent), nameof(TurretComponent.InternalUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                                     new CodeMatch(OpCodes.Ldc_I4_0));
            codeMatcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Beq_S);
            return codeMatcher.InstructionEnumeration();
        }

        private static bool Patch_Condition_Method(PlanetFactory factory, ref TurretComponent component)
            => factory.entityPool[component.entityId].modelIndex == ProtoID.M高频激光塔MK2;

        public static int Patch_ProjectileId_Method(int index, PlanetFactory factory, ref TurretComponent component)
            => Patch_Condition_Method(factory, ref component) ? -index : index;

        public static DataPoolRenderer<LocalLaserContinuous> Patch_Result_Method() => _turretAdvancedLaserContinuous;

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.GameTick))]
        [HarmonyPostfix]
        public static void SkillSystem_GameTick(SkillSystem __instance)
        {
            PlanetFactory[] factories = __instance.gameData.factories;
            int cursor = _turretAdvancedLaserContinuous.cursor;
            LocalLaserContinuous[] buffer = _turretAdvancedLaserContinuous.buffer;
            for (int id = 1; id < cursor; ++id)
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
            _turretAdvancedLaserContinuous = null;
        }

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.SetForNewGame))]
        [HarmonyPostfix]
        public static void SkillSystem_SetForNewGame() => _turretAdvancedLaserContinuous.ResetPool();

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                _turretAdvancedLaserContinuous.Import(r);
            }
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

        private static void ReInitAll()
        {
            _turretAdvancedLaserContinuous = new DataPoolRenderer<LocalLaserContinuous>();
            _turretAdvancedLaserContinuous.ResetPool();
        }
    }
}

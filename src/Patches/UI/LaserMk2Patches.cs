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
    public static class LaserMk2Patches
    {
        public static DataPoolRenderer<LocalLaserContinuous> turretLaserMk2Continuous;
        private static RenderableObjectDesc _combatTurretLaserMk2ContinuousDesc;

        private static readonly FieldInfo TurretComponent_projectileId_Field
            = AccessTools.Field(typeof(TurretComponent), nameof(TurretComponent.projectileId));

        [HarmonyPatch(typeof(SkillSystem), "Init")]
        [HarmonyPostfix]
        public static void SkillSystem_Init()
        {
            RenderableObjectDesc turretLaserContinuousDesc = Configs.combat.turretLaserContinuousDesc;
            _combatTurretLaserMk2ContinuousDesc = turretLaserContinuousDesc.gameObject.AddComponent<RenderableObjectDesc>();

            _combatTurretLaserMk2ContinuousDesc.gpuWorkType = EGpuWorkEntry.Skill;
            _combatTurretLaserMk2ContinuousDesc.castShadow = new[] { false };
            _combatTurretLaserMk2ContinuousDesc.meshProcedured = turretLaserContinuousDesc.meshProcedured;
            _combatTurretLaserMk2ContinuousDesc.rendererName = turretLaserContinuousDesc.rendererName;

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

            turretLaserMk2Continuous = new DataPoolRenderer<LocalLaserContinuous>();
            turretLaserMk2Continuous.InitRenderer(_combatTurretLaserMk2ContinuousDesc);
        }

        [HarmonyPatch(typeof(TurretComponent), "StopContinuousSkill")]
        [HarmonyPrefix]
        public static bool TurretComponent_StopContinuousSkill(TurretComponent __instance, SkillSystem skillSystem)
        {
            if (__instance.projectileId == 0) return false;
            if (__instance.projectileId > 0) return true;
            if (__instance.type != ETurretType.Laser) return true;

            int projectileId = -__instance.projectileId;

            ref LocalLaserContinuous local = ref turretLaserMk2Continuous.buffer[projectileId];

            if (local.id != projectileId) return true;

            local.Stop(skillSystem);
            __instance.projectileId = 0;
            return false;
        }

        [HarmonyPatch(typeof(TurretComponent), "Shoot_Laser")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TurretComponent_Shoot_Laser_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            /*
  IL_00f3: ldarg.1      // factory
  IL_00f4: ldfld        class SkillSystem PlanetFactory::skillSystem
  IL_00f9: ldfld        class DataPoolRenderer`1<valuetype LocalLaserContinuous> SkillSystem::turretLaserContinuous
  IL_00fe: stloc.s      turretLaserContinuous

  // [2256 11 - 2256 52]
  IL_0100: ldarg.0      // this
  IL_0101: ldfld        int32 TurretComponent::projectileId
  IL_0106: ldc.i4.0
  IL_0107: ceq
  IL_0109: dup

  // [2257 11 - 2257 85]
  IL_010a: brtrue.s     IL_0114

  IL_010c: ldarg.0      // this
  IL_010d: ldfld        int32 TurretComponent::projectileId

  ldarg.1 ldarg.0 call (return -TurretComponent::projectileId)

  IL_0112: br.s         IL_0120
  IL_0114: ldloc.s      turretLaserContinuous
  IL_0116: callvirt     instance !0/*valuetype LocalLaserContinuous* /& class DataPoolRenderer`1<valuetype LocalLaserContinuous>::Add()
  IL_011b: ldfld        int32 LocalLaserContinuous::id
  IL_0120: stloc.s      index

  // [2258 11 - 2258 36]
  IL_0122: ldarg.0      // this
  IL_0123: ldloc.s      index

  ldarg.1 ldarg.0 call (return -TurretComponent::projectileId)

  IL_0125: stfld        int32 TurretComponent::projectileId

  // [2259 11 - 2259 84]
  IL_012a: ldloc.s      turretLaserContinuous
  IL_012c: ldfld        !0/*valuetype LocalLaserContinuous* /[] class DataPoolRenderer`1<valuetype LocalLaserContinuous>::buffer
  IL_0131: ldloc.s      index
  IL_0133: ldelema      LocalLaserContinuous
  IL_0138: stloc.s      local3
             *
             */

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


            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                                 new CodeMatch(OpCodes.Br), new CodeMatch(OpCodes.Ldloc_S));

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(LaserMk2Patches), nameof(Patch_ProjectileId_Method))));

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldloc_S),
                                 new CodeMatch(OpCodes.Stfld, TurretComponent_projectileId_Field));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Call,
                                                         AccessTools.Method(typeof(LaserMk2Patches), nameof(Patch_ProjectileId_Method))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(TurretComponent), "InternalUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, TurretComponent_projectileId_Field),
                                 new CodeMatch(OpCodes.Ldc_I4_0));

            matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Beq_S);

            return matcher.InstructionEnumeration();
        }

        public static bool Patch_Condition_Method(PlanetFactory factory, ref TurretComponent component)
        {
            EntityData data = factory.entityPool[component.entityId];
            return data.modelIndex == ProtoIDUsedByPatches.M高频激光塔MK2;
        }

        public static int Patch_ProjectileId_Method(int index, PlanetFactory factory, ref TurretComponent component)
            => Patch_Condition_Method(factory, ref component) ? -index : index;

        public static DataPoolRenderer<LocalLaserContinuous> Patch_Result_Method() => turretLaserMk2Continuous;

        [HarmonyPatch(typeof(SkillSystem), "GameTick")]
        [HarmonyPostfix]
        public static void SkillSystem_GameTick(SkillSystem __instance)
        {
            PlanetFactory[] factories = __instance.gameData.factories;

            int cursor30 = turretLaserMk2Continuous.cursor;
            LocalLaserContinuous[] buffer30 = turretLaserMk2Continuous.buffer;
            for (int id = 1; id < cursor30; ++id)
            {
                if (buffer30[id].id == id)
                {
                    buffer30[id].TickSkillLogic(__instance, factories);
                    if (buffer30[id].fade == 0.0) turretLaserMk2Continuous.Remove(id);
                }
            }

            if (turretLaserMk2Continuous.count == 0 && turretLaserMk2Continuous.capacity > 256) turretLaserMk2Continuous.Flush();
        }

        [HarmonyPatch(typeof(SkillSystem), "RendererDraw")]
        [HarmonyPostfix]
        public static void SkillSystem_RendererDraw(SkillSystem __instance)
        {
            if (__instance.gameData.localPlanet == null) return;
            turretLaserMk2Continuous.Render();
        }

        [HarmonyPatch(typeof(SkillSystem), "Free")]
        [HarmonyPostfix]
        public static void SkillSystem_Free()
        {
            if (turretLaserMk2Continuous != null)
            {
                turretLaserMk2Continuous.FreeRenderer();
                turretLaserMk2Continuous = null;
            }
        }

        [HarmonyPatch(typeof(SkillSystem), "SetForNewGame")]
        [HarmonyPostfix]
        public static void SkillSystem_SetForNewGame() => turretLaserMk2Continuous.ResetPool();

        internal static void Import(BinaryReader r)
        {
            SkillSystem_SetForNewGame();
            turretLaserMk2Continuous.Import(r);
        }

        internal static void Export(BinaryWriter w) => turretLaserMk2Continuous.Export(w);

        internal static void IntoOtherSave() => SkillSystem_SetForNewGame();
    }
}

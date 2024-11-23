using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static partial class AddVeinPatches
    {
        private static readonly Dictionary<int, Color> Map = new Dictionary<int, Color>();

        private static ParticleSystem NewParticleSystem()
        {
            EffectEmitterProto effectEmitterProto = LDB.effectEmitters.Select(21);

            return Object.Instantiate(Resources.Load<ParticleSystem>(effectEmitterProto.PrefabPath));
        }

        internal static void AddEffectEmitterProto()
        {
            Map.Add(35, new Color(0.685f, 0.792f, 0.000f));
            Map.Add(36, new Color(0.965f, 0.867f, 0.352f));

            AddEffectEmitterProto(Map);
        }

        internal static void SetEffectEmitterProto()
        {
            foreach (KeyValuePair<int, Color> pair in Map)
            {
                ref ParticleSystem system = ref LDB.effectEmitters.Select(pair.Key).emitter;
                system = NewParticleSystem();
                ParticleSystem.MainModule systemMain = system.main;
                systemMain.startColor = pair.Value;
            }
        }

        private static void AddEffectEmitterProto(Dictionary<int, Color> protos)
        {
            EffectEmitterProtoSet effectEmitters = LDB.effectEmitters;

            int dataArrayLength = effectEmitters.dataArray.Length;

            EffectEmitterProto[] proto = protos.Select(p => new EffectEmitterProto
            {
                ID = p.Key, Name = "vein-break-" + p.Key,
            }).ToArray();

            Array.Resize(ref effectEmitters.dataArray, dataArrayLength + proto.Length);

            for (var index = 0; index < proto.Length; ++index) effectEmitters.dataArray[dataArrayLength + index] = proto[index];

            effectEmitters.OnAfterDeserialize();
        }
    }
}

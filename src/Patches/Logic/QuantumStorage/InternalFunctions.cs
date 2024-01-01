using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    public static partial class QuantumStoragePatches
    {
        private const int QuantumStorageSize = 90;

        private static readonly StorageComponent Component;

        private static readonly ConcurrentDictionary<int, List<int>> QuantumStorageIds;

        static QuantumStoragePatches()
        {
            QuantumStorageIds = new ConcurrentDictionary<int, List<int>>();
            Component = new StorageComponent(QuantumStorageSize);
            Component.CutNext();
        }

        internal static void SyncNewQuantumStorage(int planetId, int storageid)
        {
            QuantumStorageIds.TryAddOrInsert(planetId, storageid);
            PlanetData planet = GameMain.galaxy.PlanetById(planetId);
            FactoryStorage factoryStorage = GameMain.data.GetOrCreateFactory(planet).factoryStorage;
            factoryStorage.storagePool[storageid] = Component;
        }

        internal static void SyncRemoveQuantumStorage(int planetId, int storageid) => QuantumStorageIds.TryRemove(planetId, storageid);

        internal static void ExportPlanetQuantumStorage(int planetId, BinaryWriter w)
        {
            if (!QuantumStorageIds.ContainsKey(planetId)) QuantumStorageIds[planetId] = new List<int>();
            List<int> datas = QuantumStorageIds[planetId];
            w.Write(datas.Count);
            w.Write(planetId);
            foreach (int id in datas) w.Write(id);
        }

        internal static void ImportPlanetQuantumStorage(BinaryReader r)
        {
            int count = r.ReadInt32();
            int planetId = r.ReadInt32();

            int[] arr = new int[count];
            for (int j = 0; j < count; j++)
            {
                arr[j] = r.ReadInt32();
            }

            QuantumStorageIds[planetId] = new List<int>(arr);
        }

        public static bool Import_PatchMethod(FactoryStorage storage, int index)
        {
            bool b = QuantumStorageIds.Contains(storage.planet.id, index);
            if (b) storage.storagePool[index] = Component;
            return b;
        }

        internal static void Export(BinaryWriter w)
        {
            lock (QuantumStorageIds)
            {
                w.Write(QuantumStorageIds.Count);

                foreach (KeyValuePair<int, List<int>> pair in QuantumStorageIds)
                {
                    w.Write(pair.Key);
                    w.Write(pair.Value.Count);
                    foreach (int t in pair.Value) w.Write(t);
                }
            }

            lock (Component)
            {
                Component.Export(w);
            }
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                int storagecount = r.ReadInt32();

                for (int j = 0; j < storagecount; j++)
                {
                    int key = r.ReadInt32();
                    int length = r.ReadInt32();
                    var datas = new List<int>();
                    for (int i = 0; i < length; i++)
                    {
                        datas.Add(r.ReadInt32());
                    }

                    QuantumStorageIds.TryAdd(key, datas);
                }

                Component.Import(r);
            }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll()
        {
            QuantumStorageIds.Clear();
            Component.SetEmpty();
            Component.CutNext();
        }
    }
}

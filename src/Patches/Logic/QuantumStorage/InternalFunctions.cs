using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic.QuantumStorage
{
    internal readonly struct QuantumStorageData
    {
        public readonly int StorageId;
        public readonly int OrbitId;

        public QuantumStorageData(int storageId, int orbitId)
        {
            StorageId = storageId;
            OrbitId = orbitId;
        }
    }

    public static partial class QuantumStoragePatches
    {
        private const int QuantumStorageSize = 80;

        private static StorageComponent[] _components;

        private static readonly ConcurrentDictionary<int, List<QuantumStorageData>> QuantumStorageIds;

        static QuantumStoragePatches()
        {
            QuantumStorageIds = new ConcurrentDictionary<int, List<QuantumStorageData>>();
            _components = new StorageComponent[10];

            for (var index = 0; index < _components.Length; index++)
            {
                ref StorageComponent component = ref _components[index];
                component = new StorageComponent(QuantumStorageSize);
                component.CutNext();
            }
        }

        private static void QuantumStorageOrbitChange(int planetId, int storageId, int orbitId)
        {
            if (!QuantumStorageIds.TryGetValue(planetId, out List<QuantumStorageData> datas)) return;

            int index = datas.FindIndex(i => i.StorageId == storageId);
            if (index < 0) return;

            datas[index] = new QuantumStorageData(storageId, orbitId);

            PlanetData planet = GameMain.galaxy.PlanetById(planetId);
            FactoryStorage factoryStorage = GameMain.data.GetOrCreateFactory(planet).factoryStorage;
            factoryStorage.storagePool[storageId] = _components[orbitId - 1];
        }

        internal static void SyncNewQuantumStorage(int planetId, int storageId, int orbitId)
        {
            QuantumStorageIds.TryAddOrInsert(planetId, new QuantumStorageData(storageId, orbitId));
            PlanetData planet = GameMain.galaxy.PlanetById(planetId);
            FactoryStorage factoryStorage = GameMain.data.GetOrCreateFactory(planet).factoryStorage;
            factoryStorage.storagePool[storageId] = _components[orbitId - 1];
        }

        internal static void SyncRemoveQuantumStorage(int planetId, int storageId, int orbitId) =>
            QuantumStorageIds.TryRemove(planetId, new QuantumStorageData(storageId, orbitId));

        internal static void SyncQuantumStorageOrbitChange(int planetId, int storageId, int orbitId) =>
            QuantumStorageOrbitChange(planetId, storageId, orbitId);

        internal static void ExportPlanetQuantumStorage(int planetId, BinaryWriter w)
        {
            if (!QuantumStorageIds.ContainsKey(planetId)) QuantumStorageIds[planetId] = new List<QuantumStorageData>();

            List<QuantumStorageData> datas = QuantumStorageIds[planetId];
            w.Write(datas.Count);
            w.Write(planetId);

            foreach (QuantumStorageData data in datas)
            {
                w.Write(data.StorageId);
                w.Write(data.OrbitId);
            }
        }

        internal static void ImportPlanetQuantumStorage(BinaryReader r)
        {
            int count = r.ReadInt32();
            int planetId = r.ReadInt32();

            var arr = new QuantumStorageData[count];

            for (var j = 0; j < count; j++) arr[j] = new QuantumStorageData(r.ReadInt32(), r.ReadInt32());

            QuantumStorageIds[planetId] = new List<QuantumStorageData>(arr);
        }

        public static bool Import_PatchMethod(FactoryStorage storage, int storageId)
        {
            int orbitId = QueryOrbitId(storage.planet.id, storageId);
            if (orbitId < 0) return false;

            storage.storagePool[storageId] = _components[orbitId - 1];

            return true;
        }

        private static int QueryOrbitId(int planetId, int storageId)
        {
            if (!QuantumStorageIds.TryGetValue(planetId, out List<QuantumStorageData> datas)) return -1;

            int index = datas.FindIndex(i => i.StorageId == storageId);

            if (index < 0) return -1;

            return datas[index].OrbitId;
        }

        internal static void Export(BinaryWriter w)
        {
            lock (QuantumStorageIds)
            {
                w.Write(QuantumStorageIds.Count);

                foreach (KeyValuePair<int, List<QuantumStorageData>> pair in QuantumStorageIds)
                {
                    w.Write(pair.Key);
                    w.Write(pair.Value.Count);

                    foreach (QuantumStorageData t in pair.Value)
                    {
                        w.Write(t.StorageId);
                        w.Write(t.OrbitId);
                    }
                }
            }

            lock (_components)
                foreach (StorageComponent storageComponent in _components)
                    storageComponent.Export(w);
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                int storagecount = r.ReadInt32();

                for (var j = 0; j < storagecount; j++)
                {
                    int key = r.ReadInt32();
                    int length = r.ReadInt32();
                    var datas = new List<QuantumStorageData>();

                    for (var i = 0; i < length; i++) datas.Add(new QuantumStorageData(r.ReadInt32(), r.ReadInt32()));

                    QuantumStorageIds.TryAdd(key, datas);
                }

                foreach (StorageComponent storageComponent in _components) storageComponent.Import(r);
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

            _components = new StorageComponent[10];

            for (var index = 0; index < _components.Length; index++)
            {
                ref StorageComponent component = ref _components[index];
                component = new StorageComponent(QuantumStorageSize);
                component.CutNext();
            }
        }
    }
}

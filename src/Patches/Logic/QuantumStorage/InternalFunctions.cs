using System.Collections.Generic;
using System.IO;

namespace ProjectGenesis.Patches.Logic
{
    public static partial class QuantumStoragePatches
    {
        private const int QuantumStorageSize = 90;

        private static StorageComponent _component = new StorageComponent(QuantumStorageSize);

        private static Dictionary<int, List<int>> _quantumStorageIds = new Dictionary<int, List<int>>();

        internal static void SyncNewQuantumStorage(int planetId, int storageid)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(planetId);
            FactoryStorage factoryStorage = GameMain.data.GetOrCreateFactory(planet).factoryStorage;
            factoryStorage.storagePool[storageid] = _component;
        }

        internal static void ExportPlanetQuantumStorage(int planetId, BinaryWriter w)
        {
            if (!_quantumStorageIds.ContainsKey(planetId)) _quantumStorageIds[planetId] = new List<int>();
            List<int> datas = _quantumStorageIds[planetId];
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

            _quantumStorageIds[planetId] = new List<int>(arr);
        }

        private static void SetQuantumStorage()
        {
            GalaxyData galaxy = GameMain.data.galaxy;

            foreach (KeyValuePair<int, List<int>> pair in _quantumStorageIds)
            {
                foreach (int i in pair.Value)
                {
                    PlanetFactory planetFactory = galaxy.PlanetById(pair.Key)?.factory;

                    if (planetFactory == null) continue;

                    planetFactory.factoryStorage.storagePool[i] = _component;

                    FactorySystem factorySystem = planetFactory.factorySystem;

                    int inserterCursor = factorySystem.inserterCursor;

                    for (int index = 1; index < inserterCursor; ++index)
                    {
                        ref InserterComponent inserterComponent = ref factorySystem.inserterPool[index];

                        planetFactory.ReadObjectConn(inserterComponent.entityId, 0, out bool isOutput, out int otherObjId, out int _);
                        if (isOutput && otherObjId >= 0 && inserterComponent.insertTarget != otherObjId)
                        {
                            inserterComponent.insertTarget = otherObjId;
                        }

                        planetFactory.ReadObjectConn(inserterComponent.entityId, 1, out isOutput, out otherObjId, out int _);
                        if (!isOutput && otherObjId >= 0 && inserterComponent.pickTarget != otherObjId)
                        {
                            inserterComponent.pickTarget = otherObjId;
                        }
                    }
                }
            }
        }

        internal static void Export(BinaryWriter w)
        {
            lock (_quantumStorageIds)
            {
                w.Write(_quantumStorageIds.Count);

                foreach (KeyValuePair<int, List<int>> pair in _quantumStorageIds)
                {
                    w.Write(pair.Key);
                    w.Write(pair.Value.Count);
                    foreach (int t in pair.Value) w.Write(t);
                }
            }

            lock (_component)
            {
                _component.Export(w);
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

                    _quantumStorageIds.TryAdd(key, datas);
                }

                _component.Import(r);
                _component.InitConn();
                _component.CutNext();

                SetQuantumStorage();
            }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll()
        {
            _quantumStorageIds = new Dictionary<int, List<int>>();
            _component = new StorageComponent(QuantumStorageSize);
        }
    }
}

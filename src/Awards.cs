using UnityEngine;

namespace ProjectGenesis
{
    public static class Awards
    {
        public static void AddVeinAwards(int type, int amount = 10000)
        {
            var pos = GameMain.mainPlayer.position;

            var factory = GameMain.mainPlayer.planetData.factory;
            var planet = factory.planet;

            var veinProto = LDB.veins.Select(type);
            if (veinProto == null) return;
            var veinData = new VeinData { type = (EVeinType)veinProto.ID, modelIndex = (short)veinProto.ModelIndex, amount = amount };

            if (veinData.amount < 1) veinData.amount = 1;
            if (veinData.amount > 1000000000) veinData.amount = 1000000000;
            veinData.productId = veinProto.MiningItem;

            veinData.pos = pos;
            if (veinData.type == EVeinType.Oil)
            {
                veinData.pos = planet.aux.RawSnap(veinData.pos);
                veinData.pos = veinData.pos.normalized * planet.data.QueryModifiedHeight(pos);
            }

            veinData.minerCount = 0;
            factory.AssignGroupIndexForNewVein(ref veinData);
            var index1 = factory.AddVeinData(veinData);
            VeinData[] veinPool = factory.veinPool;
            veinPool[index1].modelId
                = planet.factoryModel.gpuiManager.AddModel(veinPool[index1].modelIndex, index1, veinPool[index1].pos,
                                                           Maths.SphericalRotation(pos, 0.0f));
            ColliderData[] colliders = veinProto.prefabDesc.colliders;

            for (var index2 = 0; colliders != null && index2 < colliders.Length; ++index2)
            {
                veinPool[index1].colliderId
                    = planet.physics.AddColliderData(colliders[index2].BindToObject(index1, veinPool[index1].colliderId, EObjectType.Vein,
                                                                                    veinPool[index1].pos,
                                                                                    Quaternion.FromToRotation(Vector3.up,
                                                                                                              veinPool[index1].pos.normalized)));
            }

            factory.RefreshVeinMiningDisplay(index1, 0, 0);

            factory.RecalculateVeinGroup(factory.veinPool[index1].groupIndex);
        }
    }
}

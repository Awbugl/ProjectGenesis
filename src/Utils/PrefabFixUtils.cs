using System.Collections.Generic;
using System.Linq;
using ProjectGenesis.Patches;
using UnityEngine;
using ERecipeType_1 = ERecipeType;

namespace ProjectGenesis.Utils
{
    internal static class PrefabFixUtils
    {
        internal static void ItemPostFix(ItemProtoSet itemProtos)
        {
            LDB.items.OnAfterDeserialize();
            itemProtos.Select(2317).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.高分子化工;
            itemProtos.Select(2317).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.化工厂).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(2317).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.化工厂).prefabDesc.workEnergyPerTick * 2;

            itemProtos.Select(物品.二级制造台).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.电路蚀刻;
            itemProtos.Select(物品.三级制造台).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.高精度加工;

            itemProtos.Select(物品.一级制造台).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.一级制造台).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.一级制造台).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(物品.一级制造台).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.一级制造台).prefabDesc.workEnergyPerTick * 2;
            itemProtos.Select(物品.二级制造台).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.二级制造台).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.二级制造台).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(物品.二级制造台).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.二级制造台).prefabDesc.workEnergyPerTick * 2;
            itemProtos.Select(物品.三级制造台).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.三级制造台).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.三级制造台).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(物品.三级制造台).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.三级制造台).prefabDesc.workEnergyPerTick * 2;

            itemProtos.Select(物品.位面熔炉).prefabDesc.assemblerSpeed = 40000;
            itemProtos.Select(物品.化工厂).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.原油精炼厂).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.粒子对撞机).prefabDesc.assemblerSpeed = 40000;

            itemProtos.Select(物品.采矿机).prefabDesc.minerPeriod = 600000;
            itemProtos.Select(物品.大型采矿机).prefabDesc.minerPeriod = 300000;
            itemProtos.Select(物品.原油采集站).prefabDesc.minerPeriod = 300000;
            itemProtos.Select(物品.水泵).prefabDesc.minerPeriod = 360000;

            List<int> waterTypes = itemProtos.Select(物品.水泵).prefabDesc.waterTypes.ToList();
            waterTypes.Add(7017);
            waterTypes.Add(7018);
            itemProtos.Select(物品.水泵).prefabDesc.waterTypes = waterTypes.ToArray();

            itemProtos.Select(物品.电磁轨道弹射器).prefabDesc.ejectorChargeFrame = 20;
            itemProtos.Select(物品.电磁轨道弹射器).prefabDesc.ejectorColdFrame = 10;

            itemProtos.Select(物品.垂直发射井).prefabDesc.siloChargeFrame = 24;
            itemProtos.Select(物品.垂直发射井).prefabDesc.siloColdFrame = 6;

            itemProtos.Select(物品.卫星配电站).prefabDesc.powerConnectDistance = 5300.5f;
            itemProtos.Select(物品.卫星配电站).prefabDesc.powerCoverRadius = 2600.5f;
            itemProtos.Select(物品.电力感应塔).prefabDesc.powerConnectDistance = 44.5f;
            itemProtos.Select(物品.电力感应塔).prefabDesc.powerCoverRadius = 20.5f;
            itemProtos.Select(物品.风力涡轮机).prefabDesc.powerConnectDistance = 32.5f;
            itemProtos.Select(物品.风力涡轮机).prefabDesc.powerCoverRadius = 14.9f;

            itemProtos.Select(物品.火力发电机).prefabDesc.genEnergyPerTick = 200000;
            itemProtos.Select(物品.太阳能板).prefabDesc.genEnergyPerTick = 30000;
            itemProtos.Select(物品.地热发电机).prefabDesc.genEnergyPerTick = 400000;
            itemProtos.Select(物品.聚变发电机).prefabDesc.genEnergyPerTick = 2500000;
            itemProtos.Select(物品.聚变发电机).prefabDesc.useFuelPerTick = 2500000;
            itemProtos.Select(物品.风力涡轮机).prefabDesc.genEnergyPerTick = 25000;
            itemProtos.Select(物品.人造恒星).prefabDesc.genEnergyPerTick = 10000000;
            itemProtos.Select(物品.人造恒星).prefabDesc.useFuelPerTick = 10000000;

            itemProtos.Select(物品.火力发电机).prefabDesc.useFuelPerTick = 150000;

            itemProtos.Select(物品.低速传送带).prefabDesc.beltSpeed = 3;
            itemProtos.Select(物品.高速传送带).prefabDesc.beltSpeed = 5;
            itemProtos.Select(物品.极速传送带).prefabDesc.beltSpeed = 10;

            itemProtos.Select(物品.低速分拣器).prefabDesc.inserterSTT = 100000;
            itemProtos.Select(物品.高速分拣器).prefabDesc.inserterSTT = 50000;
            itemProtos.Select(物品.极速分拣器).prefabDesc.inserterSTT = 25000;

            itemProtos.Select(物品.卫星配电站).prefabDesc.idleEnergyPerTick = 12000000;
            itemProtos.Select(物品.卫星配电站).prefabDesc.workEnergyPerTick = 48000000;

            itemProtos.Select(物品.电弧熔炉).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.电弧熔炉).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.电弧熔炉).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(物品.电弧熔炉).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.电弧熔炉).prefabDesc.workEnergyPerTick * 2;
            itemProtos.Select(物品.无线输电塔).prefabDesc.powerConnectDistance = 90.5f;

            itemProtos.Select(6230).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.矿物处理;
            itemProtos.Select(6230).prefabDesc.idleEnergyPerTick = 400;
            itemProtos.Select(6230).prefabDesc.workEnergyPerTick = 12000;

            itemProtos.Select(6229).prefabDesc.fluidStorageCount = 1000000;
            itemProtos.Select(6275).prefabDesc.assemblerSpeed = 500;
            itemProtos.Select(6275).prefabDesc.workEnergyPerTick = 2000000;
            itemProtos.Select(6275).prefabDesc.idleEnergyPerTick = 100000;
            itemProtos.Select(6275).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.精密组装;
            itemProtos.Select(6276).prefabDesc.assemblerSpeed = 1000;
            itemProtos.Select(6276).prefabDesc.workEnergyPerTick = 8000000;
            itemProtos.Select(6276).prefabDesc.idleEnergyPerTick = 200000;
            itemProtos.Select(6276).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.聚变生产;

            //矿场修复
            var oreFactory = LDB.items.Select(6230);
            var oreFactoryModel = LDB.items.Select(302);
            oreFactoryModel.prefabDesc.dragBuild = true;
            oreFactoryModel.prefabDesc.dragBuildDist = new Vector2(2.9f, 2.9f);
            oreFactory.prefabDesc.dragBuild = true;
            oreFactory.prefabDesc.dragBuildDist = new Vector2(2.9f, 2.9f);
        }

        internal static void ModelPostFix(ModelProtoSet models)
        {
            //行星装配站调试部分
            var testCraftingTableModel = models.Select(403);
            var testCraftingTableModel2 = models.Select(404);
            var testCraftingTableModel3 = models.Select(405);
            var testCraftingTableModel4 = models.Select(406);
            var testCraftingTableModel5 = models.Select(410);
            var testCraftingTableModel6 = models.Select(411);

            var antiMatterModel = models.Select(407);

            testCraftingTableModel.prefabDesc.isAssembler = true;
            testCraftingTableModel.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.Assemble;
            testCraftingTableModel.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            testCraftingTableModel.prefabDesc.isStation = false;
            testCraftingTableModel.prefabDesc.isStellarStation = false;
            testCraftingTableModel.prefabDesc.stationMaxDroneCount = 0;
            testCraftingTableModel.prefabDesc.stationMaxEnergyAcc = 0;
            testCraftingTableModel.prefabDesc.stationMaxItemCount = 0;
            testCraftingTableModel.prefabDesc.stationMaxItemKinds = 0;
            testCraftingTableModel.prefabDesc.stationMaxShipCount = 0;
            testCraftingTableModel.prefabDesc.idleEnergyPerTick = 100000;
            testCraftingTableModel.prefabDesc.workEnergyPerTick = 500000;

            testCraftingTableModel2.prefabDesc.isAssembler = true;
            testCraftingTableModel2.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.所有熔炉;
            testCraftingTableModel2.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            testCraftingTableModel2.prefabDesc.isStation = false;
            testCraftingTableModel2.prefabDesc.isStellarStation = false;
            testCraftingTableModel2.prefabDesc.stationMaxDroneCount = 0;
            testCraftingTableModel2.prefabDesc.stationMaxEnergyAcc = 0;
            testCraftingTableModel2.prefabDesc.stationMaxItemCount = 0;
            testCraftingTableModel2.prefabDesc.stationMaxItemKinds = 0;
            testCraftingTableModel2.prefabDesc.stationMaxShipCount = 0;
            testCraftingTableModel2.prefabDesc.idleEnergyPerTick = 100000;
            testCraftingTableModel2.prefabDesc.workEnergyPerTick = 500000;

            testCraftingTableModel3.prefabDesc.isAssembler = true;
            testCraftingTableModel3.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.所有化工;
            testCraftingTableModel3.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            testCraftingTableModel3.prefabDesc.isStation = false;
            testCraftingTableModel3.prefabDesc.isStellarStation = false;
            testCraftingTableModel3.prefabDesc.stationMaxDroneCount = 0;
            testCraftingTableModel3.prefabDesc.stationMaxEnergyAcc = 0;
            testCraftingTableModel3.prefabDesc.stationMaxItemCount = 0;
            testCraftingTableModel3.prefabDesc.stationMaxItemKinds = 0;
            testCraftingTableModel3.prefabDesc.stationMaxShipCount = 0;
            testCraftingTableModel3.prefabDesc.idleEnergyPerTick = 100000;
            testCraftingTableModel3.prefabDesc.workEnergyPerTick = 500000;

            testCraftingTableModel4.prefabDesc.isAssembler = true;
            testCraftingTableModel4.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.所有高精;
            testCraftingTableModel4.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            testCraftingTableModel4.prefabDesc.isStation = false;
            testCraftingTableModel4.prefabDesc.isStellarStation = false;
            testCraftingTableModel4.prefabDesc.stationMaxDroneCount = 0;
            testCraftingTableModel4.prefabDesc.stationMaxEnergyAcc = 0;
            testCraftingTableModel4.prefabDesc.stationMaxItemCount = 0;
            testCraftingTableModel4.prefabDesc.stationMaxItemKinds = 0;
            testCraftingTableModel4.prefabDesc.stationMaxShipCount = 0;
            testCraftingTableModel4.prefabDesc.idleEnergyPerTick = 100000;
            testCraftingTableModel4.prefabDesc.workEnergyPerTick = 500000;

            testCraftingTableModel5.prefabDesc.isAssembler = true;
            testCraftingTableModel5.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.垃圾回收;
            testCraftingTableModel5.prefabDesc.assemblerSpeed = MegaAssemblerPatches.TrashSpeed;
            testCraftingTableModel5.prefabDesc.isStation = false;
            testCraftingTableModel5.prefabDesc.isStellarStation = false;
            testCraftingTableModel5.prefabDesc.stationMaxDroneCount = 0;
            testCraftingTableModel5.prefabDesc.stationMaxEnergyAcc = 0;
            testCraftingTableModel5.prefabDesc.stationMaxItemCount = 0;
            testCraftingTableModel5.prefabDesc.stationMaxItemKinds = 0;
            testCraftingTableModel5.prefabDesc.stationMaxShipCount = 0;
            testCraftingTableModel5.prefabDesc.idleEnergyPerTick = 200000;
            testCraftingTableModel5.prefabDesc.workEnergyPerTick = 1000000;
            
            testCraftingTableModel6.prefabDesc.isAssembler = true;
            testCraftingTableModel6.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.Particle;
            testCraftingTableModel6.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            testCraftingTableModel6.prefabDesc.isStation = false;
            testCraftingTableModel6.prefabDesc.isStellarStation = false;
            testCraftingTableModel6.prefabDesc.stationMaxDroneCount = 0;
            testCraftingTableModel6.prefabDesc.stationMaxEnergyAcc = 0;
            testCraftingTableModel6.prefabDesc.stationMaxItemCount = 0;
            testCraftingTableModel6.prefabDesc.stationMaxItemKinds = 0;
            testCraftingTableModel6.prefabDesc.stationMaxShipCount = 0;
            testCraftingTableModel6.prefabDesc.idleEnergyPerTick = 200000;
            testCraftingTableModel6.prefabDesc.workEnergyPerTick = 1000000;

            LDB.items.Select(2211).prefabDesc.fuelMask = 5;
            LDB.items.Select(2210).prefabDesc.fuelMask = 6;

            antiMatterModel.prefabDesc.fuelMask = 4;
            antiMatterModel.prefabDesc.genEnergyPerTick = 1200000000;
            antiMatterModel.prefabDesc.useFuelPerTick = 1200000000;
        }
    }
}

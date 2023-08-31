using System.Collections.Generic;
using System.Linq;
using ProjectGenesis.Patches.Logic.MegaAssembler;
using UnityEngine;
using ERecipeType_1 = ERecipeType;
using static ProjectGenesis.Utils.ProtoIDUsedByPatches;

namespace ProjectGenesis.Utils
{
    internal static class PrefabFixUtils
    {
        internal static void ItemPostFix(ItemProtoSet itemProtos)
        {
            LDB.items.OnAfterDeserialize();

            ref PrefabDesc prefabDesc = ref itemProtos.Select(2317).prefabDesc;
            prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.高分子化工;
            prefabDesc.idleEnergyPerTick = 800;
            prefabDesc.workEnergyPerTick = 24000;

            ref PrefabDesc prefabDesc1 = ref itemProtos.Select(I一级制造台).prefabDesc;
            prefabDesc1.assemblerSpeed = 20000;
            prefabDesc1.idleEnergyPerTick *= 2;
            prefabDesc1.workEnergyPerTick *= 2;

            ref PrefabDesc prefabDesc2 = ref itemProtos.Select(I二级制造台).prefabDesc;
            prefabDesc2.assemblerSpeed = 20000;
            prefabDesc2.idleEnergyPerTick *= 2;
            prefabDesc2.workEnergyPerTick *= 2;
            prefabDesc2.assemblerRecipeType = (ERecipeType_1)ERecipeType.电路蚀刻;

            ref PrefabDesc prefabDesc3 = ref itemProtos.Select(I三级制造台).prefabDesc;
            prefabDesc3.assemblerSpeed = 20000;
            prefabDesc3.idleEnergyPerTick *= 2;
            prefabDesc3.workEnergyPerTick *= 2;
            prefabDesc3.assemblerRecipeType = (ERecipeType_1)ERecipeType.高精度加工;

            ref PrefabDesc prefabDesc4 = ref itemProtos.Select(I电弧熔炉).prefabDesc;
            prefabDesc4.assemblerSpeed = 20000;
            prefabDesc4.idleEnergyPerTick *= 2;
            prefabDesc4.workEnergyPerTick *= 2;

            ref PrefabDesc desc7 = ref itemProtos.Select(6230).prefabDesc;
            desc7.assemblerRecipeType = (ERecipeType_1)ERecipeType.矿物处理;
            desc7.idleEnergyPerTick = 400;
            desc7.workEnergyPerTick = 12000;
            desc7.dragBuild = true;
            desc7.dragBuildDist = new Vector2(2.9f, 2.9f);

            ref PrefabDesc prefabDesc8 = ref itemProtos.Select(6275).prefabDesc;
            prefabDesc8.assemblerSpeed = 10000;
            prefabDesc8.workEnergyPerTick = 40000000;
            prefabDesc8.idleEnergyPerTick = 4000000;
            prefabDesc8.assemblerRecipeType = (ERecipeType_1)ERecipeType.精密组装;

            ref PrefabDesc desc8 = ref itemProtos.Select(6276).prefabDesc;
            desc8.assemblerSpeed = 10000;
            desc8.workEnergyPerTick = 80000000;
            desc8.idleEnergyPerTick = 8000000;
            desc8.assemblerRecipeType = (ERecipeType_1)ERecipeType.聚变生产;

            ref PrefabDesc desc1 = ref itemProtos.Select(I水泵).prefabDesc;
            desc1.minerPeriod = 360000;
            desc1.waterTypes = new[] { 1000, 1116, 7017, 7018 };

            ref PrefabDesc desc2 = ref itemProtos.Select(I电磁轨道弹射器).prefabDesc;
            desc2.ejectorChargeFrame = 40;
            desc2.ejectorColdFrame = 20;

            ref PrefabDesc desc3 = ref itemProtos.Select(I垂直发射井).prefabDesc;
            desc3.siloChargeFrame = 48;
            desc3.siloColdFrame = 12;

            ref PrefabDesc desc4 = ref itemProtos.Select(I卫星配电站).prefabDesc;
            desc4.powerConnectDistance = 5300.5f;
            desc4.powerCoverRadius = 2600.5f;
            desc4.idleEnergyPerTick = 12000000;
            desc4.workEnergyPerTick = 48000000;

            ref PrefabDesc prefabDesc5 = ref itemProtos.Select(I电力感应塔).prefabDesc;
            prefabDesc5.powerConnectDistance = 44.5f;
            prefabDesc5.powerCoverRadius = 20.5f;

            ref PrefabDesc desc5 = ref itemProtos.Select(I风力涡轮机).prefabDesc;
            desc5.powerConnectDistance = 24f;
            desc5.powerCoverRadius = 14.9f;
            desc5.genEnergyPerTick = 12500;

            ref PrefabDesc prefabDesc6 = ref itemProtos.Select(I火力发电机).prefabDesc;
            prefabDesc6.genEnergyPerTick = 200000;
            prefabDesc6.useFuelPerTick = 250000;

            ref PrefabDesc desc6 = ref itemProtos.Select(I聚变发电机).prefabDesc;
            desc6.genEnergyPerTick = 2500000;
            desc6.useFuelPerTick = 2500000;

            ref PrefabDesc prefabDesc7 = ref itemProtos.Select(I人造恒星).prefabDesc;
            prefabDesc7.genEnergyPerTick = 10000000;
            prefabDesc7.useFuelPerTick = 10000000;

            itemProtos.Select(I位面熔炉).prefabDesc.assemblerSpeed = 40000;
            itemProtos.Select(I化工厂).prefabDesc.assemblerSpeed = 40000;
            itemProtos.Select(I精炼厂).prefabDesc.assemblerSpeed = 40000;
            itemProtos.Select(I量子化工厂).prefabDesc.assemblerSpeed = 40000;
            itemProtos.Select(I粒子对撞机).prefabDesc.assemblerSpeed = 40000;

            itemProtos.Select(I采矿机).prefabDesc.minerPeriod = 600000;
            itemProtos.Select(I大型采矿机).prefabDesc.minerPeriod = 300000;
            itemProtos.Select(I原油采集站).prefabDesc.minerPeriod = 300000;

            itemProtos.Select(I太阳能板).prefabDesc.genEnergyPerTick = 15000;
            itemProtos.Select(I地热发电机).prefabDesc.genEnergyPerTick = 400000;

            itemProtos.Select(I低速传送带).prefabDesc.beltSpeed = 3;
            itemProtos.Select(I低速分拣器).prefabDesc.inserterSTT = 100000;

            itemProtos.Select(I高速传送带).prefabDesc.beltSpeed = 5;
            itemProtos.Select(I高速分拣器).prefabDesc.inserterSTT = 50000;

            itemProtos.Select(I极速传送带).prefabDesc.beltSpeed = 10;
            itemProtos.Select(I极速分拣器).prefabDesc.inserterSTT = 25000;

            itemProtos.Select(I无线输电塔).prefabDesc.powerConnectDistance = 90.5f;

            itemProtos.Select(6229).prefabDesc.fluidStorageCount = 1000000;
        }

        internal static void ModelPostFix(ModelProtoSet models)
        {
            // 天穹装配厂
            ModelProto testCraftingTableModel = models.Select(453);
            SetMegaAssemblerPrefebDesc(ref testCraftingTableModel.prefabDesc, ERecipeType.Assemble);

            // 物质裂解塔
            ModelProto testCraftingTableModel2 = models.Select(454);
            SetMegaAssemblerPrefebDesc(ref testCraftingTableModel2.prefabDesc, ERecipeType.所有熔炉);

            // 巨型化学反应釜
            ModelProto testCraftingTableModel3 = models.Select(455);
            SetMegaAssemblerPrefebDesc(ref testCraftingTableModel3.prefabDesc, ERecipeType.所有化工);

            // 精密结构组装厂
            ModelProto testCraftingTableModel4 = models.Select(456);
            SetMegaAssemblerPrefebDesc(ref testCraftingTableModel4.prefabDesc, ERecipeType.所有高精);

            // 物质分解设施
            ModelProto testCraftingTableModel5 = models.Select(460);
            SetMegaAssemblerPrefebDesc(ref testCraftingTableModel5.prefabDesc, ERecipeType.垃圾回收, 1000000, 1000000, MegaAssemblerPatches.TrashSpeed);

            // 巨型粒子对撞机
            ModelProto testCraftingTableModel6 = models.Select(461);
            SetMegaAssemblerPrefebDesc(ref testCraftingTableModel6.prefabDesc, ERecipeType.Particle, 200000, 1000000);

            LDB.items.Select(2211).prefabDesc.fuelMask = 2;
            LDB.items.Select(2210).prefabDesc.fuelMask = 16;

            ModelProto antiMatterModel = models.Select(457);
            ref PrefabDesc antiMatterModelprefabDesc = ref antiMatterModel.prefabDesc;
            antiMatterModelprefabDesc.fuelMask = 4;
            antiMatterModelprefabDesc.genEnergyPerTick = 1200000000;
            antiMatterModelprefabDesc.useFuelPerTick = 1200000000;

            ModelProto megapumper = models.Select(462);
            ref PrefabDesc megapumperprefabDesc = ref megapumper.prefabDesc;
            megapumperprefabDesc.isFractionator = false;
            megapumperprefabDesc.assemblerRecipeType = ERecipeType_1.None;
            megapumperprefabDesc.assemblerSpeed = 0;
            megapumperprefabDesc.minerPeriod = 36000;
            megapumperprefabDesc.minerType = EMinerType.Water;
            megapumperprefabDesc.minimapType = 2;
            megapumperprefabDesc.idleEnergyPerTick = 50000;
            megapumperprefabDesc.workEnergyPerTick = 500000;
            megapumperprefabDesc.waterPoints = new[] { Vector3.zero };
            megapumperprefabDesc.waterTypes = new[] { 1000, 1116, 7017, 7018 };

            List<Pose> poses = megapumperprefabDesc.portPoses.ToList();
            poses.Add(new Pose(new Vector3(0, 0, -1.4f), Quaternion.Euler(0, 180, 0)));
            megapumperprefabDesc.portPoses = poses.ToArray();

            ModelProto gaspumper = models.Select(463);
            ref PrefabDesc gaspumperprefabDesc = ref gaspumper.prefabDesc;
            gaspumperprefabDesc.workEnergyPerTick = 0;
            gaspumperprefabDesc.isCollectStation = true;
            gaspumperprefabDesc.isPowerConsumer = false;
            gaspumperprefabDesc.stationCollectSpeed = 3;

            ModelProto orbitalcollector = models.Select(117);
            ref PrefabDesc orbitalcollectorprefabDesc = ref orbitalcollector.prefabDesc;
            orbitalcollectorprefabDesc.stationCollectSpeed = 18;

            ModelProto accumulator = models.Select(46);
            ref PrefabDesc accumulatorprefabDesc = ref accumulator.prefabDesc;
            accumulatorprefabDesc.maxAcuEnergy = 2700000000;
            accumulatorprefabDesc.inputEnergyPerTick = 150000;
            accumulatorprefabDesc.outputEnergyPerTick = 150000;

            ModelProto energyexchanger = models.Select(45);
            ref PrefabDesc energyexchangerprefabDesc = ref energyexchanger.prefabDesc;
            energyexchangerprefabDesc.maxAcuEnergy = 2700000000;
            energyexchangerprefabDesc.maxExcEnergy = 2700000000;
            energyexchangerprefabDesc.exchangeEnergyPerTick = 10000000;

            ModelProto ils = models.Select(50);
            ref PrefabDesc ilsprefabDesc = ref ils.prefabDesc;
            ilsprefabDesc.workEnergyPerTick *= 2;
            ilsprefabDesc.stationMaxEnergyAcc *= 2;
            ilsprefabDesc.stationMaxItemCount = 20000;

            ModelProto pls = models.Select(49);
            ref PrefabDesc plsprefabDesc = ref pls.prefabDesc;
            plsprefabDesc.workEnergyPerTick *= 2;
            plsprefabDesc.stationMaxEnergyAcc *= 2;
            plsprefabDesc.stationMaxItemCount = 10000;
            plsprefabDesc.stationMaxItemKinds = 5;
            plsprefabDesc.stationMaxDroneCount = 80;
        }

        private static void SetMegaAssemblerPrefebDesc(
            ref PrefabDesc prefabDesc,
            ERecipeType type,
            int idleEnergy = 100000,
            int workEnergy = 500000,
            int speed = MegaAssemblerPatches.MegaAssemblerSpeed)
        {
            prefabDesc.isAssembler = true;
            prefabDesc.assemblerRecipeType = (ERecipeType_1)type;
            prefabDesc.assemblerSpeed = speed;
            prefabDesc.isStation = false;
            prefabDesc.isStellarStation = false;
            prefabDesc.stationMaxDroneCount = 0;
            prefabDesc.stationMaxEnergyAcc = 0;
            prefabDesc.stationMaxItemCount = 0;
            prefabDesc.stationMaxItemKinds = 0;
            prefabDesc.stationMaxShipCount = 0;
            prefabDesc.idleEnergyPerTick = idleEnergy;
            prefabDesc.workEnergyPerTick = workEnergy;
        }
    }
}

using System;
using System.Collections.Generic;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using xiaoye97;
using static ProjectGenesis.Utils.JsonHelper;
using ERecipeType_1 = ERecipeType;

#pragma warning disable CS0618

namespace ProjectGenesis.Utils
{
    internal static class JsonDataUtils
    {
        internal static void ImportJson(int[] tableID)
        {
            ref Dictionary<int, IconToolNew.IconDesc> itemIconDescs
                = ref AccessTools.StaticFieldRefAccess<Dictionary<int, IconToolNew.IconDesc>>(typeof(ProtoRegistry), "itemIconDescs");

            #region TechProto

            TechProto templateTech = LDB.techs.Select(1311);

            foreach (TechProtoJson techjson in TechProtos())
            {
                TechProto proto = LDB.techs.Exist(techjson.ID) ? LDB.techs.Select(techjson.ID) : templateTech.Copy();
                proto.ID = techjson.ID;
                proto.Name = techjson.Name;
                proto.Desc = techjson.Desc;
                proto.Conclusion = techjson.Conclusion;
                proto.IsHiddenTech = techjson.IsHiddenTech;
                proto.PreItem = techjson.PreItem;
                proto.Published = techjson.Published;
                proto.IconPath = techjson.IconPath;
                proto.IsLabTech = techjson.IsLabTech;
                proto.PreTechs = techjson.PreTechs;
                proto.PreTechsImplicit = techjson.PreTechsImplicit;
                proto.PreTechsMax = techjson.PreTechsMax;
                proto.Items = techjson.Items ?? Array.Empty<int>();
                proto.ItemPoints = techjson.ItemPoints ?? Array.Empty<int>();
                proto.AddItems = techjson.AddItems ?? Array.Empty<int>();
                proto.AddItemCounts = techjson.AddItemCounts ?? Array.Empty<int>();
                proto.Position = new Vector2(techjson.Position[0], techjson.Position[1]);
                proto.HashNeeded = techjson.HashNeeded;
                proto.UnlockRecipes = techjson.UnlockRecipes;
                proto.UnlockFunctions = techjson.UnlockFunctions;
                proto.UnlockValues = techjson.UnlockValues;
                proto.Level = techjson.Level;
                proto.MaxLevel = techjson.MaxLevel;
                proto.LevelCoef1 = techjson.LevelCoef1;
                proto.LevelCoef2 = techjson.LevelCoef2;
                proto.PropertyOverrideItems = techjson.PropertyOverrideItems;
                proto.PropertyItemCounts = techjson.PropertyItemCounts;

                if (!LDB.techs.Exist(techjson.ID)) LDBTool.PreAddProto(proto);
            }

            #endregion

            #region Mod ItemProto

            foreach (ItemProtoJson itemjson in ItemModProtos())
            {
                itemjson.GridIndex = GetTableID(itemjson.GridIndex);

                ItemProto proto = ProtoRegistry.RegisterItem(itemjson.ID, itemjson.Name, itemjson.Description, itemjson.IconPath, itemjson.GridIndex,
                                                             itemjson.StackSize, (EItemType)itemjson.Type, IconDescUtils.GetIconDesc(itemjson.ID));

                proto.ID = itemjson.ID;
                proto.Name = itemjson.Name;
                proto.Description = itemjson.Description;
                proto.IconPath = itemjson.IconPath;
                proto.StackSize = itemjson.StackSize;
                proto.GridIndex = itemjson.GridIndex;
                proto.FuelType = itemjson.FuelType;
                proto.HeatValue = itemjson.HeatValue;
                proto.ReactorInc = itemjson.ReactorInc;
                proto.DescFields = itemjson.DescFields ?? Array.Empty<int>();
                proto.IsFluid = itemjson.IsFluid;
                proto.Type = (EItemType)itemjson.Type;
                proto.SubID = itemjson.SubID;
                proto.MiningFrom = itemjson.MiningFrom;
                proto.ProduceFrom = itemjson.ProduceFrom;
                proto.Grade = itemjson.Grade;
                proto.Upgrades = itemjson.Upgrades ?? Array.Empty<int>();
                proto.IsEntity = itemjson.IsEntity;
                proto.CanBuild = itemjson.CanBuild;
                proto.BuildInGas = itemjson.BuildInGas;
                proto.ModelIndex = itemjson.ModelIndex;
                proto.ModelCount = itemjson.ModelCount;
                proto.HpMax = itemjson.HpMax;
                proto.Ability = itemjson.Ability;
                proto.Potential = itemjson.Potential;
                proto.BuildIndex = itemjson.BuildIndex;
                proto.BuildMode = itemjson.BuildMode;
                proto.UnlockKey = itemjson.UnlockKey;
                proto.MechaMaterialID = itemjson.MechaMaterialID;
                proto.PreTechOverride = itemjson.PreTechOverride;
                proto.Productive = itemjson.Productive;
                proto.MechaMaterialID = itemjson.MechaMaterialID;
                proto.AmmoType = (EAmmoType)itemjson.AmmoType;
                proto.BombType = itemjson.BombType;
                proto.CraftType = itemjson.CraftType;
                proto.DropRate = itemjson.DropRate;
                proto.EnemyDropLevel = itemjson.EnemyDropLevel;
                proto.EnemyDropRange = new Vector2(itemjson.EnemyDropRange[0], itemjson.EnemyDropRange[1]);
                proto.EnemyDropCount = itemjson.EnemyDropCount;
                proto.EnemyDropMask = itemjson.EnemyDropMask;
            }

            #endregion

            #region Vanilla ItemProto

            foreach (ItemProtoJson itemjson in ItemVanillaProtos())
            {
                itemjson.GridIndex = GetTableID(itemjson.GridIndex);

                ItemProto proto = LDB.items.Select(itemjson.ID);

                if (proto.IconPath != itemjson.IconPath) itemIconDescs.Add(itemjson.ID, IconDescUtils.GetIconDesc(itemjson.ID));

                proto.ID = itemjson.ID;
                proto.Name = itemjson.Name;
                proto.Description = itemjson.Description;
                proto.IconPath = itemjson.IconPath;
                proto.StackSize = itemjson.StackSize;
                proto.GridIndex = itemjson.GridIndex;
                proto.FuelType = itemjson.FuelType;
                proto.HeatValue = itemjson.HeatValue;
                proto.ReactorInc = itemjson.ReactorInc;
                proto.DescFields = itemjson.DescFields ?? Array.Empty<int>();
                proto.IsFluid = itemjson.IsFluid;
                proto.Type = (EItemType)itemjson.Type;
                proto.SubID = itemjson.SubID;
                proto.MiningFrom = itemjson.MiningFrom;
                proto.ProduceFrom = itemjson.ProduceFrom;
                proto.Grade = itemjson.Grade;
                proto.Upgrades = itemjson.Upgrades ?? Array.Empty<int>();
                proto.IsEntity = itemjson.IsEntity;
                proto.CanBuild = itemjson.CanBuild;
                proto.BuildInGas = itemjson.BuildInGas;
                proto.ModelIndex = itemjson.ModelIndex;
                proto.ModelCount = itemjson.ModelCount;
                proto.HpMax = itemjson.HpMax;
                proto.Ability = itemjson.Ability;
                proto.Potential = itemjson.Potential;
                proto.BuildIndex = itemjson.BuildIndex;
                proto.BuildMode = itemjson.BuildMode;
                proto.UnlockKey = itemjson.UnlockKey;
                proto.MechaMaterialID = itemjson.MechaMaterialID;
                proto.PreTechOverride = itemjson.PreTechOverride;
                proto.Productive = itemjson.Productive;
                proto.MechaMaterialID = itemjson.MechaMaterialID;
                proto.AmmoType = (EAmmoType)itemjson.AmmoType;
                proto.BombType = itemjson.BombType;
                proto.CraftType = itemjson.CraftType;
                proto.DropRate = itemjson.DropRate;
                proto.EnemyDropLevel = itemjson.EnemyDropLevel;
                proto.EnemyDropRange = new Vector2(itemjson.EnemyDropRange[0], itemjson.EnemyDropRange[1]);
                proto.EnemyDropCount = itemjson.EnemyDropCount;
                proto.EnemyDropMask = itemjson.EnemyDropMask;
            }

            #endregion

            #region RecipeProto

            foreach (RecipeProtoJson recipeJson in RecipeProtos())
            {
                recipeJson.GridIndex = GetTableID(recipeJson.GridIndex);

                RecipeProto proto = LDB.recipes.Exist(recipeJson.ID)
                                        ? LDB.recipes.Select(recipeJson.ID)
                                        : ProtoRegistry.RegisterRecipe(recipeJson.ID, (ERecipeType_1)recipeJson.Type, recipeJson.Time,
                                                                       recipeJson.Input, recipeJson.InCounts, recipeJson.Output ?? Array.Empty<int>(),
                                                                       recipeJson.OutCounts ?? Array.Empty<int>(), recipeJson.Description, 0,
                                                                       recipeJson.GridIndex, recipeJson.Name, recipeJson.IconPath);

                proto.Explicit = recipeJson.Explicit;
                proto.Name = recipeJson.Name;
                proto.Handcraft = recipeJson.Handcraft;
                proto.Type = (ERecipeType_1)recipeJson.Type;
                proto.TimeSpend = recipeJson.Time;
                proto.Items = recipeJson.Input;
                proto.ItemCounts = recipeJson.InCounts;
                proto.Results = recipeJson.Output ?? Array.Empty<int>();
                proto.ResultCounts = recipeJson.OutCounts ?? Array.Empty<int>();
                proto.Description = recipeJson.Description;
                proto.GridIndex = recipeJson.GridIndex;
                proto.IconPath = recipeJson.IconPath;
                proto.NonProductive = recipeJson.NonProductive;
            }

            #endregion

            #region TutorialProto

            TutorialProtoJson[] tutorialProtos = TutorialProtos();

            foreach (TutorialProtoJson json in tutorialProtos)
            {
                LDBTool.PreAddProto(new TutorialProto
                                    {
                                        ID = json.ID,
                                        Name = json.Name,
                                        PreText = json.PreText,
                                        DeterminatorName = json.DeterminatorName,
                                        DeterminatorParams = json.DeterminatorParams
                                    });
            }

            #endregion

            int GetTableID(int gridIndex)
            {
                if (gridIndex >= 5000) return (tableID[2] - 5) * 1000 + gridIndex;
                if (gridIndex >= 4000) return (tableID[1] - 4) * 1000 + gridIndex;
                if (gridIndex >= 3000) return (tableID[0] - 3) * 1000 + gridIndex;
                return gridIndex;
            }
        }

        internal static void PrefabDescPostFix()
        {
            PrefabDescJson[] prefabDescs = PrefabDescs();

            foreach (PrefabDescJson json in prefabDescs)
            {
                ref PrefabDesc desc = ref LDB.models.Select(json.ModelID).prefabDesc;

                if (json.isAccumulator != null) desc.isAccumulator = json.isAccumulator.Value;
                if (json.isAssembler != null) desc.isAssembler = json.isAssembler.Value;
                if (json.isFractionator != null) desc.isFractionator = json.isFractionator.Value;
                if (json.isPowerGen != null) desc.isPowerGen = json.isPowerGen.Value;
                if (json.isStation != null) desc.isStation = json.isStation.Value;
                if (json.isStellarStation != null) desc.isStellarStation = json.isStellarStation.Value;
                if (json.isCollectStation != null) desc.isCollectStation = json.isCollectStation.Value;
                if (json.isPowerConsumer != null) desc.isPowerConsumer = json.isPowerConsumer.Value;
                if (json.assemblerSpeed != null) desc.assemblerSpeed = json.assemblerSpeed.Value;
                if (json.assemblerRecipeType != null) desc.assemblerRecipeType = (ERecipeType_1)json.assemblerRecipeType.Value;
                if (json.workEnergyPerTick != null) desc.workEnergyPerTick = json.workEnergyPerTick.Value;
                if (json.idleEnergyPerTick != null) desc.idleEnergyPerTick = json.idleEnergyPerTick.Value;
                if (json.dragBuild != null) desc.dragBuild = json.dragBuild.Value;
                if (json.dragBuildDist != null) desc.dragBuildDist = new Vector2(json.dragBuildDist[0], json.dragBuildDist[1]);
                if (json.minerPeriod != null) desc.minerPeriod = json.minerPeriod.Value;
                if (json.ejectorChargeFrame != null) desc.ejectorChargeFrame = json.ejectorChargeFrame.Value;
                if (json.ejectorColdFrame != null) desc.ejectorColdFrame = json.ejectorColdFrame.Value;
                if (json.siloChargeFrame != null) desc.siloChargeFrame = json.siloChargeFrame.Value;
                if (json.siloColdFrame != null) desc.siloColdFrame = json.siloColdFrame.Value;
                if (json.powerConnectDistance != null) desc.powerConnectDistance = json.powerConnectDistance.Value;
                if (json.powerCoverRadius != null) desc.powerCoverRadius = json.powerCoverRadius.Value;
                if (json.genEnergyPerTick != null) desc.genEnergyPerTick = json.genEnergyPerTick.Value;
                if (json.useFuelPerTick != null) desc.useFuelPerTick = json.useFuelPerTick.Value;
                if (json.beltSpeed != null) desc.beltSpeed = json.beltSpeed.Value;
                if (json.inserterSTT != null) desc.inserterSTT = json.inserterSTT.Value;
                if (json.fluidStorageCount != null) desc.fluidStorageCount = json.fluidStorageCount.Value;
                if (json.fuelMask != null) desc.fuelMask = json.fuelMask.Value;
                if (json.minerType != null) desc.minerType = (EMinerType)json.minerType.Value;
                if (json.minimapType != null) desc.minimapType = json.minimapType.Value;
                if (json.maxAcuEnergy != null) desc.maxAcuEnergy = json.maxAcuEnergy.Value;
                if (json.maxExcEnergy != null) desc.maxExcEnergy = json.maxExcEnergy.Value;
                if (json.inputEnergyPerTick != null) desc.inputEnergyPerTick = json.inputEnergyPerTick.Value;
                if (json.outputEnergyPerTick != null) desc.outputEnergyPerTick = json.outputEnergyPerTick.Value;
                if (json.exchangeEnergyPerTick != null) desc.exchangeEnergyPerTick = json.exchangeEnergyPerTick.Value;
                if (json.stationCollectSpeed != null) desc.stationCollectSpeed = json.stationCollectSpeed.Value;
                if (json.stationMaxEnergyAcc != null) desc.stationMaxEnergyAcc = json.stationMaxEnergyAcc.Value;
                if (json.stationMaxItemCount != null) desc.stationMaxItemCount = json.stationMaxItemCount.Value;
                if (json.stationMaxItemKinds != null) desc.stationMaxItemKinds = json.stationMaxItemKinds.Value;
                if (json.stationMaxShipCount != null) desc.stationMaxShipCount = json.stationMaxShipCount.Value;
                if (json.stationMaxDroneCount != null) desc.stationMaxDroneCount = json.stationMaxDroneCount.Value;
            }

            ref PrefabDesc pumper = ref LDB.models.Select(60).prefabDesc;
            pumper.waterTypes = new[] { 1000, 1116, 7014, 7017, 7018 };

            ref PrefabDesc megapumper = ref LDB.models.Select(508).prefabDesc;
            megapumper.waterTypes = new[] { 1000, 1116, 7014, 7017, 7018 };

            megapumper.waterPoints = new[] { Vector3.zero };
            megapumper.portPoses = new[] { megapumper.portPoses[0] };
        }

        internal static void SetBuildBar()
        {
            LDBTool.SetBuildBar(1, 9, 6221);
            LDBTool.SetBuildBar(1, 10, 6261);
            LDBTool.SetBuildBar(2, 5, 6266);
            LDBTool.SetBuildBar(3, 10, 2313);
            LDBTool.SetBuildBar(4, 4, 6229);

            LDBTool.SetBuildBar(5, 3, 6230);
            LDBTool.SetBuildBar(5, 4, 2303);
            LDBTool.SetBuildBar(5, 5, 2304);
            LDBTool.SetBuildBar(5, 6, 2305);
            LDBTool.SetBuildBar(5, 7, 2308);
            LDBTool.SetBuildBar(5, 8, 2309);
            LDBTool.SetBuildBar(5, 9, 2317);
            LDBTool.SetBuildBar(5, 10, 2310);

            LDBTool.SetBuildBar(6, 5, 6267);
            LDBTool.SetBuildBar(6, 6, 2209);
            LDBTool.SetBuildBar(6, 7, 2206);

            LDBTool.SetBuildBar(12, 1, 6257);
            LDBTool.SetBuildBar(12, 2, 6258);
            LDBTool.SetBuildBar(12, 3, 6259);
            LDBTool.SetBuildBar(12, 4, 6260);
            LDBTool.SetBuildBar(12, 5, 6264);
            LDBTool.SetBuildBar(12, 6, 6265);
        }
    }
}

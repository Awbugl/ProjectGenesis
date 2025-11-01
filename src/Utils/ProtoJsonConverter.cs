using System;
using UnityEngine;
using static ProjectGenesis.Utils.JsonHelper;

namespace ProjectGenesis.Utils
{
    internal static class ProtoJsonConverter
    {
        public static ItemProtoJson FromProto(ItemProto i) =>
            new ItemProtoJson
            {
                ID = i.ID,
                Name = i.Name,
                Description = i.Description,
                IconPath = i.IconPath,
                GridIndex = i.GridIndex,
                StackSize = i.StackSize,
                FuelType = i.FuelType,
                HeatValue = i.HeatValue,
                ReactorInc = i.ReactorInc,
                DescFields = i.DescFields,
                IsFluid = i.IsFluid,
                Type = (int)i.Type,
                SubID = i.SubID,
                MiningFrom = i.MiningFrom,
                ProduceFrom = i.ProduceFrom,
                Grade = i.Grade,
                Upgrades = i.Upgrades,
                IsEntity = i.IsEntity,
                CanBuild = i.CanBuild,
                BuildInGas = i.BuildInGas,
                ModelIndex = i.ModelIndex,
                ModelCount = i.ModelCount,
                HpMax = i.HpMax,
                Ability = i.Ability,
                Potential = i.Potential,
                BuildIndex = i.BuildIndex,
                BuildMode = i.BuildMode,
                UnlockKey = i.UnlockKey,
                PreTechOverride = i.PreTechOverride,
                Productive = i.Productive,
                MechaMaterialID = i.MechaMaterialID,
                AmmoType = (int)i.AmmoType,
                BombType = (int)i.BombType,
                CraftType = i.CraftType,
                DropRate = i.DropRate,
                EnemyDropLevel = i.EnemyDropLevel,
                EnemyDropRange = new[] { i.EnemyDropRange.x, i.EnemyDropRange.y, },
                EnemyDropCount = i.EnemyDropCount,
                EnemyDropMask = i.EnemyDropMask,
                EnemyDropMaskRatio = i.EnemyDropMaskRatio,
            };

        public static ItemProto ToProto(this ItemProtoJson json) => ToProto(json, new ItemProto());

        public static ItemProto ToProto(this ItemProtoJson json, ItemProto proto)
        {
            proto.ID = json.ID;
            proto.Name = json.Name;
            proto.Description = json.Description;
            proto.IconPath = json.IconPath;
            proto.GridIndex = json.GridIndex;
            proto.StackSize = json.StackSize;
            proto.FuelType = json.FuelType;
            proto.HeatValue = json.HeatValue;
            proto.ReactorInc = json.ReactorInc;
            proto.DescFields = json.DescFields ?? Array.Empty<int>();
            proto.IsFluid = json.IsFluid;
            proto.Type = (EItemType)json.Type;
            proto.SubID = json.SubID;
            proto.MiningFrom = json.MiningFrom;
            proto.ProduceFrom = json.ProduceFrom;
            proto.Grade = json.Grade;
            proto.Upgrades = json.Upgrades ?? Array.Empty<int>();
            proto.IsEntity = json.IsEntity;
            proto.CanBuild = json.CanBuild;
            proto.BuildInGas = json.BuildInGas;
            proto.ModelIndex = json.ModelIndex;
            proto.ModelCount = json.ModelCount;
            proto.HpMax = json.HpMax;
            proto.Ability = json.Ability;
            proto.Potential = json.Potential;
            proto.BuildIndex = json.BuildIndex;
            proto.BuildMode = json.BuildMode;
            proto.UnlockKey = json.UnlockKey;
            proto.PreTechOverride = json.PreTechOverride;
            proto.Productive = json.Productive;
            proto.MechaMaterialID = json.MechaMaterialID;
            proto.AmmoType = (EAmmoType)json.AmmoType;
            proto.BombType = (EBombType)json.BombType;
            proto.CraftType = json.CraftType;
            proto.DropRate = json.DropRate;
            proto.EnemyDropLevel = json.EnemyDropLevel;
            proto.EnemyDropRange = new Vector2(json.EnemyDropRange[0], json.EnemyDropRange[1]);
            proto.EnemyDropCount = json.EnemyDropCount;
            proto.EnemyDropMask = json.EnemyDropMask;
            proto.EnemyDropMaskRatio = json.EnemyDropMaskRatio;
            return proto;
        }

        public static RecipeProtoJson FromProto(RecipeProto i) =>
            new RecipeProtoJson
            {
                ID = i.ID,
                Explicit = i.Explicit,
                Name = i.Name,
                Handcraft = i.Handcraft,
                Type = (int)i.Type,
                Time = i.TimeSpend,
                Input = i.Items ?? Array.Empty<int>(),
                InCounts = i.ItemCounts ?? Array.Empty<int>(),
                Output = i.Results ?? Array.Empty<int>(),
                OutCounts = i.ResultCounts ?? Array.Empty<int>(),
                Description = i.Description,
                GridIndex = i.GridIndex,
                IconPath = i.IconPath,
                NonProductive = i.NonProductive,
            };

        public static RecipeProto ToProto(this RecipeProtoJson json) => ToProto(json, new RecipeProto());

        public static RecipeProto ToProto(this RecipeProtoJson json, RecipeProto proto)
        {
            proto.ID = json.ID;
            proto.Explicit = json.Explicit;
            proto.Name = json.Name;
            proto.Handcraft = json.Handcraft;
            proto.Type = (global::ERecipeType)json.Type;
            proto.TimeSpend = json.Time;
            proto.Items = json.Input;
            proto.ItemCounts = json.InCounts;
            proto.Results = json.Output ?? Array.Empty<int>();
            proto.ResultCounts = json.OutCounts ?? Array.Empty<int>();
            proto.Description = json.Description;
            proto.GridIndex = json.GridIndex;
            proto.IconPath = json.IconPath;
            proto.NonProductive = json.NonProductive;

            return proto;
        }

        public static TechProtoJson FromProto(TechProto i) =>
            new TechProtoJson
            {
                ID = i.ID,
                Name = i.Name,
                Desc = i.Desc,
                Conclusion = i.Conclusion,
                IsHiddenTech = i.IsHiddenTech,
                PreItem = i.PreItem,
                Published = i.Published,
                Level = i.Level,
                MaxLevel = i.MaxLevel,
                LevelCoef1 = i.LevelCoef1,
                LevelCoef2 = i.LevelCoef2,
                IconPath = i.IconPath,
                IsLabTech = i.IsLabTech,
                PreTechs = i.PreTechs,
                PreTechsImplicit = i.PreTechsImplicit,
                PreTechsMax = i.PreTechsMax,
                Items = i.Items,
                ItemPoints = i.ItemPoints,
                HashNeeded = i.HashNeeded,
                UnlockRecipes = i.UnlockRecipes,
                UnlockFunctions = i.UnlockFunctions,
                UnlockValues = i.UnlockValues,
                AddItems = i.AddItems,
                AddItemCounts = i.AddItemCounts,
                Position = new[] { i.Position.x, i.Position.y },
                PropertyOverrideItems = i.PropertyOverrideItems,
                PropertyItemCounts = i.PropertyItemCounts,
            };

        public static TechProto ToProto(this TechProtoJson json) => ToProto(json, new TechProto());

        public static TechProto ToProto(this TechProtoJson json, TechProto proto)
        {
            proto.ID = json.ID;
            proto.Name = json.Name;
            proto.Desc = json.Desc;
            proto.Conclusion = json.Conclusion;
            proto.IsHiddenTech = json.IsHiddenTech;
            proto.PreItem = json.PreItem ?? Array.Empty<int>();
            proto.Published = json.Published;
            proto.IconPath = json.IconPath;
            proto.IsLabTech = json.IsLabTech;
            proto.PreTechs = json.PreTechs ?? Array.Empty<int>();
            proto.PreTechsImplicit = json.PreTechsImplicit ?? Array.Empty<int>();
            proto.PreTechsMax = json.PreTechsMax;
            proto.Items = json.Items ?? Array.Empty<int>();
            proto.ItemPoints = json.ItemPoints ?? Array.Empty<int>();
            proto.AddItems = json.AddItems ?? Array.Empty<int>();
            proto.AddItemCounts = json.AddItemCounts ?? Array.Empty<int>();
            proto.Position = new Vector2(json.Position[0], json.Position[1]);
            proto.HashNeeded = json.HashNeeded;
            proto.UnlockRecipes = json.UnlockRecipes ?? Array.Empty<int>();
            proto.UnlockFunctions = json.UnlockFunctions ?? Array.Empty<int>();
            proto.UnlockValues = json.UnlockValues ?? Array.Empty<double>();
            proto.Level = json.Level;
            proto.MaxLevel = json.MaxLevel;
            proto.LevelCoef1 = json.LevelCoef1;
            proto.LevelCoef2 = json.LevelCoef2;
            proto.PropertyOverrideItems = json.PropertyOverrideItems ?? Array.Empty<int>();
            proto.PropertyItemCounts = json.PropertyItemCounts ?? Array.Empty<int>();
            return proto;
        }

        public static GoalProtoJson FromProto(GoalProto i) =>
            new GoalProtoJson
            {
                ID = i.ID,
                Name = i.Name,
                TooltipText = i.TooltipText,
                Level = (int)i.Level,
                QueueJumpPriority = i.QueueJumpPriority,
                ParentId = i.ParentId,
                NeedCombatMode = i.NeedCombatMode,
                DisplayParams = i.DisplayParams,
                DeterminatorName = i.DeterminatorName,
                DeterminatorParams = i.DeterminatorParams,
                IgnoreParamsLevel1 = i.IgnoreParamsLevel1,
                IgnoreParamsLevel2 = i.IgnoreParamsLevel2,
                PatchIgnoreParams = i.PatchIgnoreParams,
                PatchCompleteParams = i.PatchCompleteParams,
                OnLoadIgnoreParams = i.OnLoadIgnoreParams,
                EnterQueueIgnoreParams = i.EnterQueueIgnoreParams,
                Childs = i.Childs,
            };

        public static GoalProto ToProto(this GoalProtoJson json) => ToProto(json, new GoalProto());

        public static GoalProto ToProto(this GoalProtoJson json, GoalProto proto)
        {
            proto.ID = json.ID;
            proto.Name = json.Name;
            proto.TooltipText = json.TooltipText;
            proto.Level = (EGoalLevel)json.Level;
            proto.QueueJumpPriority = json.QueueJumpPriority;
            proto.ParentId = json.ParentId;
            proto.NeedCombatMode = json.NeedCombatMode;
            proto.DisplayParams = json.DisplayParams ?? Array.Empty<double>();
            proto.DeterminatorName = json.DeterminatorName;
            proto.DeterminatorParams = json.DeterminatorParams ?? Array.Empty<long>();
            proto.IgnoreParamsLevel1 = json.IgnoreParamsLevel1 ?? Array.Empty<long>();
            proto.IgnoreParamsLevel2 = json.IgnoreParamsLevel2 ?? Array.Empty<long>();
            proto.PatchIgnoreParams = json.PatchIgnoreParams ?? Array.Empty<long>();
            proto.PatchCompleteParams = json.PatchCompleteParams ?? Array.Empty<long>();
            proto.OnLoadIgnoreParams = json.OnLoadIgnoreParams ?? Array.Empty<long>();
            proto.EnterQueueIgnoreParams = json.EnterQueueIgnoreParams ?? Array.Empty<long>();
            proto.Childs = json.Childs ?? Array.Empty<int>();
            return proto;
        }

        public static TutorialProto ToProto(this TutorialProtoJson json) =>
            new TutorialProto
            {
                ID = json.ID,
                Name = json.Name,
                PreText = json.PreText,
                DeterminatorName = json.DeterminatorName,
                DeterminatorParams = json.DeterminatorParams,
            };

        public static PrefabDescJson FromPrefabDesc(PrefabDesc i, int itemID, int modelID) =>
            new PrefabDescJson
            {
                ItemID = itemID,
                ModelID = modelID,
                isAccumulator = i.isAccumulator,
                isAssembler = i.isAssembler,
                isFractionator = i.isFractionator,
                isPowerGen = i.isPowerGen,
                isStation = i.isStation,
                isStellarStation = i.isStellarStation,
                isCollectStation = i.isCollectStation,
                isPowerConsumer = i.isPowerConsumer,
                assemblerSpeed = i.assemblerSpeed,
                assemblerRecipeType = (int)i.assemblerRecipeType,
                workEnergyPerTick = i.workEnergyPerTick,
                idleEnergyPerTick = i.idleEnergyPerTick,
                minerPeriod = i.minerPeriod,
                ejectorChargeFrame = i.ejectorChargeFrame,
                ejectorColdFrame = i.ejectorColdFrame,
                siloChargeFrame = i.siloChargeFrame,
                siloColdFrame = i.siloColdFrame,
                labAssembleSpeed = i.labAssembleSpeed,
                labResearchSpeed = i.labResearchSpeed,
                powerConnectDistance = i.powerConnectDistance,
                powerCoverRadius = i.powerCoverRadius,
                genEnergyPerTick = i.genEnergyPerTick,
                useFuelPerTick = i.useFuelPerTick,
                beltSpeed = i.beltSpeed,
                inserterSTT = i.inserterSTT,
                fluidStorageCount = i.fluidStorageCount,
                fuelMask = i.fuelMask,
                minerType = (int)i.minerType,
                minimapType = i.minimapType,
                maxAcuEnergy = i.maxAcuEnergy,
                maxExcEnergy = i.maxExcEnergy,
                inputEnergyPerTick = i.inputEnergyPerTick,
                outputEnergyPerTick = i.outputEnergyPerTick,
                exchangeEnergyPerTick = i.exchangeEnergyPerTick,
                stationCollectSpeed = i.stationCollectSpeed,
                stationMaxEnergyAcc = i.stationMaxEnergyAcc,
                stationMaxItemCount = i.stationMaxItemCount,
                stationMaxItemKinds = i.stationMaxItemKinds,
                stationMaxShipCount = i.stationMaxShipCount,
                stationMaxDroneCount = i.stationMaxDroneCount,
                AmmoBlastRadius1 = i.AmmoBlastRadius1,
                AmmoMoveAcc = i.AmmoMoveAcc,
                AmmoTurnAcc = i.AmmoTurnAcc,
                turretMuzzleInterval = i.turretMuzzleInterval,
                turretRoundInterval = i.turretRoundInterval,
                turretMaxAttackRange = i.turretMaxAttackRange,
                turretDamageScale = i.turretDamageScale,
                turretSpaceAttackRange = i.turretSpaceAttackRange,
                turretAddEnemyExppBase = i.turretAddEnemyExppBase,
                turretAddEnemyExppCoef = i.turretAddEnemyExppCoef,
                turretAddEnemyHatredBase = i.turretAddEnemyHatredBase,
                turretAddEnemyHatredCoef = i.turretAddEnemyHatredCoef,
                turretAddEnemyThreatBase = i.turretAddEnemyThreatBase,
                turretAddEnemyThreatCoef = i.turretAddEnemyThreatCoef,
                enemyGenMatter = i.enemyGenMatter,
                enemySpMax = i.enemySpMax,
                unitAttackDamage0 = i.unitAttackDamage0,
                unitAttackDamageInc0 = i.unitAttackDamageInc0,
                multiLevel = i.multiLevel,
                storageCol = i.storageCol,
                storageRow = i.storageRow,
                isStorage = i.isStorage,
                subId = i.subId,
                allowBuildInWater = i.allowBuildInWater,
                needBuildInWaterTech = i.needBuildInWaterTech,
                waterTypes = i.waterTypes,
                turretMinAttackRange = i.turretMinAttackRange,
            };

        public static void ToPrefabDesc(this PrefabDescJson json, PrefabDesc desc)
        {
            if (json.isAccumulator != null) desc.isAccumulator = json.isAccumulator.Value;
            if (json.isAssembler != null) desc.isAssembler = json.isAssembler.Value;
            if (json.isFractionator != null) desc.isFractionator = json.isFractionator.Value;
            if (json.isPowerGen != null) desc.isPowerGen = json.isPowerGen.Value;
            if (json.isStation != null) desc.isStation = json.isStation.Value;
            if (json.isStellarStation != null) desc.isStellarStation = json.isStellarStation.Value;
            if (json.isCollectStation != null) desc.isCollectStation = json.isCollectStation.Value;
            if (json.isPowerConsumer != null) desc.isPowerConsumer = json.isPowerConsumer.Value;
            if (json.assemblerSpeed != null) desc.assemblerSpeed = json.assemblerSpeed.Value;
            if (json.assemblerRecipeType != null) desc.assemblerRecipeType = (global::ERecipeType)json.assemblerRecipeType.Value;
            if (json.workEnergyPerTick != null) desc.workEnergyPerTick = json.workEnergyPerTick.Value;
            if (json.idleEnergyPerTick != null) desc.idleEnergyPerTick = json.idleEnergyPerTick.Value;
            if (json.minerPeriod != null) desc.minerPeriod = json.minerPeriod.Value;
            if (json.ejectorChargeFrame != null) desc.ejectorChargeFrame = json.ejectorChargeFrame.Value;
            if (json.ejectorColdFrame != null) desc.ejectorColdFrame = json.ejectorColdFrame.Value;
            if (json.siloChargeFrame != null) desc.siloChargeFrame = json.siloChargeFrame.Value;
            if (json.siloColdFrame != null) desc.siloColdFrame = json.siloColdFrame.Value;
            if (json.labAssembleSpeed != null) desc.labAssembleSpeed = json.labAssembleSpeed.Value;
            if (json.labResearchSpeed != null) desc.labResearchSpeed = json.labResearchSpeed.Value;
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
            if (json.AmmoBlastRadius1 != null) desc.AmmoBlastRadius1 = json.AmmoBlastRadius1.Value;
            if (json.AmmoMoveAcc != null) desc.AmmoMoveAcc = json.AmmoMoveAcc.Value;
            if (json.AmmoTurnAcc != null) desc.AmmoTurnAcc = json.AmmoTurnAcc.Value;
            if (json.turretMuzzleInterval != null) desc.turretMuzzleInterval = json.turretMuzzleInterval.Value;
            if (json.turretRoundInterval != null) desc.turretRoundInterval = json.turretRoundInterval.Value;
            if (json.turretMaxAttackRange != null) desc.turretMaxAttackRange = json.turretMaxAttackRange.Value;
            if (json.turretDamageScale != null) desc.turretDamageScale = json.turretDamageScale.Value;
            if (json.turretSpaceAttackRange != null) desc.turretSpaceAttackRange = json.turretSpaceAttackRange.Value;
            if (json.turretAddEnemyExppBase != null) desc.turretAddEnemyExppBase = json.turretAddEnemyExppBase.Value;
            if (json.turretAddEnemyExppCoef != null) desc.turretAddEnemyExppCoef = json.turretAddEnemyExppCoef.Value;
            if (json.turretAddEnemyHatredBase != null) desc.turretAddEnemyHatredBase = json.turretAddEnemyHatredBase.Value;
            if (json.turretAddEnemyHatredCoef != null) desc.turretAddEnemyHatredCoef = json.turretAddEnemyHatredCoef.Value;
            if (json.turretAddEnemyThreatBase != null) desc.turretAddEnemyThreatBase = json.turretAddEnemyThreatBase.Value;
            if (json.turretAddEnemyThreatCoef != null) desc.turretAddEnemyThreatCoef = json.turretAddEnemyThreatCoef.Value;
            if (json.enemyGenMatter != null) desc.enemyGenMatter = json.enemyGenMatter.Value;
            if (json.enemySpMax != null) desc.enemySpMax = json.enemySpMax.Value;
            if (json.unitAttackDamage0 != null) desc.unitAttackDamage0 = json.unitAttackDamage0.Value;
            if (json.unitAttackDamageInc0 != null) desc.unitAttackDamageInc0 = json.unitAttackDamageInc0.Value;
            if (json.multiLevel != null) desc.multiLevel = json.multiLevel.Value;
            if (json.storageCol != null) desc.storageCol = json.storageCol.Value;
            if (json.storageRow != null) desc.storageRow = json.storageRow.Value;
            if (json.isStorage != null) desc.isStorage = json.isStorage.Value;
            if (json.subId != null) desc.subId = json.subId.Value;
            if (json.allowBuildInWater != null) desc.allowBuildInWater = json.allowBuildInWater.Value;
            if (json.needBuildInWaterTech != null) desc.needBuildInWaterTech = json.needBuildInWaterTech.Value;
            if (json.waterTypes != null) desc.waterTypes = json.waterTypes;
            if (json.turretMinAttackRange != null) desc.turretMinAttackRange = json.turretMinAttackRange.Value;
        }
    }
}

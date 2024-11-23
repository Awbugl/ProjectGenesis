using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using ERecipeType_1 = ERecipeType;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable MemberCanBePrivate.Global

namespace ProjectGenesis.Utils
{
    internal static class JsonHelper
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        internal static ItemProtoJson[] ItemModProtos() => GetJsonContent<ItemProtoJson>("items_mod");

        internal static ItemProtoJson[] ItemVanillaProtos() => GetJsonContent<ItemProtoJson>("items_vanilla");

        internal static RecipeProtoJson[] RecipeProtos() => GetJsonContent<RecipeProtoJson>("recipes");

        internal static TechProtoJson[] TechProtos() => GetJsonContent<TechProtoJson>("techs");

        internal static StringProtoJson[] StringProtos() => GetJsonContent<StringProtoJson>("strings");

        internal static StringProtoJson[] StringModProtos() => GetJsonContent<StringProtoJson>("strings_mod");

        internal static TutorialProtoJson[] TutorialProtos() => GetJsonContent<TutorialProtoJson>("tutorials");

        internal static PrefabDescJson[] PrefabDescs() => GetJsonContent<PrefabDescJson>("prefabDescs");

        internal static T[] GetJsonContent<T>(string json) =>
            JsonConvert.DeserializeObject<T[]>(new StreamReader(Assembly.GetManifestResourceStream($"ProjectGenesis.data.{json}.json"))
               .ReadToEnd());

        internal static string SerializeObject(object obj) =>
            JsonConvert.SerializeObject(obj, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, });

        internal static void ExportAsJson(string path)
        {
            File.WriteAllText($"{path}/techs.json", SerializeObject(LDB.techs.dataArray.Select(TechProtoJson.FromProto)));
            File.WriteAllText($"{path}/items.json", SerializeObject(LDB.items.dataArray.Select(ItemProtoJson.FromProto)));
            File.WriteAllText($"{path}/recipes.json", SerializeObject(LDB.recipes.dataArray.Select(RecipeProtoJson.FromProto)));
        }

        [Serializable]
        public class ItemProtoJson
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string IconPath { get; set; }
            public int GridIndex { get; set; }
            public int StackSize { get; set; }
            public int Type { get; set; }
            public int PreTechOverride { get; set; }
            public int[] DescFields { get; set; }
            public int FuelType { get; set; }
            public long HeatValue { get; set; }
            public float ReactorInc { get; set; }
            public bool IsFluid { get; set; }
            public bool Productive { get; set; }
            public int SubID { get; set; }
            public string MiningFrom { get; set; }
            public string ProduceFrom { get; set; }
            public int Grade { get; set; }
            public int[] Upgrades { get; set; }
            public bool IsEntity { get; set; }
            public bool CanBuild { get; set; }
            public bool BuildInGas { get; set; }
            public int ModelIndex { get; set; }
            public int ModelCount { get; set; }
            public int HpMax { get; set; }
            public int Ability { get; set; }
            public long Potential { get; set; }
            public int BuildIndex { get; set; }
            public int BuildMode { get; set; }
            public int UnlockKey { get; set; }
            public int MechaMaterialID { get; set; }
            public int AmmoType { get; set; }
            public int BombType { get; set; }
            public int CraftType { get; set; }
            public float DropRate { get; set; }
            public int EnemyDropLevel { get; set; }
            public float[] EnemyDropRange { get; set; }
            public float EnemyDropCount { get; set; }
            public int EnemyDropMask { get; set; }
            public float EnemyDropMaskRatio { get; set; }

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

            public ItemProto ToProto() => ToProto(new ItemProto());

            public ItemProto ToProto(ItemProto proto)
            {
                proto.ID = ID;
                proto.Name = Name;
                proto.Description = Description;
                proto.IconPath = IconPath;
                proto.GridIndex = GridIndex;
                proto.StackSize = StackSize;
                proto.FuelType = FuelType;
                proto.HeatValue = HeatValue;
                proto.ReactorInc = ReactorInc;
                proto.DescFields = DescFields ?? Array.Empty<int>();
                proto.IsFluid = IsFluid;
                proto.Type = (EItemType)Type;
                proto.SubID = SubID;
                proto.MiningFrom = MiningFrom;
                proto.ProduceFrom = ProduceFrom;
                proto.Grade = Grade;
                proto.Upgrades = Upgrades ?? Array.Empty<int>();
                proto.IsEntity = IsEntity;
                proto.CanBuild = CanBuild;
                proto.BuildInGas = BuildInGas;
                proto.ModelIndex = ModelIndex;
                proto.ModelCount = ModelCount;
                proto.HpMax = HpMax;
                proto.Ability = Ability;
                proto.Potential = Potential;
                proto.BuildIndex = BuildIndex;
                proto.BuildMode = BuildMode;
                proto.UnlockKey = UnlockKey;
                proto.MechaMaterialID = MechaMaterialID;
                proto.PreTechOverride = PreTechOverride;
                proto.Productive = Productive;
                proto.AmmoType = (EAmmoType)AmmoType;
                proto.BombType = (EBombType)BombType;
                proto.CraftType = CraftType;
                proto.DropRate = DropRate;
                proto.EnemyDropLevel = EnemyDropLevel;
                proto.EnemyDropRange = new Vector2(EnemyDropRange[0], EnemyDropRange[1]);
                proto.EnemyDropCount = EnemyDropCount;
                proto.EnemyDropMask = EnemyDropMask;
                proto.EnemyDropMaskRatio = EnemyDropMaskRatio;

                return proto;
            }
        }

        [Serializable]
        public class RecipeProtoJson
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string IconPath { get; set; }
            public int Type { get; set; }
            public int GridIndex { get; set; }
            public int Time { get; set; }
            public int[] Input { get; set; }
            public int[] InCounts { get; set; }
            public int[] Output { get; set; }
            public int[] OutCounts { get; set; }
            public bool Explicit { get; set; }
            public bool Handcraft { get; set; }
            public bool NonProductive { get; set; }

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

            public RecipeProto ToProto() => ToProto(new RecipeProto());

            public RecipeProto ToProto(RecipeProto proto)
            {
                proto.ID = ID;
                proto.Explicit = Explicit;
                proto.Name = Name;
                proto.Handcraft = Handcraft;
                proto.Type = (ERecipeType_1)Type;
                proto.TimeSpend = Time;
                proto.Items = Input;
                proto.ItemCounts = InCounts;
                proto.Results = Output ?? Array.Empty<int>();
                proto.ResultCounts = OutCounts ?? Array.Empty<int>();
                proto.Description = Description;
                proto.GridIndex = GridIndex;
                proto.IconPath = IconPath;
                proto.NonProductive = NonProductive;

                return proto;
            }
        }

        [Serializable]
        public class TechProtoJson
        {
            public int ID { get; set; }
            public string Name { get; set; }

            public string IconPath { get; set; }
            public string Desc { get; set; }
            public string Conclusion { get; set; }
            public bool IsHiddenTech { get; set; }
            public int[] PreItem { get; set; }
            public float[] Position { get; set; }
            public int[] PreTechs { get; set; }
            public int[] PreTechsImplicit { get; set; }
            public int[] Items { get; set; }
            public int[] ItemPoints { get; set; }
            public long HashNeeded { get; set; }
            public int[] UnlockRecipes { get; set; }
            public int[] UnlockFunctions { get; set; }
            public double[] UnlockValues { get; set; }
            public bool Published { get; set; }
            public int Level { get; set; }
            public int MaxLevel { get; set; }
            public int LevelCoef1 { get; set; }
            public int LevelCoef2 { get; set; }
            public bool IsLabTech { get; set; }
            public bool PreTechsMax { get; set; }
            public int[] AddItems { get; set; }
            public int[] AddItemCounts { get; set; }
            public int[] PropertyOverrideItems { get; set; }
            public int[] PropertyItemCounts { get; set; }

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
                    Position = new[] { i.Position.x, i.Position.y, },
                    PropertyOverrideItems = i.PropertyOverrideItems,
                    PropertyItemCounts = i.PropertyItemCounts,
                };

            public TechProto ToProto() => ToProto(new TechProto());

            public TechProto ToProto(TechProto proto)
            {
                proto.ID = ID;
                proto.Name = Name;
                proto.Desc = Desc;
                proto.Conclusion = Conclusion;
                proto.IsHiddenTech = IsHiddenTech;
                proto.PreItem = PreItem ?? Array.Empty<int>();
                proto.Published = Published;
                proto.IconPath = IconPath;
                proto.IsLabTech = IsLabTech;
                proto.PreTechs = PreTechs ?? Array.Empty<int>();
                proto.PreTechsImplicit = PreTechsImplicit ?? Array.Empty<int>();
                proto.PreTechsMax = PreTechsMax;
                proto.Items = Items ?? Array.Empty<int>();
                proto.ItemPoints = ItemPoints ?? Array.Empty<int>();
                proto.AddItems = AddItems ?? Array.Empty<int>();
                proto.AddItemCounts = AddItemCounts ?? Array.Empty<int>();
                proto.Position = new Vector2(Position[0], Position[1]);
                proto.HashNeeded = HashNeeded;
                proto.UnlockRecipes = UnlockRecipes ?? Array.Empty<int>();
                proto.UnlockFunctions = UnlockFunctions ?? Array.Empty<int>();
                proto.UnlockValues = UnlockValues ?? Array.Empty<double>();
                proto.Level = Level;
                proto.MaxLevel = MaxLevel;
                proto.LevelCoef1 = LevelCoef1;
                proto.LevelCoef2 = LevelCoef2;
                proto.PropertyOverrideItems = PropertyOverrideItems ?? Array.Empty<int>();
                proto.PropertyItemCounts = PropertyItemCounts ?? Array.Empty<int>();

                return proto;
            }
        }

        [Serializable]
        public class StringProtoJson
        {
            public string Name { get; set; }
            public string ZHCN { get; set; }
            public string ENUS { get; set; }
        }

        [Serializable]
        public class TutorialProtoJson
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string PreText { get; set; }
            public string DeterminatorName { get; set; }
            public long[] DeterminatorParams { get; set; }

            public TutorialProto ToProto() =>
                new TutorialProto
                {
                    ID = ID,
                    Name = Name,
                    PreText = PreText,
                    DeterminatorName = DeterminatorName,
                    DeterminatorParams = DeterminatorParams,
                };
        }

        [Serializable]
        public class PrefabDescJson
        {
            public int ItemID { get; set; }
            public int ModelID { get; set; }
            public bool? isAccumulator { get; set; } = null;
            public bool? isAssembler { get; set; } = null;
            public bool? isCollectStation { get; set; } = null;
            public bool? isFractionator { get; set; } = null;
            public bool? isPowerGen { get; set; } = null;
            public bool? isPowerConsumer { get; set; } = null;
            public bool? isStation { get; set; } = null;
            public bool? isStellarStation { get; set; } = null;
            public int? assemblerRecipeType { get; set; } = null;
            public long? idleEnergyPerTick { get; set; } = null;
            public long? workEnergyPerTick { get; set; } = null;
            public int? assemblerSpeed { get; set; } = null;
            public int? minerPeriod { get; set; } = null;
            public int? ejectorChargeFrame { get; set; } = null;
            public int? ejectorColdFrame { get; set; } = null;
            public int? siloChargeFrame { get; set; } = null;
            public int? siloColdFrame { get; set; } = null;
            public int? labAssembleSpeed { get; set; } = null;
            public float? labResearchSpeed { get; set; } = null;
            public float? powerConnectDistance { get; set; } = null;
            public float? powerCoverRadius { get; set; } = null;
            public long? genEnergyPerTick { get; set; } = null;
            public long? useFuelPerTick { get; set; } = null;
            public int? beltSpeed { get; set; } = null;
            public int? inserterSTT { get; set; } = null;
            public int? fluidStorageCount { get; set; } = null;
            public int? fuelMask { get; set; } = null;
            public int? minerType { get; set; } = null;
            public int? minimapType { get; set; } = null;
            public int? stationCollectSpeed { get; set; } = null;
            public long? maxAcuEnergy { get; set; } = null;
            public long? inputEnergyPerTick { get; set; } = null;
            public long? outputEnergyPerTick { get; set; } = null;
            public long? maxExcEnergy { get; set; } = null;
            public long? exchangeEnergyPerTick { get; set; } = null;
            public long? stationMaxEnergyAcc { get; set; } = null;
            public int? stationMaxItemCount { get; set; } = null;
            public int? stationMaxItemKinds { get; set; } = null;
            public int? stationMaxDroneCount { get; set; } = null;
            public int? stationMaxShipCount { get; set; } = null;
            public float? AmmoBlastRadius1 { get; set; } = null;
            public float? AmmoMoveAcc { get; set; } = null;
            public float? AmmoTurnAcc { get; set; } = null;
            public int? turretMuzzleInterval { get; set; } = null;
            public int? turretRoundInterval { get; set; } = null;
            public float? turretMaxAttackRange { get; set; } = null;
            public float? turretDamageScale { get; set; } = null;
            public float? turretSpaceAttackRange { get; set; } = null;
            public int? turretAddEnemyExppBase { get; set; } = null;
            public float? turretAddEnemyExppCoef { get; set; } = null;
            public int? turretAddEnemyHatredBase { get; set; } = null;
            public float? turretAddEnemyHatredCoef { get; set; } = null;
            public int? turretAddEnemyThreatBase { get; set; } = null;
            public float? turretAddEnemyThreatCoef { get; set; } = null;
            public int? enemyGenMatter { get; set; } = null;
            public bool? multiLevel { get; set; } = null;
            public int? storageCol { get; set; } = null;
            public int? storageRow { get; set; } = null;
            public bool? isStorage { get; set; } = null;
            public bool? allowBuildInWater { get; set; } = null;
            public bool? needBuildInWaterTech { get; set; } = null;
            public int[] waterTypes { get; set; } = null;
            public float? turretMinAttackRange { get; set; } = null;

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
                    multiLevel = i.multiLevel,
                    storageCol = i.storageCol,
                    storageRow = i.storageRow,
                    isStorage = i.isStorage,
                    allowBuildInWater = i.allowBuildInWater,
                    needBuildInWaterTech = i.needBuildInWaterTech,
                    waterTypes = i.waterTypes,
                    turretMinAttackRange = i.turretMinAttackRange,
                };

            public void ToPrefabDesc(PrefabDesc desc)
            {
                if (isAccumulator != null) desc.isAccumulator = isAccumulator.Value;

                if (isAssembler != null) desc.isAssembler = isAssembler.Value;

                if (isFractionator != null) desc.isFractionator = isFractionator.Value;

                if (isPowerGen != null) desc.isPowerGen = isPowerGen.Value;

                if (isStation != null) desc.isStation = isStation.Value;

                if (isStellarStation != null) desc.isStellarStation = isStellarStation.Value;

                if (isCollectStation != null) desc.isCollectStation = isCollectStation.Value;

                if (isPowerConsumer != null) desc.isPowerConsumer = isPowerConsumer.Value;

                if (assemblerSpeed != null) desc.assemblerSpeed = assemblerSpeed.Value;

                if (assemblerRecipeType != null) desc.assemblerRecipeType = (ERecipeType_1)assemblerRecipeType.Value;

                if (workEnergyPerTick != null) desc.workEnergyPerTick = workEnergyPerTick.Value;

                if (idleEnergyPerTick != null) desc.idleEnergyPerTick = idleEnergyPerTick.Value;

                if (minerPeriod != null) desc.minerPeriod = minerPeriod.Value;

                if (ejectorChargeFrame != null) desc.ejectorChargeFrame = ejectorChargeFrame.Value;

                if (ejectorColdFrame != null) desc.ejectorColdFrame = ejectorColdFrame.Value;

                if (siloChargeFrame != null) desc.siloChargeFrame = siloChargeFrame.Value;

                if (siloColdFrame != null) desc.siloColdFrame = siloColdFrame.Value;

                if (labAssembleSpeed != null) desc.labAssembleSpeed = labAssembleSpeed.Value;

                if (labResearchSpeed != null) desc.labResearchSpeed = labResearchSpeed.Value;

                if (powerConnectDistance != null) desc.powerConnectDistance = powerConnectDistance.Value;

                if (powerCoverRadius != null) desc.powerCoverRadius = powerCoverRadius.Value;

                if (genEnergyPerTick != null) desc.genEnergyPerTick = genEnergyPerTick.Value;

                if (useFuelPerTick != null) desc.useFuelPerTick = useFuelPerTick.Value;

                if (beltSpeed != null) desc.beltSpeed = beltSpeed.Value;

                if (inserterSTT != null) desc.inserterSTT = inserterSTT.Value;

                if (fluidStorageCount != null) desc.fluidStorageCount = fluidStorageCount.Value;

                if (fuelMask != null) desc.fuelMask = fuelMask.Value;

                if (minerType != null) desc.minerType = (EMinerType)minerType.Value;

                if (minimapType != null) desc.minimapType = minimapType.Value;

                if (maxAcuEnergy != null) desc.maxAcuEnergy = maxAcuEnergy.Value;

                if (maxExcEnergy != null) desc.maxExcEnergy = maxExcEnergy.Value;

                if (inputEnergyPerTick != null) desc.inputEnergyPerTick = inputEnergyPerTick.Value;

                if (outputEnergyPerTick != null) desc.outputEnergyPerTick = outputEnergyPerTick.Value;

                if (exchangeEnergyPerTick != null) desc.exchangeEnergyPerTick = exchangeEnergyPerTick.Value;

                if (stationCollectSpeed != null) desc.stationCollectSpeed = stationCollectSpeed.Value;

                if (stationMaxEnergyAcc != null) desc.stationMaxEnergyAcc = stationMaxEnergyAcc.Value;

                if (stationMaxItemCount != null) desc.stationMaxItemCount = stationMaxItemCount.Value;

                if (stationMaxItemKinds != null) desc.stationMaxItemKinds = stationMaxItemKinds.Value;

                if (stationMaxShipCount != null) desc.stationMaxShipCount = stationMaxShipCount.Value;

                if (stationMaxDroneCount != null) desc.stationMaxDroneCount = stationMaxDroneCount.Value;

                if (AmmoBlastRadius1 != null) desc.AmmoBlastRadius1 = AmmoBlastRadius1.Value;

                if (AmmoMoveAcc != null) desc.AmmoMoveAcc = AmmoMoveAcc.Value;

                if (AmmoTurnAcc != null) desc.AmmoTurnAcc = AmmoTurnAcc.Value;

                if (turretMuzzleInterval != null) desc.turretMuzzleInterval = turretMuzzleInterval.Value;

                if (turretRoundInterval != null) desc.turretRoundInterval = turretRoundInterval.Value;

                if (turretMaxAttackRange != null) desc.turretMaxAttackRange = turretMaxAttackRange.Value;

                if (turretDamageScale != null) desc.turretDamageScale = turretDamageScale.Value;

                if (turretSpaceAttackRange != null) desc.turretSpaceAttackRange = turretSpaceAttackRange.Value;

                if (turretAddEnemyExppBase != null) desc.turretAddEnemyExppBase = turretAddEnemyExppBase.Value;

                if (turretAddEnemyExppCoef != null) desc.turretAddEnemyExppCoef = turretAddEnemyExppCoef.Value;

                if (turretAddEnemyHatredBase != null) desc.turretAddEnemyHatredBase = turretAddEnemyHatredBase.Value;

                if (turretAddEnemyHatredCoef != null) desc.turretAddEnemyHatredCoef = turretAddEnemyHatredCoef.Value;

                if (turretAddEnemyThreatBase != null) desc.turretAddEnemyThreatBase = turretAddEnemyThreatBase.Value;

                if (turretAddEnemyThreatCoef != null) desc.turretAddEnemyThreatCoef = turretAddEnemyThreatCoef.Value;

                if (enemyGenMatter != null) desc.enemyGenMatter = enemyGenMatter.Value;

                if (multiLevel != null) desc.multiLevel = multiLevel.Value;

                if (storageCol != null) desc.storageCol = storageCol.Value;

                if (storageRow != null) desc.storageRow = storageRow.Value;

                if (isStorage != null) desc.isStorage = isStorage.Value;

                if (allowBuildInWater != null) desc.allowBuildInWater = allowBuildInWater.Value;

                if (needBuildInWaterTech != null) desc.needBuildInWaterTech = needBuildInWaterTech.Value;

                if (waterTypes != null) desc.waterTypes = waterTypes;

                if (turretMinAttackRange != null) desc.turretMinAttackRange = turretMinAttackRange.Value;
            }
        }
    }
}

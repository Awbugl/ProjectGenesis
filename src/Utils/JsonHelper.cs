using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("ProjectGenesis.DevTools")]
[assembly: InternalsVisibleTo("ProjectGenesis.Packer")]

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

        internal static GoalProtoJson[] GoalProtos() => GetJsonContent<GoalProtoJson>("goals");

        internal static PrefabDescJson[] PrefabDescs() => GetJsonContent<PrefabDescJson>("prefabDescs");

        internal static T[] GetJsonContent<T>(string json) =>
            JsonConvert.DeserializeObject<T[]>(new StreamReader(Assembly.GetManifestResourceStream($"ProjectGenesis.data.{json}.json"))
               .ReadToEnd());

        internal static string SerializeObject(object obj) =>
            JsonConvert.SerializeObject(obj, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, });

        internal static void ExportAsJson(string path)
        {
            File.WriteAllText($"{path}/techs.json", SerializeObject(LDB.techs.dataArray.Select(ProtoJsonConverter.FromProto)));
            File.WriteAllText($"{path}/items.json", SerializeObject(LDB.items.dataArray.Select(ProtoJsonConverter.FromProto)));
            File.WriteAllText($"{path}/recipes.json", SerializeObject(LDB.recipes.dataArray.Select(ProtoJsonConverter.FromProto)));
            File.WriteAllText($"{path}/goals.json", SerializeObject(LDB.goals.dataArray.Select(ProtoJsonConverter.FromProto)));
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
        }

        [Serializable]
        public class GoalProtoJson
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string TooltipText { get; set; }
            public int Level { get; set; }
            public int QueueJumpPriority { get; set; }
            public int ParentId { get; set; }
            public int NeedCombatMode { get; set; }
            public double[] DisplayParams { get; set; }
            public string DeterminatorName { get; set; }
            public long[] DeterminatorParams { get; set; }
            public long[] IgnoreParamsLevel1 { get; set; }
            public long[] IgnoreParamsLevel2 { get; set; }
            public long[] PatchIgnoreParams { get; set; }
            public long[] PatchCompleteParams { get; set; }
            public long[] OnLoadIgnoreParams { get; set; }
            public long[] EnterQueueIgnoreParams { get; set; }
            public int[] Childs { get; set; }
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
            public int? enemySpMax { get; set; } = null;
            public int? unitAttackDamage0 { get; set; } = null;
            public int? unitAttackDamageInc0 { get; set; } = null;
            public bool? multiLevel { get; set; } = null;
            public int? storageCol { get; set; } = null;
            public int? storageRow { get; set; } = null;
            public bool? isStorage { get; set; } = null;
            public int? subId { get; set; } = null;
            public bool? allowBuildInWater { get; set; } = null;
            public bool? needBuildInWaterTech { get; set; } = null;
            public int[] waterTypes { get; set; } = null;
            public float? turretMinAttackRange { get; set; } = null;
        }
    }
}

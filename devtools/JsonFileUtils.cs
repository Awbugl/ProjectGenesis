using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace ProjectGenesis
{
    internal static class JsonFileUtils
    {
        private const string BaseDir = @"D:\Git\ProjectGenesis\data";

        private static readonly string RecipeFile = Path.Combine(BaseDir, "recipes.json"),
                                       ItemVanilla = Path.Combine(BaseDir, "items_vanilla.json"),
                                       ItemMod = Path.Combine(BaseDir, "items_mod.json"),
                                       ItemExport = Path.Combine(BaseDir, "items.json"),
                                       TechFile = Path.Combine(BaseDir, "techs.json"),
                                       StringsFile = Path.Combine(BaseDir, "strings.json");

        public static List<RecipeProtoJson> LoadRecipes() => Load<RecipeProtoJson>(RecipeFile);

        public static List<ItemProtoJson> LoadItemsVanilla() => Load<ItemProtoJson>(ItemVanilla);

        public static List<ItemProtoJson> LoadItemsMod() => Load<ItemProtoJson>(ItemMod);
        
        public static List<ItemProtoJson> LoadItemsExport() => Load<ItemProtoJson>(ItemExport);

        public static List<TechProtoJson> LoadTechs() => Load<TechProtoJson>(TechFile);

        public static List<StringProtoJson> LoadStrings() => Load<StringProtoJson>(StringsFile);

        public static void SaveRecipes(List<RecipeProtoJson> data) => Save(RecipeFile, data);

        public static void SaveItemsVanilla(List<ItemProtoJson> data) => Save(ItemVanilla, data);

        public static void SaveItemsMod(List<ItemProtoJson> data) => Save(ItemMod, data);

        public static void SaveTechs(List<TechProtoJson> data) => Save(TechFile, data);

        public static void SaveStrings(List<StringProtoJson> data) => Save(StringsFile, data);

        private static List<T> Load<T>(string path) => JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path));

        private static void Save<T>(string path, List<T> data) =>
            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    [Serializable]
    public class ItemProtoJson
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public string IconTag { get; set; }
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
        public string IconTag { get; set; }
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
        public string IconTag { get; set; }
        public string Desc { get; set; }
        public string Conclusion { get; set; }
        public bool IsHiddenTech { get; set; }
        public int[] PreItem { get; set; }
        public int[] Position { get; set; }
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
    public class StringProtoJson
    {
        public string Name { get; set; }
        public string ZHCN { get; set; }
        public string ENUS { get; set; }
    }
}

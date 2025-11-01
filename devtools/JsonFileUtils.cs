using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using static ProjectGenesis.Utils.JsonHelper;
using Formatting = Newtonsoft.Json.Formatting;

namespace ProjectGenesis
{
    internal static class JsonFileUtils
    {
        private const string BaseDir = @"D:\Git\ProjectGenesis\data";

        private static readonly string RecipeFile = Path.Combine(BaseDir, "recipes.json"),
                                       ItemVanilla = Path.Combine(BaseDir, "items_vanilla.json"),
                                       ItemMod = Path.Combine(BaseDir, "items_mod.json"),
                                       TechFile = Path.Combine(BaseDir, "techs.json");

        public static List<RecipeProtoJson> LoadRecipes() => Load<RecipeProtoJson>(RecipeFile);

        public static List<ItemProtoJson> LoadItemsVanilla() => Load<ItemProtoJson>(ItemVanilla);

        public static List<ItemProtoJson> LoadItemsMod() => Load<ItemProtoJson>(ItemMod);

        public static List<TechProtoJson> LoadTechs() => Load<TechProtoJson>(TechFile);

        public static void SaveRecipes(List<RecipeProtoJson> data) => Save(RecipeFile, data);

        public static void SaveItemsVanilla(List<ItemProtoJson> data) => Save(ItemVanilla, data);

        public static void SaveItemsMod(List<ItemProtoJson> data) => Save(ItemMod, data);

        private static List<T> Load<T>(string path) => JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path));

        private static void Save<T>(string path, List<T> data) =>
            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
    }
}

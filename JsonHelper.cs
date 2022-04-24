using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ProjectGenesis
{
    public static class JsonHelper
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        internal static ItemProtoJson[] ItemProtos() =>
            JsonConvert.DeserializeObject<ItemProtoJson[]>(new StreamReader(Assembly
                                                                                .GetManifestResourceStream("ProjectGenesis.items"))
                                                               .ReadToEnd());

        internal static RecipeProtoJson[] RecipeProtos() =>
            JsonConvert.DeserializeObject<RecipeProtoJson[]>(new StreamReader(Assembly
                                                                                  .GetManifestResourceStream("ProjectGenesis.recipes"))
                                                                 .ReadToEnd());

        internal static TechProtoJson[] TechProtos() =>
            JsonConvert.DeserializeObject<TechProtoJson[]>(new StreamReader(Assembly
                                                                                .GetManifestResourceStream("ProjectGenesis.techs"))
                                                               .ReadToEnd());


        [Serializable]
        public class ItemProtoJson
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string IconPath { get; set; }
            public int GridIndex { get; set; }
            public int StackSize { get; set; }
            public int PreTech { get; set; }
            public int FuelType { get; set; }
            public long HeatValue { get; set; }
            public float ReactorInc { get; set; }
            public int[] DescFields { get; set; }
            public bool IsFluid { get; set; }

            public int Type { get; set; }
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
            public int PreTechOverride { get; set; }
            public bool Productive { get; set; }
            public int MechaMaterialID { get; set; }
        }

        [Serializable]
        public class RecipeProtoJson
        {
            public int ID { get; set; }
            public int Type { get; set; }
            public int Time { get; set; }
            public int[] Input { get; set; }
            public int[] InCounts { get; set; }
            public int[] Output { get; set; }
            public int[] OutCounts { get; set; }
            public string Description { get; set; }
            public int PreTech { get; set; }
            public int GridIndex { get; set; }
            public string Name { get; set; }
            public string IconPath { get; set; }
            public bool Explicit { get; set; }
            public bool Handcraft { get; set; }
            public bool NonProductive { get; set; }
        }

        [Serializable]
        public class TechProtoJson
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Desc { get; set; }
            public string Conclusion { get; set; }
            public bool Published { get; set; }
            public int Level { get; set; }
            public int MaxLevel { get; set; }
            public int LevelCoef1 { get; set; }
            public int LevelCoef2 { get; set; }
            public string IconPath { get; set; }
            public bool IsLabTech { get; set; }
            public int[] PreTechs { get; set; }
            public int[] PreTechsImplicit { get; set; }
            public bool PreTechsMax { get; set; }
            public int[] Items { get; set; }
            public int[] ItemPoints { get; set; }
            public long HashNeeded { get; set; }
            public int[] UnlockRecipes { get; set; }
            public int[] UnlockFunctions { get; set; }
            public double[] UnlockValues { get; set; }
            public int[] AddItems { get; set; }
            public int[] AddItemCounts { get; set; }
            public float[] Position { get; set; }
        }
    }
}

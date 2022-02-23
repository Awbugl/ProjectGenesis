using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ProjectGenesis
{
    public static class CustomFunction
    {
        //图标加载到流
        //AssetBundle Bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProjectGenesis.icon"));
        public static class VanillaRecipeEdit
        {
            public static  void Name(int id, string name)
            {
                LDB.recipes.Select(id).name = name;
                LDB.recipes.Select(id).Name = name;
            }
            public static  void Desc(int id, string desc)
            {
                LDB.recipes.Select(id).description = desc;
                LDB.recipes.Select(id).Description = desc;
            }
            public  static void Item(int id, int[] item)
            {
                var recipe = LDB.recipes.Select(id);
                recipe.Items = item;
            }
            public  static void ItemCount(int id, int[] itemCount)
            {
                var recipe = LDB.recipes.Select(id);
                recipe.ItemCounts = itemCount;
            }
            public  static void Result(int id, int[] result)
            {
                var recipe = LDB.recipes.Select(id);
                recipe.Results = result;
            }
            public  static void ResultCount(int id, int[] resultCount)
            {
                var recipe = LDB.recipes.Select(id);
                recipe.ResultCounts = resultCount;
            }
            public  static void NeedTime(int id, int needTime)
            {
                var recipe = LDB.recipes.Select(id);
                recipe.TimeSpend = needTime * 60;
            }
            public static  void GridIndex(int id, int gridIndex)
            {
                var recipe = LDB.recipes.Select(id);
                recipe.GridIndex = gridIndex;
            }
            public static  void RHandCraft(int id, bool boolean)
            {
                var recipe = LDB.recipes.Select(id);
                recipe.Handcraft = boolean;
            }
            public  static void PreTech(int id, TechProto tech)
            {
                LDB.recipes.Select(id).preTech = tech;
            }
            public static  void RecipeProto(int id)
            {
                LDB.recipes.Select(id);
            }
        }

        public static  class CreateRecipe
        {
            //从流加载图标
            public static Dictionary<string, Sprite>   Sprites;
              

            
            public static  void RecipeID(RecipeProto recipe, int id)
            {
                recipe.ID = id;
            }
            public  static void RecipeName(RecipeProto recipe, string name)
            {
                recipe.name = name;
                recipe.Name = name;
            }
            public  static void RecipeDesc(RecipeProto recipe, string desc)
            {
                recipe.description = desc;
                recipe.Description = desc;
            }
            public static  void RecipeGridIndex(RecipeProto recipe, int gridIndex)
            {
                recipe.GridIndex = gridIndex;
            }
            public  static void RecipeData(RecipeProto recipe, int[] recipeItem, int[] recipeItemCount, int[] recipeResult, int[] recipeResultCount)
            {
                recipe.Items = recipeItem;
                recipe.ItemCounts = recipeItemCount;
                recipe.Results = recipeResult;
                recipe.ResultCounts = recipeResultCount;
            }
            public static  void RecipeNeedTime(RecipeProto recipe, int needTime)
            {
                recipe.TimeSpend = needTime * 60;
            }
            public  static void RecipeHandCraft(RecipeProto recipe, bool boolean)
            {
                recipe.Handcraft = boolean;
            }
            public static  void RecipePreTech(RecipeProto recipe, TechProto tech)
            {
                recipe.preTech = tech;
            }
            
            public static void LoadSprites()
            {
                var loadIconAsset = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly()
                                                                       .GetManifestResourceStream("ProjectGenesis.genesistexpack"));

                Sprites = new[]
                        {
                            "乙烯", "氨", "苯酚", "丙酮", "二氨基联苯胺", "二甲苯", "二氯联苯胺", "二硝基氯苯", "甘油", "环氧氯丙烷",
                            "间苯二甲酸二苯酯", "邻苯二甲酸", "氯苯", "氢氯酸", "三氯化铁", "双酚A", "硝酸", "盐水", "煤焦油", "超低功率IC晶片",
                            "低功率IC晶片", "标准功率IC晶片", "高功率IC晶片", "超高功率IC晶片", "生物IC晶片", "基础机械组件", "先进机械组件",
                            "尖端机械组件", "超级机械组件", "终极机械组件", "塑料基板", "覆层塑料基板", "钛蚀刻覆层塑料基板", "生物基板", "覆层生物基板",
                            "钨蚀刻覆层生物基板", "生物微晶元件", "先进生物电路", "超生物计算集群", "生物神经纤维", "超级纳米电脑", "湿件主机", "钛齿轮",
                            "钨齿轮", "铜线圈", "石墨线圈", "钛线圈", "稳定矩阵", "虚空矩阵", "起源矩阵", "硅晶圆", "钛掺杂的硅晶圆", "钨掺杂的硅晶圆",
                            "钨矿", "钨块", "钨复合金属", "钨强化玻璃", "氯化钠", "硫酸钠", "硝酸钠", "氢氧化钠", "氧气", "聚苯并咪唑",
                            "活性生物纤维", "环氧树脂", "空燃料棒", "创世之书", "虚空之书", "起源之书", "精炼精炼油", "精炼氢", "高效重氢", "高效反物质",
                            "高效奇异物质", "极效石墨烯", "石油裂化", "电解水", "氢燃料", "氘核燃料", "反物质燃料", "电路板", "电路基板", "精炼科技",
                            "钨钢", "生物神经", "主机科技", "基础组件", "先进组件", "尖端组件", "玻璃科技", "虚拟技术革新", "化工科技", "高效硅晶圆",
                            "高效钛晶圆", "高效钨晶圆", "剧毒有机液体", "中文图标", "英文图标", "高效塑料","重燃油", "轻燃油", "燃油精炼", "含硫重燃油", 
                    "含硫轻燃油", "柴油", "硝基柴油", "甲醇", "乙酸", "乙酸甲酯", "次氯酸", "一氧化碳", "二氧化碳", "四硝基甲烷","偏二甲肼",  "硫化氢", "火箭燃料", 
                    "火箭燃料棒", "硝基柴油燃料棒", "偏二甲肼燃料棒", "高效微晶元件", "高效处理器", "高效量子芯片", "高效硫酸", "高效氢氯酸"
                        }.ToDictionary(spriteName => spriteName,
                                       spriteName => loadIconAsset.LoadAsset<Sprite>(spriteName));
            }
            
            public static void RecipeIcon(Proto proto, string spriteName)
            {
                Traverse.Create(proto).Field("_iconSprite").SetValue(Sprites[spriteName]);
            }
        }
        public static  class VanillaItemEdit
        {
            public  static void Name(int id, string name)
            {
                LDB.items.Select(id).name = name;
                LDB.items.Select(id).Name = name;
            }
            public  static void Desc(int id, string desc)
            {
                LDB.items.Select(id).description = desc;
                LDB.items.Select(id).Description = desc;
            }
            public static  void GridIndex(int id, int gridIndex)
            {
                LDB.items.Select(id).GridIndex = gridIndex;
            }
            public static  void PreTech(int id, TechProto tech)
            {
                LDB.items.Select(id).preTech = tech;
            }
        }
        public static  class CreateItem
        {
            public static  void ItemData(ItemProto item, int id, string name, string desc, int gridIndex, TechProto tech)
            {
                item.ID = id;
                item.name = name;
                item.Name = name;
                item.description = desc;
                item.Description = desc;
                item.GridIndex = gridIndex;
                item.preTech = tech;
            }
            public static  void ItemRecipe(ItemProto item, RecipeProto recipe)
            {
                item.handcrafts = new List<RecipeProto> { recipe };
                item.recipes = new List<RecipeProto> { recipe };
                item.handcraft = recipe;
                item.maincraft = recipe;
            }
            public static  void HandRecipe(ItemProto item, RecipeProto recipe)
            {
                item.handcrafts = new List<RecipeProto> { recipe };
                item.handcraft = recipe;
                item.recipes = new List<RecipeProto> { recipe };
                item.maincraft = recipe;
            }
            public static  void NoHandRecipe(ItemProto item, RecipeProto recipe)
            {
                item.handcrafts = new List<RecipeProto>();
                item.recipes = new List<RecipeProto> { recipe };
                item.maincraft = recipe;
                item.handcraft = recipe;
            }
            public static  void NoRecipe(ItemProto item)
            {
                item.handcrafts = new List<RecipeProto>();
                item.recipes = new List<RecipeProto>();
                item.maincraft = null;
                item.handcraft = null;
            }
            /*
            public void ItemIcon(ItemProto Item, string IconName)
            {
                Traverse.Create(Item).Field("_iconSprite").SetValue(AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProjectGenesis.texpack")).LoadAsset<Sprite>(IconName));
            }
            */
        }
        public  static class VanillaTechEdit
        {
            public  static void Name(int id, string name) 
            {
                LDB.techs.Select(id).name = name;
                LDB.techs.Select(id).Name = name;
            }
            public static  void Desc(int id, string desc)
            {
                LDB.techs.Select(id).description = desc;
                LDB.techs.Select(id).Desc = desc;
            }
            public  static void Vector(int id, Vector2 vector) 
            {
                LDB.techs.Select(id).Position = vector;
            }
            public  static void UnlockRecipeArray(int id, RecipeProto[] recipe) 
            {
                LDB.techs.Select(id).unlockRecipeArray = recipe;
            }
            public static  void UnlockRecipe(int id, int[] recipe)
            {
                LDB.techs.Select(id).UnlockRecipes = recipe;
            }
            public static  void PreTechArray(int id, TechProto[] tech) 
            {
                LDB.techs.Select(id).preTechArray = tech;
            }
            public  static void PostTechArray(int id, TechProto[] tech)
            {
                LDB.techs.Select(id).postTechArray = tech;
            }
            public  static void PreTech(int id, int[] tech) 
            {
                LDB.techs.Select(id).PreTechs = tech;
            }
            public static  void CanLab(int id, bool boolean) 
            {
                LDB.techs.Select(id).Published = boolean;    
            }
            public static  void ItemArray(int id, ItemProto[] item) 
            {
                LDB.techs.Select(id).itemArray = item;
            }
            public  static void ItemAndPoint(int id, int[] item, int[] point)
            {
                LDB.techs.Select(id).Items = item;
                LDB.techs.Select(id).ItemPoints = point;
            }
            public static  void Hash(int id, long hash)
            {
                LDB.techs.Select(id).HashNeeded = hash;
            }
        }
        public static  class CreateTech 
        {
            public static  void ID(TechProto tech, int id) 
            {
                tech.ID = id;
            }
            public static  void NameAndDesc(TechProto tech, string name, string desc)
            {
                tech.name = name;
                tech.Name = name;
                tech.description = desc;
                tech.Desc = desc;
            }
            public static  void Vector(TechProto tech, Vector2 vector)
            {
                tech.Position = vector;
            }
            public static  void UnlockRecipeArray(TechProto tech, RecipeProto[] recipe)
            {
                tech.unlockRecipeArray = recipe;
            }
            public static  void UnlockRecipe(TechProto tech, int[] recipe)
            {
                tech.UnlockRecipes = recipe;
            }
            public  static void PreTechArray(TechProto tech, TechProto[] tech1)
            {
                tech.preTechArray = tech1;
            }
            public static  void PostTechArray(TechProto tech, TechProto[] tech1)
            {
                tech.postTechArray = tech1;
            }
            public static  void PreTech(TechProto tech, int[] tech1)
            {
                tech.PreTechs = tech1;
            }
            public  static void ItemArray(TechProto tech, ItemProto[] item)
            {
                tech.itemArray = item;
            }
            public static  void ItemAndPoint(TechProto tech, int[] item, int[] point)
            {
                tech.Items = item;
                tech.ItemPoints = point;
            }
            public  static void Hash(TechProto tech, long hash)
            {
                tech.HashNeeded = hash;
            }







        }
    }
}

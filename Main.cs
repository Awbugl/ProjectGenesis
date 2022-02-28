using xiaoye97;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;
using CommonAPI;
using CommonAPI.Systems;
using ProjectGenesis.Patches;

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace ProjectGenesis
{
    [BepInPlugin("org.LoShin.GenesisBook", "Genesis", "1.0.0")]
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool")]
    [BepInDependency("dsp.common-api.CommonAPI")]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem), nameof(TabSystem),
                                  nameof(AssemblerRecipeSystem))]
    public class Main : BaseUnityPlugin
    {
        private int TableID, TableID2;

        public void Awake()
        {
            var pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var resources = new ResourceData("org.LoShin.GenesisBook", "texpack", pluginfolder);
            resources.LoadAssetBundle("texpack");
            ProtoRegistry.AddResource(resources);

            TableID = TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab",
                                            new TabData("3", "Assets/texpack/主机科技"));
            TableID2 = TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab2",
                                             new TabData("4", "Assets/texpack/化工科技"));

            LDBTool.PreAddDataAction += InitData;
            LDBTool.PostAddDataAction += PostAddDataAction;

            Harmony.CreateAndPatchAll(typeof(UIPatches));
            Harmony.CreateAndPatchAll(typeof(MultiProductionPatches));
            Harmony.CreateAndPatchAll(typeof(PlanetGasPatches));
            Harmony.CreateAndPatchAll(typeof(OceanDischargePatches));
        }

        public void InitData()
        {
            PreFix();

            var templateTech = LDB.techs.Select(1311);

            foreach (var techjson in JsonHelper.TechProtos())
            {
                var proto = LDB.techs.Exist(techjson.ID)
                    ? LDB.techs.Select(techjson.ID)
                    : templateTech.Copy();
                proto.ID = techjson.ID;
                proto.Name = techjson.Name;
                proto.Desc = techjson.Desc;
                proto.Conclusion = techjson.Conclusion;
                proto.Published = techjson.Published;
                proto.IconPath = techjson.IconPath;
                proto.IsLabTech = techjson.IsLabTech;
                proto.PreTechs = techjson.PreTechs;
                proto.Items = techjson.Items ?? new int[] { };
                proto.ItemPoints = techjson.ItemPoints ?? new int[] { };
                proto.HashNeeded = techjson.HashNeeded;
                proto.UnlockRecipes = techjson.UnlockRecipes;
                proto.AddItems = techjson.AddItems ?? new int[] { };
                proto.AddItemCounts = techjson.AddItemCounts ?? new int[] { };
                proto.Position = new Vector2(techjson.Position[0], techjson.Position[1]);

                if (!LDB.techs.Exist(techjson.ID)) LDBTool.PreAddProto(proto);
            }

            foreach (var itemjson in JsonHelper.ItemProtos())
            {
                if (itemjson.GridIndex > 4000)
                    itemjson.GridIndex = TableID2 * 1000 + (itemjson.GridIndex - 4000);
                else if (itemjson.GridIndex > 3000) 
                    itemjson.GridIndex = TableID * 1000 + (itemjson.GridIndex - 3000);

                if (!LDB.items.Exist(itemjson.ID))
                {
                    var proto = ProtoRegistry.RegisterItem(itemjson.ID, itemjson.Name, itemjson.Description,
                                                           itemjson.IconPath, itemjson.GridIndex, itemjson.StackSize);

                    proto.FuelType = itemjson.FuelType;
                    proto.HeatValue = itemjson.HeatValue;
                    proto.ReactorInc = itemjson.ReactorInc;
                    proto.DescFields = itemjson.DescFields ?? new int[] { };
                    proto.IsFluid = itemjson.IsFluid;
                    proto.Type = (EItemType)itemjson.Type;
                    proto.PreTechOverride = itemjson.PreTechOverride;
                    proto.Productive = itemjson.Productive;
                    proto.UnlockKey = itemjson.UnlockKey;
                    proto.ModelIndex = itemjson.ModelIndex;
                }
                else
                {
                    var proto = LDB.items.Select(itemjson.ID);
                    proto.ID = itemjson.ID;
                    proto.Name = itemjson.Name;
                    proto.Description = itemjson.Description;
                    proto.IconPath = itemjson.IconPath;
                    proto.GridIndex = itemjson.GridIndex;
                    proto.StackSize = itemjson.StackSize;
                    proto.FuelType = itemjson.FuelType;
                    proto.HeatValue = itemjson.HeatValue;
                    proto.ReactorInc = itemjson.ReactorInc;
                    proto.DescFields = itemjson.DescFields ?? new int[] { };
                    proto.IsFluid = itemjson.IsFluid;
                    proto.Type = (EItemType)itemjson.Type;
                    proto.SubID = itemjson.SubID;
                    proto.MiningFrom = itemjson.MiningFrom;
                    proto.ProduceFrom = itemjson.ProduceFrom;
                    proto.Grade = itemjson.Grade;
                    proto.Upgrades = itemjson.Upgrades ?? new int[] { };
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
                    proto.PreTechOverride = itemjson.PreTechOverride;
                    proto.Productive = itemjson.Productive;
                }
            }

            var type_9 = AssemblerRecipeSystem.RegisterRecipeType("电路蚀刻");
            var type_10 = AssemblerRecipeSystem.RegisterRecipeType("高精度加工");

            foreach (var recipeJson in JsonHelper.RecipeProtos())
            {
                if (!LDB.recipes.Exist(recipeJson.ID))
                {
                    RecipeProto proto;

                    if (recipeJson.Type == 9 || recipeJson.Type == 10)
                    {
                        proto = ProtoRegistry.RegisterRecipe(recipeJson.ID, recipeJson.Type == 9
                                                                 ? type_9
                                                                 : type_10, recipeJson.Time, recipeJson.Input,
                                                             recipeJson.InCounts, recipeJson.Output ?? new int[] { },
                                                             recipeJson.OutCounts ?? new int[] { },
                                                             recipeJson.Description, recipeJson.PreTech,
                                                             recipeJson.GridIndex, recipeJson.Name,
                                                             recipeJson.IconPath);
                    }
                    else
                    {
                        proto = ProtoRegistry.RegisterRecipe(recipeJson.ID, (global::ERecipeType)recipeJson.Type,
                                                             recipeJson.Time, recipeJson.Input, recipeJson.InCounts,
                                                             recipeJson.Output ?? new int[] { },
                                                             recipeJson.OutCounts ?? new int[] { },
                                                             recipeJson.Description, recipeJson.PreTech,
                                                             recipeJson.GridIndex, recipeJson.Name,
                                                             recipeJson.IconPath);
                    }

                    proto.Explicit = recipeJson.Explicit;
                    proto.Name = recipeJson.Name;
                    proto.Handcraft = recipeJson.Handcraft;
                    proto.NonProductive = recipeJson.NonProductive;
                }
                else
                {
                    var proto = LDB.recipes.Select(recipeJson.ID);
                    ProtoRegistry.EditRecipe(recipeJson.ID, (global::ERecipeType)recipeJson.Type, recipeJson.Time,
                                             recipeJson.Input, recipeJson.InCounts, recipeJson.Output ?? new int[] { },
                                             recipeJson.OutCounts ?? new int[] { }, recipeJson.Description,
                                             recipeJson.PreTech, recipeJson.GridIndex, recipeJson.IconPath);

                    proto.Explicit = recipeJson.Explicit;
                    proto.Name = recipeJson.Name;
                    proto.Handcraft = recipeJson.Handcraft;
                    proto.NonProductive = recipeJson.NonProductive;
                }
            }
        }

        public void PreFix()
        {
            LDB.strings.Select(2314).name = "剧毒液体海洋";
            LDB.strings.Select(2314).Name = "剧毒液体海洋";

            LDB.veins.Select(14).Name = "钨矿";
            LDB.veins.Select(14).name = "钨矿";

            LDB.milestones.Select(9).name = "钨";
            LDB.milestones.Select(9).Name = "钨";
            LDB.milestones.Select(9).defaultDesc = "你采集了钨矿,宇宙珍奇之一.它是一种用途广泛的新材料.";
            LDB.milestones.Select(9).DefaultDesc = "你采集了钨矿,宇宙珍奇之一.它是一种用途广泛的新材料.";

            LDB.items.Select(物品.单极磁石).ID = 6980;
            LDB.items.Select(物品.硫酸).ID = 6998;

            LDB.items.OnAfterDeserialize();
        }

        public void PostFix(ItemProtoSet itemProtos)
        {
            LDB.items.OnAfterDeserialize();

            itemProtos.Select(物品.二级制造台).prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.电路蚀刻;
            itemProtos.Select(物品.三级制造台).prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.高精度加工;

            itemProtos.Select(物品.一级制造台).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.二级制造台).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.三级制造台).prefabDesc.assemblerSpeed = 20000;

            itemProtos.Select(物品.位面熔炉).prefabDesc.assemblerSpeed = 40000;
            itemProtos.Select(物品.化工厂).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.原油精炼厂).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.粒子对撞机).prefabDesc.assemblerSpeed = 40000;

            itemProtos.Select(物品.采矿机).prefabDesc.minerPeriod = 600000;
            itemProtos.Select(物品.大型采矿机).prefabDesc.minerPeriod = 300000;
            itemProtos.Select(物品.原油采集站).prefabDesc.minerPeriod = 300000;
            itemProtos.Select(物品.水泵).prefabDesc.minerPeriod = 360000;

            itemProtos.Select(物品.研究站).prefabDesc.labAssembleSpeed = 2;
            itemProtos.Select(物品.研究站).prefabDesc.labResearchSpeed = 2;

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
            itemProtos.Select(物品.太阳能板).prefabDesc.genEnergyPerTick = 40000;
            itemProtos.Select(物品.地热发电机).prefabDesc.genEnergyPerTick = 400000;
            itemProtos.Select(物品.聚变发电机).prefabDesc.genEnergyPerTick = 2500000;
            itemProtos.Select(物品.风力涡轮机).prefabDesc.genEnergyPerTick = 50000;
            itemProtos.Select(物品.人造恒星).prefabDesc.genEnergyPerTick = 120000000;

            itemProtos.Select(物品.火力发电机).prefabDesc.useFuelPerTick = 200000;

            itemProtos.Select(物品.低速传送带).prefabDesc.beltSpeed = 3;
            itemProtos.Select(物品.高速传送带).prefabDesc.beltSpeed = 5;
            itemProtos.Select(物品.极速传送带).prefabDesc.beltSpeed = 10;

            itemProtos.Select(物品.低速分拣器).prefabDesc.inserterSTT = 100000;
            itemProtos.Select(物品.高速分拣器).prefabDesc.inserterSTT = 50000;
            itemProtos.Select(物品.极速分拣器).prefabDesc.inserterSTT = 25000;

            itemProtos.Select(物品.卫星配电站).prefabDesc.idleEnergyPerTick = 1200000;
            itemProtos.Select(物品.卫星配电站).prefabDesc.workEnergyPerTick = 40000000;

            itemProtos.Select(物品.电弧熔炉).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.无线输电塔).prefabDesc.powerConnectDistance = 90.5f;
        }

        private void PostAddDataAction()
        {
            foreach (var proto in LDB.techs.dataArray)
            {
                proto.Preload();
            }

            for (var i = 0; i < LDB.items.dataArray.Length; ++i)
            {
                LDB.items.dataArray[i].Preload(i);
            }

            for (var i = 0; i < LDB.recipes.dataArray.Length; ++i)
            {
                LDB.recipes.dataArray[i].Preload(i);
            }

            foreach (var proto in LDB.techs.dataArray)
            {
                proto.Preload2();
            }

            PostFix(LDB.items);

            ItemProto.InitFluids();
            ItemProto.InitItemIds();
            ItemProto.InitFuelNeeds();
            ItemProto.InitItemIndices();
            ItemProto.InitMechaMaterials();

            var dataArray = LDB.items.dataArray;

            foreach (var proto in dataArray)
            {
                StorageComponent.itemIsFuel[proto.ID] = proto.HeatValue > 0L;
                StorageComponent.itemStackCount[proto.ID] = proto.StackSize;
            }

            var logo = GameObject.Find("UI Root/Overlay Canvas/Main Menu/dsp-logo");
            var image = logo.GetComponent<RawImage>();
            var rectTransform = logo.GetComponent<RectTransform>();

            var iconstr = Localization.language == Language.zhCN
                ? "Assets/texpack/中文图标"
                : "Assets/texpack/英文图标";

            image.texture = Resources.Load<Sprite>(iconstr).texture;

            rectTransform.sizeDelta = new Vector2(800f, 500f);
            rectTransform.anchoredPosition
                = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + 50f);
        }
    }
}

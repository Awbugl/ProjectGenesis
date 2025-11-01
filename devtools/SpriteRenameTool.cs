using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ProjectGenesis
{
    internal static class SpriteRenameTool
    {
        private static readonly string SpriteDir = @"D:\Git\ProjectGenesis\assets\sprite";
        

        /* Name → ZHCN 映射 */
        private static Dictionary<string, string> _zhcnMap;

        /* 在 Run() 最开始调用 */
        private static void LoadStringsMap()
        {
            _zhcnMap = JsonFileUtils. LoadStrings()
               .ToDictionary(
                    t => t.Name,
                    t => t.ZHCN,
                    StringComparer.OrdinalIgnoreCase);
        }

        /* 控制台交互修正入口 */
        private static bool FixNameIfNeed<T>(T obj, string typeLetter, Func<T, string> getName, Action<T, string> setName, Action<T, string> setIcon)
        {
            var oldName = getName(obj);
            if (!_zhcnMap.TryGetValue(oldName, out var rightName) || string.Equals(oldName, rightName, StringComparison.Ordinal))
                return false; // 无需修正

            Console.Write($"[{typeLetter}] Name/ZHCN不一致 当前:{oldName}  建议:{rightName}  修正?(Y/N):");
            var key = Console.ReadKey().Key;
            Console.WriteLine();

            if (key != ConsoleKey.Y) return false;

            /* 1. 更新 JSON Name */
            setName(obj, rightName);

            /* 2. 生成新 IconPath 并更新 */
            var newIcon = $"Assets/texpack/{rightName}";
            setIcon(obj, newIcon);

            /* 3. 重命名 sprite 文件（如果存在） */
            var oldFileName = oldName; // 旧文件名（无扩展名）
            var srcPng = Path.Combine(SpriteDir, oldFileName + ".png");
            if (File.Exists(srcPng))
            {
                var dstPng = Path.Combine(SpriteDir, rightName + ".png");
                File.Move(srcPng, dstPng);
                Console.WriteLine($"[Sprite] {oldFileName}.png → {rightName}.png");
            }
            return true; // 已修改
        }
        
        public static void Run()
        {
            EnsureSpriteDir();
            LoadStringsMap();
            
            /* ---------- 1. Items ---------- */
            // 分别加载，保持来源
            var vanillaItems = JsonFileUtils.LoadItemsVanilla();
            var modItems     = JsonFileUtils.LoadItemsMod();

            // 处理并标记是否改动
            bool vChanged = ProcessList(vanillaItems, "I");
            bool mChanged = ProcessList(modItems,     "I");

            // 按来源回写
            if (vChanged) JsonFileUtils.SaveItemsVanilla(vanillaItems);
            if (mChanged) JsonFileUtils.SaveItemsMod(modItems);

            /* ---------- 2. Recipes ---------- */
            var recipes = JsonFileUtils.LoadRecipes();
            if (ProcessList(recipes, "R"))
                JsonFileUtils.SaveRecipes(recipes);

            /* ---------- 3. Techs ---------- */
            var techs = JsonFileUtils.LoadTechs();
            if (ProcessList(techs, "T"))
                JsonFileUtils.SaveTechs(techs);
        }

        /* 返回 true 表示有改动 */
        private static bool ProcessList<T>(List<T> list, string typeLetter)
        {
            bool any = false;
            foreach (var obj in list)
            {
                /* ===== 新增：先修正 Name / IconPath / 文件 ===== */
                any |= FixNameIfNeed(obj, typeLetter,
                    getName: x => (string)x.GetType().GetProperty("Name").GetValue(x),
                    setName: (x, v) => x.GetType().GetProperty("Name").SetValue(x, v),
                    setIcon: (x, v) => x.GetType().GetProperty("IconPath").SetValue(x, v));
                
                var icon = GetIconPath(obj);
                if (string.IsNullOrEmpty(icon) || !icon.StartsWith("Assets/texpack/")) continue;

                var oldName = icon.Substring("Assets/texpack/".Length);
                var srcPng  = Path.Combine(SpriteDir, oldName + ".png");
                if (!File.Exists(srcPng)) continue;

                var newName = typeLetter + GetName(obj);
                var dstPng  = Path.Combine(SpriteDir, newName + ".png");

                File.Move(srcPng, dstPng);
                Console.WriteLine($"[Sprite] {oldName}.png → {newName}.png");

                SetIconPath(obj, "Assets/texpack/" + newName);
                any = true;
            }
            return any;
        }

        /* 反射辅助 */
        private static string GetIconPath<T>(T obj) =>
            (string)obj.GetType().GetProperty("IconPath")?.GetValue(obj);
        private static void SetIconPath<T>(T obj, string newPath) =>
            obj.GetType().GetProperty("IconPath")?.SetValue(obj, newPath);
        private static string GetName<T>(T obj) =>
            (string)obj.GetType().GetProperty("Name")?.GetValue(obj);

        private static void EnsureSpriteDir()
        {
            if (!Directory.Exists(SpriteDir))
                throw new DirectoryNotFoundException(SpriteDir);
        }
    }
}
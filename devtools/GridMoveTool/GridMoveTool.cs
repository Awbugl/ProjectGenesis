using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectGenesis
{
    internal static class GridMoveTool
    {
        public static void MoveRecipeGrid(Pos pos1, Pos pos2, Pos newPos) =>
            MoveGrid(JsonFileUtils.LoadRecipes(), JsonFileUtils.SaveRecipes, pos1, pos2, newPos, r => r.GridIndex,
                (r, v) => r.GridIndex = v);

        public static void MoveItemGrid(Pos pos1, Pos pos2, Pos newPos)
        {
            MoveGrid(JsonFileUtils.LoadItemsVanilla(), JsonFileUtils.SaveItemsVanilla, pos1, pos2, newPos, i => i.GridIndex,
                (i, v) => i.GridIndex = v);

            MoveGrid(JsonFileUtils.LoadItemsMod(), JsonFileUtils.SaveItemsMod, pos1, pos2, newPos, i => i.GridIndex,
                (i, v) => i.GridIndex = v);
        }

        private static void MoveGrid<T>(List<T> data, Action<List<T>> save, Pos pos1, Pos pos2, Pos newPos, Func<T, int> getGrid,
            Action<T, int> setGrid)
        {
            if (!GridUtil.ValidateRect(pos1, pos2)) return;

            save(GridUtil.MoveInRect(data, pos1, pos2, newPos, getGrid, setGrid));
        }

        internal static void CheckRecipeGrid() =>
            GridUtil.CheckGrids(JsonFileUtils.LoadRecipes(), r => r.GridIndex, r => r.ID, 5, 7, 17, "CheckRecipeGrid");

        internal static void CheckItemGrid()
        {
            var items = JsonFileUtils.LoadItemsVanilla();
            items.AddRange(JsonFileUtils.LoadItemsMod());
            GridUtil.CheckGrids(items, i => i.GridIndex, i => i.ID, 5, 7, 17, "CheckItemGrid");
        }

        internal static void CheckTechUnlockRecipe()
        {
            var teches = JsonFileUtils.LoadTechs();
            var recipeDict = JsonFileUtils.LoadRecipes().ToDictionary(r => r.ID);
            foreach (var tech in teches)
                foreach (int id in tech.UnlockRecipes.Where(id => !recipeDict.ContainsKey(id)))
                    Console.WriteLine($"CheckTechUnlockRecipe invalid recipe:{id}  tech:{tech.ID}");
        }
    }
}

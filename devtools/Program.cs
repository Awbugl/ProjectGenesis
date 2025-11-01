using static ProjectGenesis.GridMoveTool;
using static ProjectGenesis.DropListTool;
using static ProjectGenesis.SpriteRenameTool;

namespace ProjectGenesis
{
    internal static class DevTools
    {
        internal static void Main(string[] args)
        {
            // MoveRecipeGrid(5207, 5215, 5208);
            // MoveItemGrid(5207, 5215, 5208);
            // CheckTechUnlockRecipe();
            // CheckItemGrid();
            // CheckRecipeGrid(); 
            SpriteRenameTool.Run();
        }
    }
}

using System.Linq;

namespace ProjectGenesis
{
    internal static class DropListTool
    {
        public static void SetItemModDropList()
        {
            var items = JsonFileUtils.LoadItemsMod()
               .ToDictionary(i => i.ID);

            float cursor = 1f;
            for (int i = 0; i < DropConfig.ID.Count; i++)
            {
                var it = items[DropConfig.ID[i]];
                it.EnemyDropMask  = int.MaxValue;
                it.EnemyDropCount = DropConfig.Count[i];
                it.EnemyDropLevel = DropConfig.Level[i];

                cursor -= DropConfig.Prob[i];
                it.EnemyDropRange = new[]
                {
                    (int)(cursor * 100000) / 100000f,
                    DropConfig.Prob[i],
                };
            }

            JsonFileUtils.SaveItemsMod(items.Values.OrderBy(x => x.ID).ToList());
        }
    }
}

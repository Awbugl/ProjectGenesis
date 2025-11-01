using System;
using System.Collections.Generic;

namespace ProjectGenesis
{
    internal static class GridUtil
    {
        internal static bool ValidateRect(in Pos p1, in Pos p2)
        {
            if (p1.Page != p2.Page)
            {
                Console.WriteLine("page1 != page2");
                return false;
            }

            if (p1.Row > p2.Row)
            {
                Console.WriteLine("row1 > row2");
                return false;
            }

            if (p1.Column > p2.Column)
            {
                Console.WriteLine("column1 > column2");
                return false;
            }

            return true;
        }

        internal static List<T> MoveInRect<T>(List<T> data, in Pos topLeft, in Pos bottomRight, in Pos newTopLeft, // ← 直接给目标左上角
            Func<T, int> getGrid, Action<T, int> setGrid)
        {
            foreach (var item in data)
            {
                var pos = new Pos(getGrid(item));
                if (pos.Page == topLeft.Page && pos.Row >= topLeft.Row && pos.Row <= bottomRight.Row && pos.Column >= topLeft.Column
                 && pos.Column <= bottomRight.Column)
                {
                    // 相对坐标平移
                    setGrid(item,
                        new Pos(pos.Page + (newTopLeft.Page - topLeft.Page), pos.Row + (newTopLeft.Row - topLeft.Row),
                            pos.Column + (newTopLeft.Column - topLeft.Column)).ToGridIndex());
                }
            }

            return data;
        }

        internal static void CheckGrids<T>(IEnumerable<T> data, Func<T, int> getGrid, Func<T, int> getId, int maxPage, int maxRow,
            int maxCol, string tag)
        {
            var seen = new HashSet<int>();
            foreach (var x in data)
            {
                var grid = getGrid(x);
                if (grid == 0) continue;

                var p = new Pos(grid);
                if (p.Page < 1 || p.Page > maxPage) Console.WriteLine($"{tag} invalid Page: {grid}  id:{getId(x)}");
                if (p.Row < 1 || p.Row > maxRow) Console.WriteLine($"{tag} invalid Row:  {grid}  id:{getId(x)}");
                if (p.Column < 1 || p.Column > maxCol) Console.WriteLine($"{tag} invalid Col:  {grid}  id:{getId(x)}");
                if (!seen.Add(grid)) Console.WriteLine($"{tag} duplicate:    {grid}  id:{getId(x)}");
            }
        }
    }
}

namespace ProjectGenesis
{
    internal readonly struct Pos
    {
        public readonly int Page, Row, Column;

        public Pos(int gridIndex)
        {
            Page = gridIndex / 1000;
            Row = gridIndex / 100 % 10;
            Column = gridIndex % 100;
        }

        public Pos(int page, int row, int column)
        {
            Page = page;
            Row = row;
            Column = column;
        }

        public int ToGridIndex() => Page * 1000 + Row * 100 + Column;

        // 关键：隐式转换
        public static implicit operator Pos(int gridIndex) => new Pos(gridIndex);
    }
}

using System;

namespace ProjectGenesis.Utils
{
    [Flags]
    public enum EOverflowFlag
    {
        None = 0,
        FirstProduct = 1,
        SecondProduct = 2,
        ThirdProduct = 4,
        FourthProduct = 8,
        FifthProduct = 16,
        All = 32,
    }
}

using System.Collections.Generic;

namespace ProjectGenesis
{
    internal static class DropConfig
    {
        public static IReadOnlyList<int> ID { get; } = new List<int>
        {
            7805, 7803,       //
            7709,             //
            6263, 7804, 6235, //
            7705,             //
            6222,             //
            6207, 6208, 6204  //
        };

        public static IReadOnlyList<float> Prob { get; } = new List<float>
        {
            0.001f, 0.003f,         //
            0.002f,                 //
            0.003f, 0.004f, 0.003f, //
            0.004f,                 //
            0.005f,                 //
            0.006f, 0.005f, 0.005f, //
        };

        public static IReadOnlyList<int> Level { get; } = new List<int>
        {
            8, 8,    //
            7,       //
            6, 6, 6, //
            5,       //
            4,       //
            3, 2, 1, //
        };

        public static IReadOnlyList<float> Count { get; } = new List<float>
        {
            0.1f, 0.1f,       //
            0.7f,             //
            0.7f, 0.7f, 0.7f, //
            0.25f, 0.25f,     //
            0.8f,             //
            0.8f,             //
            1.4f, 1.4f, 1.4f, //
        };
    }
}

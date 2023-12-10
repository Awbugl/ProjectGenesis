namespace ProjectGenesis.Utils
{
    public enum ERecipeType
    {
        None = 0,
        Smelt = 1,
        Chemical = 2,
        Refine = 3,
        Assemble = 4,
        Particle = 5,
        Exchange = 6,
        PhotonStore = 7,
        Fractionate = 8,
        标准制造 = 9,
        高精度加工 = 10,
        矿物处理 = 11,
        精密组装 = 12,
        聚变生产 = 13,
        垃圾回收 = 14,
        Research = 15,
        高分子化工 = 16,
        所有化工 = 17, // 2 + 3 + 16 
        所有制造 = 18, // 4 + 9
        所有熔炉 = 19  // 1 + 11
    }
}

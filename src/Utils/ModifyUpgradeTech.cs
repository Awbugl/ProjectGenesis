using System;
using System.Linq;

namespace ProjectGenesis.Utils
{
    internal static class ModifyUpgradeTech
    {
        private static readonly int[] Items5 =
                                      {
                                          6001, 6002, 6003, 6004,
                                          6005,
                                      },
                                      Items4 =
                                      {
                                          6001, 6002, 6003, 6004,
                                      },
                                      Items3 = { 6001, 6002, 6003, },
                                      Items2 = { 6001, 6002, };


        internal static void ModifyUpgradeTeches()
        {
            TechProto tech = LDB.techs.Select(ProtoID.T批量建造1);
            tech.HashNeeded = 1200;
            tech.UnlockValues = new[] { 450.0, };

            tech = LDB.techs.Select(ProtoID.T批量建造2);
            tech.UnlockValues = new[] { 900.0, };

            tech = LDB.techs.Select(ProtoID.T批量建造3);
            tech.UnlockValues = new[] { 1800.0, };

            tech = LDB.techs.Select(ProtoID.T能量回路4);
            tech.Items = Items4;
            tech.ItemPoints = Enumerable.Repeat(12, 4).ToArray();

            tech = LDB.techs.Select(ProtoID.T驱动引擎4);
            tech.Items = Items4;
            tech.ItemPoints = Enumerable.Repeat(10, 4).ToArray();

            tech = LDB.techs.Select(ProtoID.T驱动引擎5);
            tech.Items = Items5;
            tech.ItemPoints = Enumerable.Repeat(10, 5).ToArray();

            tech = LDB.techs.Select(ProtoID.T垂直建造3);
            tech.Items = Items3;
            tech.ItemPoints = new[] { 20, 20, 10, };

            tech = LDB.techs.Select(ProtoID.T垂直建造6);
            tech.Items = Items5;
            tech.ItemPoints = Enumerable.Repeat(6, 5).ToArray();

            tech = LDB.techs.Select(ProtoID.T集装分拣6);
            tech.Items = Items5;
            tech.ItemPoints = Enumerable.Repeat(6, 5).ToArray();

            for (int i = ProtoID.T宇宙探索1; i <= ProtoID.T宇宙探索4; i++)
            {
                TechProto techProto = LDB.techs.Select(i);
                techProto.Items = new[] { 6001, };
                techProto.ItemPoints = new[] { techProto.ItemPoints[0], };
                techProto.PreTechsImplicit = Array.Empty<int>();
            }

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (TechProto techProto in LDB.techs.dataArray)
            {
                if (techProto.ID < 2000) continue;

                int[] items = techProto.Items;

                if (items.SequenceEqual(Items5))
                {
                    techProto.Items = new[] { 6280, };
                    techProto.ItemPoints = new[] { techProto.ItemPoints[4], };
                    continue;
                }

                if (items.SequenceEqual(Items4))
                {
                    if (techProto.ID % 100 > 2)
                    {
                        techProto.Items = new[] { 6278, 6279, };
                        techProto.ItemPoints = new[] { techProto.ItemPoints[1], techProto.ItemPoints[3], };
                    }
                    else
                    {
                        techProto.Items = new[] { 6278, 6003, 6004, };
                        techProto.ItemPoints = new[] { techProto.ItemPoints[1], techProto.ItemPoints[2], techProto.ItemPoints[3], };
                    }

                    continue;
                }

                if (items.SequenceEqual(Items3))
                {
                    techProto.Items = new[] { 6278, 6003, };
                    techProto.ItemPoints = new[] { techProto.ItemPoints[1], techProto.ItemPoints[2], };
                    continue;
                }

                // ReSharper disable once InvertIf
                if (items.SequenceEqual(Items2) && techProto.ID % 100 > 2)
                {
                    techProto.Items = new[] { 6278, };
                    techProto.ItemPoints = new[] { techProto.ItemPoints[0], };
                    continue;
                }
            }
        }
    }
}

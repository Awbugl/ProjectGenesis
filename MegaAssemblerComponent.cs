using System;
using System.IO;
using CommonAPI;
using CommonAPI.Systems;
using ERecipeType_1 = ERecipeType;

namespace ProjectGenesis
{
    public class MegaAssemblerComponent : FactoryComponent
    {
        public static readonly string componentID = "org.LoShin.GenesisBook:org.LoShin.MegaAssemblerComponent";


        private static int _cachedId;

        public static int cachedId
        {
            get
            {
                if (_cachedId == 0) _cachedId = ComponentExtension.componentRegistry.GetUniqueId(componentID);

                return _cachedId;
            }
        }


        // This is where you get your updates. power is a value from 0 to 1 of how much power machine recieved.
        // The value you return here is then passed to `UpdateAnimation`
        public override int InternalUpdate(float power, PlanetFactory factory)
        {
            if (power < 0.1f) return 0;

            FactoryProductionStat factoryProductionStat = GameMain.statistics.production.factoryStatPool[factory.index];
            int[] productRegister = factoryProductionStat.productRegister;
            int[] consumeRegister = factoryProductionStat.consumeRegister;
            
            CargoTraffic cargoTraffic = factory.cargoTraffic;
            SignData[] entitySignPool = factory.entitySignPool;
            
            int stationPilerLevel = GameMain.history.stationPilerLevel;
            
            UpdateInputSlots(cargoTraffic, entitySignPool);
            UpdateOutputSlots(cargoTraffic, entitySignPool, stationPilerLevel);
            
            return (int)InternalUpdate(power, productRegister, consumeRegister);
        }

        // Here you can update machine animations. You get the update result from update method, and power state.
        // This code just loops simple animation.
        public override void UpdateAnimation(ref AnimData data, int updateResult, float power) { }

        // Determine if machine should consume full power, other wise idle.
        public override void UpdatePowerState(ref PowerConsumerComponent component)
        {
            component.SetRequiredEnergy(true);
        }
       
        public bool replicating;
        public int speed;
        public int speedOverride;
        public int time;
        public int extraTime;
        public int extraSpeed;
        public int extraPowerRatio;
        public bool productive;
        public bool forceAccMode;
        public int recipeId;
        public ERecipeType_1 recipeType;
        public int timeSpend;
        public int extraTimeSpend;
        public int[] requires;
        public int[] requireCounts;
        public int[] served;
        public int[] incServed;
        public int[] needs;
        public int[] products;
        public int[] productCounts;
        public int[] produced;
        public SlotData[] slots = new SlotData[12];
        private int outSlotOffset;

        public void UpdateOutputSlots(CargoTraffic traffic, SignData[] signPool, int maxPilerCount)
        {
            int maxStack = maxPilerCount;
            int length1 = slots.Length;
            int length2 = products.Length;
            int num1 = -1;

            for (int index3 = 0; index3 < maxStack; ++index3)
            {
                for (int index4 = 0; index4 < length1; ++index4)
                {
                    int index5 = (this.outSlotOffset + index4) % length1;
                    if (slots[index5].dir == IODir.Output)
                    {
                        if (slots[index5].counter > 0)
                        {
                            --slots[index5].counter;
                        }
                        else
                        {
                            int entityId = traffic.beltPool[slots[index5].beltId].entityId;
                            CargoPath cargoPath
                                = traffic.GetCargoPath(traffic.beltPool[slots[index5].beltId].segPathId);
                            if (cargoPath != null)
                            {
                                int index6 = slots[index5].storageIdx - 1;
                                if (index6 >= 0 && index6 < length2)
                                {
                                    int itemId = products[index6];
                                    if (itemId > 0)
                                    {
                                        if (cargoPath.TryUpdateItemAtHeadAndFillBlank(itemId, maxStack, (byte)1,
                                                                                      (byte)0))
                                        {
                                            --produced[index6];
                                            num1 = (index5 + 1) % length1;
                                        }
                                    }
                                }

                                signPool[entityId].iconType = 0U;
                                signPool[entityId].iconId0 = 0U;
                            }
                        }
                    }
                    else if (slots[index5].dir != IODir.Input)
                    {
                        slots[index5].beltId = 0;
                        slots[index5].counter = 0;
                    }

                    this.outSlotOffset = num1;
                }
            }

            if (num1 < 0) return;
            outSlotOffset = num1;
        }

        public void UpdateInputSlots(CargoTraffic traffic, SignData[] signPool)
        {
            int length1 = slots.Length;
            for (int index = 0; index < length1; ++index)
            {
                if (slots[index].dir == IODir.Input)
                {
                    if (slots[index].counter > 0)
                    {
                        --slots[index].counter;
                    }
                    else
                    {
                        int entityId = traffic.beltPool[slots[index].beltId].entityId;
                        CargoPath cargoPath = traffic.GetCargoPath(traffic.beltPool[slots[index].beltId].segPathId);
                        if (cargoPath != null)
                        {
                            int needIdx = -1;
                            byte stack;
                            byte inc;
                            int itemId = cargoPath.TryPickItemAtRear(needs, out needIdx, out stack, out inc);
                            if (needIdx >= 0)
                            {
                                int stack1 = (int)stack;
                                int inc1 = (int)inc;
                                if (itemId > 0)
                                {
                                    if (needIdx < needs.Length && needs[needIdx] == itemId)
                                    {
                                        served[needIdx] += stack1;
                                        incServed[needIdx] += inc1;
                                    }
                                }

                                slots[index].storageIdx = needIdx + 1;
                            }

                            if (itemId > 0)
                            {
                                signPool[entityId].iconType = 1U;
                                signPool[entityId].iconId0 = (uint)itemId;
                            }
                        }
                    }
                }
                else if (slots[index].dir != IODir.Output)
                {
                    slots[index].beltId = 0;
                    slots[index].counter = 0;
                }
            }
        }

        public override void Export(BinaryWriter w)
        {
            w.Write(3);
            w.Write(id);
            w.Write(entityId);
            w.Write(pcId);
            w.Write(replicating);
            w.Write(false);
            w.Write(speed);
            w.Write(time);
            w.Write(speedOverride);
            w.Write(extraTime);
            w.Write(extraSpeed);
            w.Write(extraPowerRatio);
            w.Write(productive);
            w.Write(forceAccMode);
            w.Write((short)recipeId);
            if (recipeId <= 0) return;
            w.Write((byte)recipeType);
            w.Write(timeSpend);
            w.Write(extraTimeSpend);
            w.Write((byte)requires.Length);
            for (int index = 0; index < requires.Length; ++index) w.Write((short)requires[index]);
            w.Write((byte)requireCounts.Length);
            for (int index = 0; index < requireCounts.Length; ++index) w.Write((short)requireCounts[index]);
            w.Write((byte)served.Length);
            for (int index = 0; index < served.Length; ++index) w.Write(served[index]);
            w.Write((byte)incServed.Length);
            for (int index = 0; index < incServed.Length; ++index) w.Write(incServed[index]);
            w.Write((byte)needs.Length);
            for (int index = 0; index < needs.Length; ++index) w.Write((short)needs[index]);
            w.Write((byte)products.Length);
            for (int index = 0; index < products.Length; ++index) w.Write((short)products[index]);
            w.Write((byte)productCounts.Length);
            for (int index = 0; index < productCounts.Length; ++index) w.Write((short)productCounts[index]);
            w.Write((byte)produced.Length);
            for (int index = 0; index < produced.Length; ++index) w.Write(produced[index]);
        }

        public override void Import(BinaryReader r)
        {
            int num = r.ReadInt32();
            bool flag = num >= 1;
            id = r.ReadInt32();
            entityId = r.ReadInt32();
            pcId = r.ReadInt32();
            replicating = r.ReadBoolean();
            r.ReadBoolean();
            speed = r.ReadInt32();
            time = r.ReadInt32();
            if (num >= 2)
            {
                speedOverride = r.ReadInt32();
                extraTime = r.ReadInt32();
                extraSpeed = r.ReadInt32();
                extraPowerRatio = r.ReadInt32();
                productive = r.ReadBoolean();
                forceAccMode = num >= 3 && r.ReadBoolean();
            }
            else
            {
                speedOverride = speed;
                extraTime = 0;
                extraSpeed = 0;
                extraPowerRatio = 0;
                productive = false;
                forceAccMode = false;
            }

            recipeId = !flag
                ? r.ReadInt32()
                : (int)r.ReadInt16();
            if (recipeId > 0)
            {
                RecipeProto recipeProto = LDB.recipes.Select(recipeId);
                if (flag)
                {
                    recipeType = (ERecipeType_1)r.ReadByte();
                    timeSpend = r.ReadInt32();
                    extraTimeSpend = num < 2
                        ? timeSpend * 10
                        : r.ReadInt32();
                    int length1 = (int)r.ReadByte();
                    requires = new int[length1];
                    for (int index = 0; index < length1; ++index) requires[index] = (int)r.ReadInt16();
                    int length2 = (int)r.ReadByte();
                    requireCounts = new int[length2];
                    for (int index = 0; index < length2; ++index) requireCounts[index] = (int)r.ReadInt16();
                    int length3 = (int)r.ReadByte();
                    served = new int[length3];
                    for (int index = 0; index < length3; ++index) served[index] = r.ReadInt32();
                    if (num >= 2)
                    {
                        int length4 = (int)r.ReadByte();
                        incServed = new int[length4];
                        for (int index = 0; index < length4; ++index) incServed[index] = r.ReadInt32();
                    }
                    else
                    {
                        incServed = new int[served.Length];
                        for (int index = 0; index < served.Length; ++index) incServed[index] = 0;
                    }

                    int length5 = (int)r.ReadByte();
                    needs = new int[length5];
                    for (int index = 0; index < length5; ++index) needs[index] = (int)r.ReadInt16();
                    int length6 = (int)r.ReadByte();
                    products = new int[length6];
                    for (int index = 0; index < length6; ++index) products[index] = (int)r.ReadInt16();
                    int length7 = (int)r.ReadByte();
                    productCounts = new int[length7];
                    for (int index = 0; index < length7; ++index) productCounts[index] = (int)r.ReadInt16();
                    int length8 = (int)r.ReadByte();
                    produced = new int[length8];
                    for (int index = 0; index < length8; ++index) produced[index] = r.ReadInt32();
                    if (recipeProto != null)
                    {
                        if (recipeProto.Items.Length == requires.Length)
                            Array.Copy((Array)recipeProto.Items, (Array)requires, requires.Length);
                        if (recipeProto.ItemCounts.Length == requireCounts.Length)
                            Array.Copy((Array)recipeProto.ItemCounts, (Array)requireCounts, requireCounts.Length);
                        if (recipeProto.Results.Length == products.Length)
                            Array.Copy((Array)recipeProto.Results, (Array)products, products.Length);
                        if (recipeProto.ResultCounts.Length == productCounts.Length)
                            Array.Copy((Array)recipeProto.ResultCounts, (Array)productCounts, productCounts.Length);
                        timeSpend = recipeProto.TimeSpend * 10000;
                    }
                }
                else
                {
                    recipeType = (ERecipeType_1)r.ReadInt32();
                    timeSpend = r.ReadInt32();
                    extraTimeSpend = num < 2
                        ? timeSpend * 10
                        : r.ReadInt32();
                    int length9 = r.ReadInt32();
                    requires = new int[length9];
                    for (int index = 0; index < length9; ++index) requires[index] = r.ReadInt32();
                    int length10 = r.ReadInt32();
                    requireCounts = new int[length10];
                    for (int index = 0; index < length10; ++index) requireCounts[index] = r.ReadInt32();
                    int length11 = r.ReadInt32();
                    served = new int[length11];
                    for (int index = 0; index < length11; ++index) served[index] = r.ReadInt32();
                    if (num >= 2)
                    {
                        int length12 = (int)r.ReadByte();
                        incServed = new int[length12];
                        for (int index = 0; index < length12; ++index) incServed[index] = r.ReadInt32();
                    }
                    else
                    {
                        incServed = new int[served.Length];
                        for (int index = 0; index < served.Length; ++index) incServed[index] = 0;
                    }

                    int length13 = r.ReadInt32();
                    needs = new int[length13];
                    for (int index = 0; index < length13; ++index) needs[index] = r.ReadInt32();
                    int length14 = r.ReadInt32();
                    products = new int[length14];
                    for (int index = 0; index < length14; ++index) products[index] = r.ReadInt32();
                    int length15 = r.ReadInt32();
                    productCounts = new int[length15];
                    for (int index = 0; index < length15; ++index) productCounts[index] = r.ReadInt32();
                    int length16 = r.ReadInt32();
                    produced = new int[length16];
                    for (int index = 0; index < length16; ++index) produced[index] = r.ReadInt32();
                }
            }

            if (num > 2) return;
            CheckOldVersionProductive();
        }

        public override void Free()
        {
            id = 0;
            entityId = 0;
            pcId = 0;
            replicating = false;
            speed = 0;
            time = 0;
            speedOverride = 0;
            extraTime = 0;
            extraSpeed = 0;
            productive = false;
            forceAccMode = false;
            recipeId = 0;
            recipeType = ERecipeType_1.None;
            timeSpend = 0;
            extraTimeSpend = 0;
            requires = (int[])null;
            requireCounts = (int[])null;
            served = (int[])null;
            incServed = (int[])null;
            needs = (int[])null;
            products = (int[])null;
            productCounts = (int[])null;
            produced = (int[])null;
            slots = (SlotData[])null;
        }

        public void SetRecipe(int recpId, SignData[] signPool)
        {
            replicating = false;
            time = 0;
            extraTime = 0;
            extraSpeed = 0;
            productive = false;
            forceAccMode = false;
            RecipeProto recipeProto = (RecipeProto)null;
            if (recpId > 0) recipeProto = LDB.recipes.Select(recpId);
            if (recipeProto != null)
            {
                recipeId = recipeProto.ID;
                recipeType = recipeProto.Type;
                timeSpend = recipeProto.TimeSpend * 10000;
                extraTimeSpend = recipeProto.TimeSpend * 100000;
                speedOverride = speed;
                productive = recipeProto.productive;
                requires = new int[recipeProto.Items.Length];
                Array.Copy((Array)recipeProto.Items, (Array)requires, requires.Length);
                requireCounts = new int[recipeProto.ItemCounts.Length];
                Array.Copy((Array)recipeProto.ItemCounts, (Array)requireCounts, requireCounts.Length);
                served = new int[requireCounts.Length];
                incServed = new int[requireCounts.Length];
                Assert.True(requires.Length == requireCounts.Length);
                needs = new int[6];
                products = new int[recipeProto.Results.Length];
                Array.Copy((Array)recipeProto.Results, (Array)products, products.Length);
                productCounts = new int[recipeProto.ResultCounts.Length];
                Array.Copy((Array)recipeProto.ResultCounts, (Array)productCounts, productCounts.Length);
                produced = new int[productCounts.Length];
                Assert.True(products.Length == productCounts.Length);
                signPool[entityId].iconId0 = (uint)recipeId;
                signPool[entityId].iconType = 2U;
            }
            else
            {
                recipeId = 0;
                recipeType = ERecipeType_1.None;
                timeSpend = 0;
                extraTimeSpend = 0;
                speedOverride = speed;
                requires = (int[])null;
                requireCounts = (int[])null;
                served = (int[])null;
                incServed = (int[])null;
                needs = (int[])null;
                products = (int[])null;
                productCounts = (int[])null;
                produced = (int[])null;
                signPool[entityId].iconId0 = 0U;
                signPool[entityId].iconType = 0U;
            }
        }

        public void SetPCState(PowerConsumerComponent[] pcPool) =>
            pcPool[pcId].SetRequiredEnergy(replicating, 1000 + extraPowerRatio);

        public override int[] UpdateNeeds()
        {
            int length = requires.Length;
            needs[0] = 0 >= length || served[0] >= requireCounts[0] * 10
                ? 0
                : requires[0];
            needs[1] = 1 >= length || served[1] >= requireCounts[1] * 10
                ? 0
                : requires[1];
            needs[2] = 2 >= length || served[2] >= requireCounts[2] * 10
                ? 0
                : requires[2];
            needs[3] = 3 >= length || served[3] >= requireCounts[3] * 10
                ? 0
                : requires[3];
            needs[4] = 4 >= length || served[4] >= requireCounts[4] * 10
                ? 0
                : requires[4];
            needs[5] = 5 >= length || served[5] >= requireCounts[5] * 10
                ? 0
                : requires[5];

            return needs;
        }

        public uint InternalUpdate(float power, int[] productRegister, int[] consumeRegister)
        {
            if ((double)power < 0.100000001490116) return 0;
            if (extraTime >= extraTimeSpend)
            {
                int length = products.Length;
                if (length == 1)
                {
                    produced[0] += productCounts[0];
                    lock (productRegister) productRegister[products[0]] += productCounts[0];
                }
                else
                {
                    for (int index = 0; index < length; ++index)
                    {
                        produced[index] += productCounts[index];
                        lock (productRegister) productRegister[products[index]] += productCounts[index];
                    }
                }

                extraTime -= extraTimeSpend;
            }

            if (time >= timeSpend)
            {
                replicating = false;
                int length = products.Length;
                if (length == 1)
                {
                    if (recipeType == ERecipeType_1.Smelt)
                    {
                        if (produced[0] + productCounts[0] > 100) return 0;
                    }
                    else if (recipeType == ERecipeType_1.Assemble)
                    {
                        if (produced[0] > productCounts[0] * 9) return 0;
                    }
                    else if (produced[0] > productCounts[0] * 19) return 0;

                    produced[0] += productCounts[0];
                    lock (productRegister) productRegister[products[0]] += productCounts[0];
                }
                else
                {
                    if (recipeType == ERecipeType_1.Refine)
                    {
                        for (int index = 0; index < length; ++index)
                        {
                            if (produced[index] > productCounts[index] * 19) return 0;
                        }
                    }
                    else if (recipeType == ERecipeType_1.Particle)
                    {
                        for (int index = 0; index < length; ++index)
                        {
                            if (produced[index] > productCounts[index] * 19) return 0;
                        }
                    }
                    else if (recipeType == ERecipeType_1.Chemical)
                    {
                        for (int index = 0; index < length; ++index)
                        {
                            if (produced[index] > productCounts[index] * 19) return 0;
                        }
                    }
                    else if (recipeType == ERecipeType_1.Smelt)
                    {
                        for (int index = 0; index < length; ++index)
                        {
                            if (produced[index] + productCounts[index] > 100) return 0;
                        }
                    }
                    else if (recipeType == ERecipeType_1.Assemble)
                    {
                        for (int index = 0; index < length; ++index)
                        {
                            if (produced[index] > productCounts[index] * 9) return 0;
                        }
                    }
                    else
                    {
                        for (int index = 0; index < length; ++index)
                        {
                            if (produced[index] > productCounts[index] * 19) return 0;
                        }
                    }

                    for (int index = 0; index < length; ++index)
                    {
                        produced[index] += productCounts[index];
                        lock (productRegister) productRegister[products[index]] += productCounts[index];
                    }
                }

                extraSpeed = 0;
                speedOverride = speed;
                extraPowerRatio = 0;
                time -= timeSpend;
            }

            if (!replicating)
            {
                int length = requireCounts.Length;
                for (int index = 0; index < length; ++index)
                {
                    if (incServed[index] <= 0) incServed[index] = 0;
                    if (served[index] < requireCounts[index] || served[index] == 0)
                    {
                        time = 0;
                        return 0;
                    }
                }

                int index1 = length > 0
                    ? 10
                    : 0;
                for (int index2 = 0; index2 < length; ++index2)
                {
                    int num = split_inc_level(ref served[index2], ref incServed[index2], requireCounts[index2]);
                    index1 = index1 < num
                        ? index1
                        : num;
                    if (served[index2] == 0) incServed[index2] = 0;
                    lock (consumeRegister) consumeRegister[requires[index2]] += requireCounts[index2];
                }

                if (index1 < 0) index1 = 0;
                if (productive && !forceAccMode)
                {
                    extraSpeed = (int)((double)speed * Cargo.incTableMilli[index1] * 10.0 + 0.1);
                    speedOverride = speed;
                    extraPowerRatio = Cargo.powerTable[index1];
                }
                else
                {
                    extraSpeed = 0;
                    speedOverride = (int)((double)speed * (1.0 + Cargo.accTableMilli[index1]) + 0.1);
                    extraPowerRatio = Cargo.powerTable[index1];
                }

                replicating = true;
            }

            if (replicating && time < timeSpend && extraTime < extraTimeSpend)
            {
                time += (int)((double)power * (double)speedOverride);
                extraTime += (int)((double)power * (double)extraSpeed);
            }

            return !replicating
                ? 0U
                : 1U;
        }

        public void CheckOldVersionProductive()
        {
            RecipeProto recipeProto = LDB.recipes.Select(recipeId);
            if (recipeProto != null)
                productive = recipeProto.productive;
            else
                productive = false;
        }

        private int split_inc_level(ref int n, ref int m, int p)
        {
            int num1 = m / n;
            int num2 = m - num1 * n;
            n -= p;
            int num3 = num2 - n;
            m -= num3 > 0
                ? num1 * p + num3
                : num1 * p;
            return num1;
        }
    }
}

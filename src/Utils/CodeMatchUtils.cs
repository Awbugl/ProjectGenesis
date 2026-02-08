using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Utils
{
    public static class CodeMatchUtils
    {
        /// <summary>条件为真时跳转 (brtrue/brtrue.s)</summary>
        public static CodeMatch BrTrue => new CodeMatch(i => i.opcode == OpCodes.Brtrue || i.opcode == OpCodes.Brtrue_S);

        /// <summary>条件为假/零/空时跳转 (brfalse/brfalse.s)</summary>
        public static CodeMatch BrFalse => new CodeMatch(i => i.opcode == OpCodes.Brfalse || i.opcode == OpCodes.Brfalse_S);

        /// <summary>相等时跳转 (beq/beq.s)</summary>
        public static CodeMatch Beq => new CodeMatch(i => i.opcode == OpCodes.Beq || i.opcode == OpCodes.Beq_S);

        /// <summary>不相等时跳转 (bne.un/bne.un.s) - 用于无符号整数或无序浮点数</summary>
        public static CodeMatch BneUn => new CodeMatch(i => i.opcode == OpCodes.Bne_Un || i.opcode == OpCodes.Bne_Un_S);

        /// <summary>大于等于时跳转 (bge/bge.s)</summary>
        public static CodeMatch Bge => new CodeMatch(i => i.opcode == OpCodes.Bge || i.opcode == OpCodes.Bge_S);

        /// <summary>大于等于时跳转（无符号/无序） (bge.un/bge.un.s)</summary>
        public static CodeMatch BgeUn => new CodeMatch(i => i.opcode == OpCodes.Bge_Un || i.opcode == OpCodes.Bge_Un_S);

        /// <summary>大于时跳转 (bgt/bgt.s)</summary>
        public static CodeMatch Bgt => new CodeMatch(i => i.opcode == OpCodes.Bgt || i.opcode == OpCodes.Bgt_S);

        /// <summary>大于时跳转（无符号/无序） (bgt.un/bgt.un.s)</summary>
        public static CodeMatch BgtUn => new CodeMatch(i => i.opcode == OpCodes.Bgt_Un || i.opcode == OpCodes.Bgt_Un_S);

        /// <summary>小于等于时跳转 (ble/ble.s)</summary>
        public static CodeMatch Ble => new CodeMatch(i => i.opcode == OpCodes.Ble || i.opcode == OpCodes.Ble_S);

        /// <summary>小于等于时跳转（无符号/无序） (ble.un/ble.un.s)</summary>
        public static CodeMatch BleUn => new CodeMatch(i => i.opcode == OpCodes.Ble_Un || i.opcode == OpCodes.Ble_Un_S);

        /// <summary>小于时跳转 (blt/blt.s)</summary>
        public static CodeMatch Blt => new CodeMatch(i => i.opcode == OpCodes.Blt || i.opcode == OpCodes.Blt_S);

        /// <summary>小于时跳转（无符号/无序） (blt.un/blt.un.s)</summary>
        public static CodeMatch BltUn => new CodeMatch(i => i.opcode == OpCodes.Blt_Un || i.opcode == OpCodes.Blt_Un_S);

        /// <summary>无条件跳转 (br/br.s)</summary>
        public static CodeMatch Br => new CodeMatch(i => i.opcode == OpCodes.Br || i.opcode == OpCodes.Br_S);

        /// <summary>离开受保护代码区域 (leave/leave.s)</summary>
        public static CodeMatch Leave => new CodeMatch(i => i.opcode == OpCodes.Leave || i.opcode == OpCodes.Leave_S);

        /// <summary>比较大于 (cgt/cgt.un)</summary>
        public static CodeMatch Cgt => new CodeMatch(i => i.opcode == OpCodes.Cgt || i.opcode == OpCodes.Cgt_Un);

        /// <summary>比较小于 (clt/clt.un)</summary>
        public static CodeMatch Clt => new CodeMatch(i => i.opcode == OpCodes.Clt || i.opcode == OpCodes.Clt_Un);

        /// <summary>加载参数地址</summary>
        public static CodeMatch LdArgA => new CodeMatch(i => i.opcode == OpCodes.Ldarga || i.opcode == OpCodes.Ldarga_S);

        /// <summary>存储参数</summary>
        public static CodeMatch StArg => new CodeMatch(i => i.opcode == OpCodes.Starg || i.opcode == OpCodes.Starg_S);

        /// <summary>加载局部变量地址</summary>
        public static CodeMatch LdLocA => new CodeMatch(i => i.opcode == OpCodes.Ldloca || i.opcode == OpCodes.Ldloca_S);

        /// <summary>加法 (add/add.ovf/add.ovf.un)</summary>
        public static CodeMatch Add =>
            new CodeMatch(i => i.opcode == OpCodes.Add || i.opcode == OpCodes.Add_Ovf || i.opcode == OpCodes.Add_Ovf_Un);

        /// <summary>减法 (sub/sub.ovf/sub.ovf.un)</summary>
        public static CodeMatch Sub =>
            new CodeMatch(i => i.opcode == OpCodes.Sub || i.opcode == OpCodes.Sub_Ovf || i.opcode == OpCodes.Sub_Ovf_Un);

        /// <summary>乘法 (mul/mul.ovf/mul.ovf.un)</summary>
        public static CodeMatch Mul =>
            new CodeMatch(i => i.opcode == OpCodes.Mul || i.opcode == OpCodes.Mul_Ovf || i.opcode == OpCodes.Mul_Ovf_Un);

        /// <summary>除法 (div/div.un)</summary>
        public static CodeMatch Div => new CodeMatch(i => i.opcode == OpCodes.Div || i.opcode == OpCodes.Div_Un);

        /// <summary>取余 (rem/rem.un)</summary>
        public static CodeMatch Rem => new CodeMatch(i => i.opcode == OpCodes.Rem || i.opcode == OpCodes.Rem_Un);

        /// <summary>右移（有符号）</summary>
        public static CodeMatch Shr => new CodeMatch(i => i.opcode == OpCodes.Shr || i.opcode == OpCodes.Shr_Un);

        /// <summary>转换为 int8</summary>
        public static CodeMatch ConvI1 =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_I1 || i.opcode == OpCodes.Conv_Ovf_I1 || i.opcode == OpCodes.Conv_Ovf_I1_Un);

        /// <summary>转换为 int16</summary>
        public static CodeMatch ConvI2 =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_I2 || i.opcode == OpCodes.Conv_Ovf_I2 || i.opcode == OpCodes.Conv_Ovf_I2_Un);

        /// <summary>转换为 int32</summary>
        public static CodeMatch ConvI4 =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_I4 || i.opcode == OpCodes.Conv_Ovf_I4 || i.opcode == OpCodes.Conv_Ovf_I4_Un);

        /// <summary>转换为 int64</summary>
        public static CodeMatch ConvI8 =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_I8 || i.opcode == OpCodes.Conv_Ovf_I8 || i.opcode == OpCodes.Conv_Ovf_I8_Un);

        /// <summary>转换为 float32</summary>
        public static CodeMatch ConvR4 => new CodeMatch(i => i.opcode == OpCodes.Conv_R4 || i.opcode == OpCodes.Conv_R_Un);

        /// <summary>转换为 unsigned int8</summary>
        public static CodeMatch ConvU1 =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_U1 || i.opcode == OpCodes.Conv_Ovf_U1 || i.opcode == OpCodes.Conv_Ovf_U1_Un);

        /// <summary>转换为 unsigned int16</summary>
        public static CodeMatch ConvU2 =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_U2 || i.opcode == OpCodes.Conv_Ovf_U2 || i.opcode == OpCodes.Conv_Ovf_U2_Un);

        /// <summary>转换为 unsigned int32</summary>
        public static CodeMatch ConvU4 =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_U4 || i.opcode == OpCodes.Conv_Ovf_U4 || i.opcode == OpCodes.Conv_Ovf_U4_Un);

        /// <summary>转换为 unsigned int64</summary>
        public static CodeMatch ConvU8 =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_U8 || i.opcode == OpCodes.Conv_Ovf_U8 || i.opcode == OpCodes.Conv_Ovf_U8_Un);

        /// <summary>转换为 native int</summary>
        public static CodeMatch ConvI =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_I || i.opcode == OpCodes.Conv_Ovf_I || i.opcode == OpCodes.Conv_Ovf_I_Un);

        /// <summary>转换为 unsigned native int</summary>
        public static CodeMatch ConvU =>
            new CodeMatch(i => i.opcode == OpCodes.Conv_U || i.opcode == OpCodes.Conv_Ovf_U || i.opcode == OpCodes.Conv_Ovf_U_Un);

        /// <summary>拆箱</summary>
        public static CodeMatch UnBox => new CodeMatch(i => i.opcode == OpCodes.Unbox || i.opcode == OpCodes.Unbox_Any);
    }
}

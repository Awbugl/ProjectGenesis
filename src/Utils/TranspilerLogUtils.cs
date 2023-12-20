using HarmonyLib;

namespace ProjectGenesis.Utils
{
    public static class TranspilerLogUtils
    {
        public static void LogInstructionEnumeration(this CodeMatcher matcher)
        {
            foreach (CodeInstruction codeInstruction in matcher.InstructionEnumeration())
            {
                ProjectGenesis.logger.LogInfo(codeInstruction.ToString());
            }
        }
    }
}

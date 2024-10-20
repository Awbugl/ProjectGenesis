using HarmonyLib;
using ProjectGenesis.Compatibility;

namespace ProjectGenesis.Utils
{
    public static class TranspilerLogUtils
    {
        public static void LogInstructionEnumeration(this CodeMatcher matcher)
        {
            foreach (CodeInstruction codeInstruction in matcher.InstructionEnumeration())
                ProjectGenesis.logger.LogInfo(codeInstruction.ToString());
        }

        public static void LogInstructionEnumerationWhenChecking(this CodeMatcher matcher)
        {
            foreach (CodeInstruction codeInstruction in matcher.InstructionEnumeration())
                InstallationCheckPlugin.logger.LogInfo(codeInstruction.ToString());
        }
    }
}

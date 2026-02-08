using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Utils;
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

        public static void ToILDasmString(this MethodBase methodInfo)
        {
            using (var methodDefinition = new DynamicMethodDefinition(methodInfo))
            {
                string invoke = (string)AccessTools
                   .Method("HarmonyLib.Internal.Util.MethodBodyLogExtensions:ToILDasmString", new[] { typeof(Mono.Cecil.Cil.MethodBody) })
                   .Invoke(null, new object[] { methodDefinition.Definition.Body, });
                InstallationCheckPlugin.logger.LogInfo("Generated IL string for (" + methodInfo.FullDescription() + "):\n" + invoke);
            }
        }
    }
}

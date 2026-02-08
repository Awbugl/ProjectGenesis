using System.Reflection;
using System.Text;
using HarmonyLib;
using MonoMod.Utils;
using ProjectGenesis.Compatibility;

namespace ProjectGenesis.Utils
{
    public static class TranspilerLogUtils
    {
        public static void LogInstructions(this CodeMatcher matcher)
        {
            var sb = new StringBuilder();

            foreach (CodeInstruction codeInstruction in matcher.Instructions()) sb.AppendLine(codeInstruction.ToString());

            ProjectGenesis.logger.LogInfo(sb.ToString());
        }

        public static void LogInstructionsWithOffset(this CodeMatcher matcher, int startOffset, int endOffset)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"LogInstructions {matcher.Pos + startOffset} - {matcher.Pos + endOffset}");
            
            foreach (CodeInstruction codeInstruction in matcher.InstructionsWithOffsets(startOffset, endOffset))
                sb.AppendLine(codeInstruction.ToString());

            ProjectGenesis.logger.LogInfo(sb.ToString());
        }

        public static void LogInstructionsWhenChecking(this CodeMatcher matcher)
        {
            var sb = new StringBuilder();

            foreach (CodeInstruction codeInstruction in matcher.Instructions()) sb.AppendLine(codeInstruction.ToString());

            InstallationCheckPlugin.logger.LogInfo(sb.ToString());
        }

        public static void ToILString(this MethodBase methodInfo)
        {
            using (var methodDefinition = new DynamicMethodDefinition(methodInfo))
            {
                string invoke = (string)AccessTools
                   .Method("HarmonyLib.Internal.Util.MethodBodyLogExtensions:ToILDasmString", new[] { typeof(Mono.Cecil.Cil.MethodBody), })
                   .Invoke(null, new object[] { methodDefinition.Definition.Body, });
                InstallationCheckPlugin.logger.LogInfo("Generated IL string for (" + methodInfo.FullDescription() + "):\n" + invoke);
            }
        }
    }
}

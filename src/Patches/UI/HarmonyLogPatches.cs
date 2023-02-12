using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;

namespace ProjectGenesis.Patches.UI
{
    internal class HarmonyLogListener : ILogListener
    {
        internal static object LogData;

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if (eventArgs.Source.SourceName == "HarmonyX" && eventArgs.Level == LogLevel.Error)
            {
                ProjectGenesis.logger.LogInfo("Harmony throws an error when patching!");
                LogData = eventArgs.Data;
            }
        }

        public void Dispose() { }
    }

    public static class HarmonyLogPatches
    {
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void InvokeOnLoadWorkEnded()
        {
            if (HarmonyLogListener.LogData != null)
                UIFatalErrorTip.instance.ShowError("Harmony throws an error when patching!", HarmonyLogListener.LogData.ToString());
        }
    }
}

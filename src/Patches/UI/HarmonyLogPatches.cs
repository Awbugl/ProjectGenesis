using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;

namespace ProjectGenesis.Patches.UI
{
    internal class HarmonyLogListener : ILogListener
    {
        internal static readonly Queue<object> LogData = new Queue<object>();

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if (eventArgs.Source.SourceName == "HarmonyX" && eventArgs.Level == LogLevel.Error) LogData.Enqueue(eventArgs.Data);
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
            while (HarmonyLogListener.LogData.Count > 0)
                UIFatalErrorTip.instance.ShowError("Harmony throws an error when patching!", HarmonyLogListener.LogData.Dequeue().ToString());
        }
    }
}

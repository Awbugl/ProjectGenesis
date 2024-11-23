using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;

namespace ProjectGenesis.Patches.UI
{
    internal class HarmonyLogListener : ILogListener
    {
        internal static readonly Queue<string> LogData = new Queue<string>();

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            var s = eventArgs.Data.ToString();

            if ((eventArgs.Source.SourceName == "HarmonyX" || s.Contains("InvalidProgramException")) && eventArgs.Level == LogLevel.Error)
                LogData.Enqueue(s);
        }

        public void Dispose() {}
    }

    public static class HarmonyLogPatches
    {
        private static bool _finished;

        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.InvokeOnLoadWorkEnded))]
        [HarmonyPostfix]
        public static void InvokeOnLoadWorkEnded()
        {
            if (_finished) return;

            while (HarmonyLogListener.LogData.Count > 0)
                UIFatalErrorTip.instance.ShowError("Harmony throws an error when patching!", HarmonyLogListener.LogData.Dequeue());

            _finished = true;
        }
    }
}

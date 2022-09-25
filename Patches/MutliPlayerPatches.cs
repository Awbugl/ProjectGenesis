using HarmonyLib;
using NebulaAPI;

namespace ProjectGenesis.Patches
{
    internal static class MutliPlayerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Begin))]
        public static void OnGameBegin()
        {
            if (NebulaModAPI.IsMultiplayerActive && NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new NcModSaveRequest(ProjectGenesis.MODGUID));
        }
    }
}

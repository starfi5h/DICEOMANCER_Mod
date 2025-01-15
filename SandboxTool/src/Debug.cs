#if DEBUG

using GameData;
using HarmonyLib;
using Utilities;
using World;

namespace SandboxTool
{
    public static class Debug
    {
        public static void PrintCollections()
        {
            foreach (var cc in CardDataManager.Instance._colorCardDateCollections)
            {
                Plugin.Log.LogDebug(cc.cardCollectionName);
            }
            foreach (var cc in CardDataManager.Instance._colorCardDateCollectionsUnLocked)
            {
                Plugin.Log.LogDebug(cc.cardCollectionName);
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(WorldRandomSeedUtil), nameof(WorldRandomSeedUtil.GetMapNodeUniqueSeed))]
        static void GetMapNodeUniqueSeed_Postfix()
        {
            Plugin.Log.LogInfo(WorldRandomSeedUtil._mapNodeUniqueSeed);
            Plugin.Log.LogDebug(System.Environment.StackTrace);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.InitializeGame))]
        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.MapEngage))]
        static void InitializeGame_Postfix()
        {
            Plugin.Log.LogWarning("GameManager.InitializeGame or MapEngage");
        }
    }
}

#endif
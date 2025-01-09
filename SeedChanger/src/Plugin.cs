using BepInEx;
using BepInEx.Logging;
using HarmonyLib;


namespace SeedChanger
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "starfi5h.plugin.SeedChanger";
        public const string NAME = "SeedChanger";
        public const string VERSION = "1.0.0";

        public static ManualLogSource Log;
        static Harmony harmony;

        public void Awake()
        {
            Log = Logger;
            harmony = new Harmony(GUID);
            harmony.PatchAll(typeof(SeedManager));
            harmony.PatchAll(typeof(RNG_Map_Patch));
            harmony.PatchAll(typeof(RNG_Loot_Patch));

#if DEBUG
            SeedManager.Start();
#endif
            Log.LogDebug("Seed Changer Load");
        }

#if DEBUG
        public void OnDestroy()
        {
            Log.LogDebug("OnDestroy");
            harmony.UnpatchSelf();
            harmony = null;
            SeedManager.OnDestory();
        }
#endif
    }
}

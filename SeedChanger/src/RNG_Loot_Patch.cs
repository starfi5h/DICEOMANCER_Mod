using HarmonyLib;
using Loot;
using Store;
using Story;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Utilities;
using World;

namespace SeedChanger
{
    public static class RNG_Loot_Patch
    {
		static int lootUniqueSeed;

		public static int GetMapNodeUniqueSeed()
        {
			// Use a separate seed for store and loot
			//Plugin.Log.LogInfo($"Loot: {lootUniqueSeed} + {WorldRandomSeedUtil.GetMapNodeSeed()}");			
			return WorldRandomSeedUtil.GetMapNodeSeed() + lootUniqueSeed++;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.ContinueOnMap))]
		[HarmonyPatch(typeof(MapPlayerTracker), nameof(MapPlayerTracker.RegisterTravelMapNode))]
		public static void ResetLootUniqueSeed()
        {
			// Reset when currentMapNode change (move to the next node)
			lootUniqueSeed = 0;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(LootManager), nameof(LootManager.GenRandomCardByProbabilityPool))]
		[HarmonyPatch(typeof(StoreManager), nameof(StoreManager.GenerateCards))]
		[HarmonyPatch(typeof(StoreManager), nameof(StoreManager.GenerateRelics))]
		[HarmonyPatch(typeof(StoryConsequence), nameof(StoryConsequence.GainRandomRelic))]
		public static IEnumerable<CodeInstruction> GetMapNodeUniqueSeed_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			try
			{
				// Replace the original GetMapNodeUniqueSeed so it is no longer affect by other RNG actions
				var newMethod = AccessTools.Method(typeof(RNG_Loot_Patch), nameof(GetMapNodeUniqueSeed));

				var codeMacher = new CodeMatcher(instructions)
					.MatchForward(false, new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetMapNodeUniqueSeed"))
					.Repeat(matcher => matcher
						.SetAndAdvance(OpCodes.Call, newMethod)) ;

				return codeMacher.InstructionEnumeration();
			}
			catch (Exception e)
			{
				Plugin.Log.LogError("Transpiler GetMapNodeUniqueSeed error");
				Plugin.Log.LogError(e);
				return instructions;
			}
		}
	}
}

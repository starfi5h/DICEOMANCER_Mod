using Combat;
using HarmonyLib;
using Utilities;
using World.ChapterMapGenerator;

namespace SeedChanger
{
    public static class RNG_Map_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChapterMapGeneratorBase), nameof(ChapterMapGeneratorBase.GenerateMap))]
        static void InitRNG()
        {
			int seed = CampaignManager.Instance._campaignRandomSeed;

			UnityEngine.Random.InitState(seed);
			ResetSystemRandom(ShufflingExtension.rng, seed);
			//var uState = UnityEngine.Random.state;
			//Plugin.Log.LogDebug($"unity rand (prev): {uState.s0} , {uState.s1} , {uState.s2} , {uState.s3}");
		}

		public static void ResetSystemRandom(System.Random rng, int Seed)
		{
			int[] seedArray = (int[])AccessTools.Field(typeof(System.Random), "_seedArray").GetValue(rng);

			int num = 0;
			int num2 = (Seed == int.MinValue) ? int.MaxValue : System.Math.Abs(Seed);
			int num3 = 161803398 - num2;
			seedArray[55] = num3;
			int num4 = 1;
			for (int i = 1; i < 55; i++)
			{
				if ((num += 21) >= 55)
				{
					num -= 55;
				}
				seedArray[num] = num4;
				num4 = num3 - num4;
				if (num4 < 0)
				{
					num4 += int.MaxValue;
				}
				num3 = seedArray[num];
			}
			for (int j = 1; j < 5; j++)
			{
				for (int k = 1; k < 56; k++)
				{
					int num5 = k + 30;
					if (num5 >= 55)
					{
						num5 -= 55;
					}
					seedArray[k] -= seedArray[1 + num5];
					if (seedArray[k] < 0)
					{
						seedArray[k] += int.MaxValue;
					}
				}
			}

			AccessTools.Field(typeof(System.Random), "_inext").SetValue(rng, 0);
			AccessTools.Field(typeof(System.Random), "_inextp").SetValue(rng, 21);
		}
	}
}

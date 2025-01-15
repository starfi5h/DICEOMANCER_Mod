using Card;
using Combat;
using GameData;
using HarmonyLib;
using Relic.RelicEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SandboxTool
{
    public static class PoolManager
    {
        static Vector2 scrollPosition;
        static readonly string[] colorTexts = new string[] { "红", "绿", "蓝", "紫", "虚" };
        static readonly string[] rarityTexts = new string[] { "普通", "稀有", "传说" };
        static readonly bool[] colorCardEnables = new bool[5];
        static readonly bool[] colorRelicEnables = new bool[5];
        static bool applyWhenReload = false;
        static string messageText = "注意: 奖励池的修改在没有勾选选项时, 会在SL重新载入时重置!";

        public static void DrawTabContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            applyWhenReload = GUILayout.Toggle(applyWhenReload, "载入时自动依照以下设定修改掉落池");

            // Card Pool Overwrite
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("牌池构成修改");
            for (int i = 0; i < 5; i++)
            {
                colorCardEnables[i] = GUILayout.Toggle(colorCardEnables[i], colorTexts[i]);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("查看当前牌池")) messageText = GetCampaignCardPool();
            if (GUILayout.Button("修改")) messageText = SetCampaignCardPool();
            if (GUILayout.Button("重置")) messageText = ResetCampaignCardPool();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            // Relic Pool Overwrite
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("宝物池修改");
            for (int i = 0; i < 5; i++)
            {
                colorRelicEnables[i] = GUILayout.Toggle(colorRelicEnables[i], colorTexts[i]);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("查看当前宝物池")) messageText = GetCampaignRelicPool();
            if (GUILayout.Button("修改")) messageText = SetCampaignRelicPool();
            if (GUILayout.Button("重置")) messageText = ResetCampaignRelicPool();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();


            // Feedback message
            GUILayout.TextArea(messageText);

            GUILayout.EndScrollView();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignManager), nameof(CampaignManager.SetCampaignPoolByUsedColorE))]
        static void SetCampaignPoolByUsedColorE_Postfix()
        {
            if (applyWhenReload)
            {
                Plugin.Log.LogDebug("Apply campaign card pool & relic pool modification");
                try
                {
                    SetCampaignCardPool();
                    SetCampaignRelicPool();
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError(ex);
                    messageText = "修改掉落池时出错!\n";
                    messageText += ex.ToString();
                }
            }
        }

        static string SetCampaignCardPool()
        {
            if (CampaignManager.Instance?.campaignCardPools == null) return "错误: 不在游戏中";
            var colorList = new List<ColorE>();
            for (int i = 0; i < 5; i++)
            {
                if (colorCardEnables[i]) colorList.Add((ColorE)i);
            }
            if (colorList.Count == 0) return "错误: 必须至少选择1个颜色!";

            var list = CampaignManager.Instance.campaignCardPools;
            int previousCount = list.Count;
            list.Clear();
            foreach (var cardDateCollection in CardDataManager.Instance.GetAllStandardDataCollections())
            {
                foreach (var cardSo in cardDateCollection.GetAllCards())
                {
                    if (cardSo.cardInfo.cardColorEs.All(new Func<ColorE, bool>(colorList.Contains<ColorE>)))
                        list.Add(cardSo);
                }
            }
            int currentCount = list.Count;
            return $"修改完成。牌池卡牌数目: {previousCount} => {currentCount}";
        }

        static string SetCampaignRelicPool()
        {
            var relicDataManager = DataManager.Instance?.GetRelicDataManager();
            if (relicDataManager == null) return "错误: 不在游戏中";
            var colorList = new List<ColorE>();
            for (int i = 0; i < 5; i++)
            {
                if (colorRelicEnables[i]) colorList.Add((ColorE)i);
            }
            if (colorList.Count == 0) return "错误: 必须至少选择1个颜色!";

            var list = relicDataManager._campaignRelicPool;
            int previousCount = list.Count;
            list.Clear();
            foreach (var relicSo in relicDataManager._allStandPoolRelicSo.Values)
            {
                if (relicSo.relicInfo.relicColors.All(new Func<ColorE, bool>(colorList.Contains<ColorE>)))
                    list.Add(relicSo);
            }
            int currentCount = list.Count;
            return $"修改完成。宝物池数目: {previousCount} => {currentCount}";
        }

        static string ResetCampaignCardPool()
        {
            if (CampaignManager.Instance?.campaignCardPools == null) return "错误: 不在游戏中";

            int previousCount = CampaignManager.Instance.campaignCardPools.Count;
            CampaignManager.Instance.SetCampaignCardPool();
            int currentCount = CampaignManager.Instance.campaignCardPools.Count;
            return $"重置完成。牌池卡牌数目: {previousCount} => {currentCount}";
        }

        static string ResetCampaignRelicPool()
        {
            var relicDataManager = DataManager.Instance?.GetRelicDataManager();
            if (relicDataManager == null) return "错误: 不在游戏中";

            int previousCount = relicDataManager._campaignRelicPool.Count;
            DataManager.Instance.GetRelicDataManager().SetCampaignRelicPool(CampaignManager.Instance.campaignUsedColorE);
            int currentCount = relicDataManager._campaignRelicPool.Count;
            return $"重置完成。宝物池数目: {previousCount} => {currentCount}";
        }

        static string GetCampaignCardPool()
        {
            if (CampaignManager.Instance?.campaignCardPools == null) return "错误: 不在游戏中";

            var list = CampaignManager.Instance.campaignCardPools;
            var sb = new StringBuilder();
            var cardBase = new CardBase();
            sb.AppendLine($"当前牌池卡牌数目: {list.Count}");

            var rarityCounts = new int[3];
            foreach (var cardso in list)
            {
                rarityCounts[(int)cardso.CardInfo.rarity]++;
            }
            for (int rarity = 0; rarity <= 2; rarity++)
            {
                sb.AppendLine();
                sb.AppendLine($"==== {rarityTexts[rarity]}:{rarityCounts[rarity]} ====");
                foreach (var cardso in list)
                {
                    if ((int)cardso.cardInfo.rarity != rarity) continue;
                    cardBase._cardName = cardso.cardInfo.cardName;
                    sb.Append(cardBase.GetOwnerLocalizedName());
                    sb.Append(" [");
                    for (int i = 0; i < cardso.cardInfo.cardColorEs.Length; i++)
                    {
                        sb.Append(colorTexts[(int)cardso.cardInfo.cardColorEs[i]]);
                    }
                    sb.Append("] ");
                    sb.Append(cardso.cardInfo.cardName);
                    sb.AppendLine();
                }
            }
            GameObject.Destroy(cardBase);
            return sb.ToString();
        }

        static string GetCampaignRelicPool()
        {
            var relicDataManager = DataManager.Instance?.GetRelicDataManager();
            if (relicDataManager == null) return "错误: 不在游戏中";

            var list = relicDataManager._campaignRelicPool;
            var sb = new StringBuilder();
            var relicBase = new RelicBase();
            sb.AppendLine($"当前宝物池数目: {list.Count}");

            var rarityCounts = new int[3];
            foreach (var relicSo in list)
            {
                rarityCounts[(int)relicSo.relicInfo.relicRarity]++;
            }
            for (int rarity = 0; rarity <= 2; rarity++)
            {
                sb.AppendLine();
                sb.AppendLine($"==== {rarityTexts[rarity]}:{rarityCounts[rarity]} ====");
                foreach (var relicSo in list)
                {
                    if ((int)relicSo.relicInfo.relicRarity != rarity) continue;
                    relicBase.RelicName = relicSo.relicInfo.relicName;
                    sb.Append(relicBase.GetOwnerLocalizedName());
                    sb.Append(" [");
                    for (int i = 0; i < relicSo.relicInfo.relicColors.Length; i++)
                    {
                        sb.Append(colorTexts[(int)relicSo.relicInfo.relicColors[i]]);
                    }
                    sb.Append("] ");
                    sb.Append(relicSo.relicInfo.relicName);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }
}

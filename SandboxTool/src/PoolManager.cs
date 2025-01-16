using Card;
using Combat;
using GameData;
using HarmonyLib;
using Relic;
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
        private const int ColorCount = 6;
        static readonly string[] colorTexts = new string[] { "红", "绿", "蓝", "紫", "虚", "黑" };
        static readonly string[] rarityTexts = new string[] { "普通", "稀有", "传说" };

        static readonly bool[] colorCardEnables = new bool[ColorCount];
        static bool includeStandardCardPool = true;
        static bool includeSpecialCardPool = false;
        static bool includeOtherCardPool = false;
        static string importedCardText = "";
        static int statusCardPool = 0; // 0:vanilla >0:import <0:filtered

        static readonly bool[] colorRelicEnables = new bool[ColorCount];
        static string importedRelicText = "";
        static int statusRelicPool = 0; // 0:vanilla >0:import

        static bool applyWhenReload = false;
        static string messageText = "注意: 奖励池的修改在没有勾选选项时, 会在SL重新载入时重置!";

        public static void DrawTabContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            applyWhenReload = GUILayout.Toggle(applyWhenReload, "载入时自动依照以下设定修改掉落池");

            // Card Pool Overwrite
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            string cardPoolString = "自定义卡池";
            if (statusCardPool < 0) cardPoolString += $"(已修改:{-statusCardPool})";
            else if (statusCardPool > 0) cardPoolString += $"(已导入:{statusCardPool})";
            GUILayout.Label(cardPoolString);
            if (GUILayout.Button("导出")) messageText = PrintCampaignCardPool();
            if (GUILayout.Button("导入"))
            {
                messageText = SetCampaignCardPoolByImport(messageText);
                Plugin.Log.LogInfo(messageText);
                messageText += "\n" + PrintCampaignCardPool();
            }
            GUILayout.EndHorizontal();

            DrawCardPoolFilter();

            GUILayout.EndVertical();

            // Relic Pool Overwrite
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            string relicPoolString = "自定义宝物池";
            if (statusRelicPool < 0) relicPoolString += $"(已修改:{-statusRelicPool})";
            else if (statusRelicPool > 0) relicPoolString += $"(已导入:{statusRelicPool})";
            GUILayout.Label(relicPoolString);
            if (GUILayout.Button("导出")) messageText = PrintRelicPool();
            if (GUILayout.Button("导入"))
            {
                messageText = SetCampaignRelicPoolByImport(messageText);
                Plugin.Log.LogInfo(messageText);
                messageText += "\n" + PrintRelicPool();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("标准池")) messageText = "查看所有标准宝物池:\n" + PrintRelicPool(GetRelicList(0));
            if (GUILayout.Button("颜色池")) messageText = "查看所有颜色宝物池:\n" + PrintRelicPool(GetRelicList(1));
            if (GUILayout.Button("其他")) messageText = "查看所有其他宝物(不掉落):\n" + PrintRelicPool(GetRelicList(2));
            if (GUILayout.Button("重置")) messageText = ResetCampaignRelicPool();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            // Interaction area for message feedback, import and export
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("代码编辑区");
            if (GUILayout.Button("复制")) GUIUtility.systemCopyBuffer = messageText;
            if (GUILayout.Button("黏贴")) messageText = GUIUtility.systemCopyBuffer;
            GUILayout.EndHorizontal();
            messageText = GUILayout.TextArea(messageText);
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        static void DrawCardPoolFilter()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("过滤条件设置");
            if (GUILayout.Button("套用修改"))
            {
                messageText = SetCampaignCardPoolByFilter();
                Plugin.Log.LogInfo(messageText);
                messageText += "\n" + PrintCampaignCardPool();
            }
            if (GUILayout.Button("重置"))
            {
                messageText = ResetCampaignCardPool();
                Plugin.Log.LogInfo(messageText);
                messageText += "\n" + PrintCampaignCardPool();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            includeStandardCardPool = GUILayout.Toggle(includeStandardCardPool, "标准卡池");
            includeSpecialCardPool = GUILayout.Toggle(includeSpecialCardPool, "特殊卡池");
            includeOtherCardPool = GUILayout.Toggle(includeOtherCardPool, "其他");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (int i = 0; i < ColorCount; i++)
            {
                colorCardEnables[i] = GUILayout.Toggle(colorCardEnables[i], colorTexts[i]);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignManager), nameof(CampaignManager.SetCampaignPoolByUsedColorE))]
        static void SetCampaignPoolByUsedColorE_Postfix()
        {
            statusCardPool = 0;
            statusRelicPool = 0;
            if (applyWhenReload)
            {
                Plugin.Log.LogInfo("Apply campaign card pool & relic pool modification");
                try
                {
                    if (string.IsNullOrEmpty(importedCardText)) messageText = SetCampaignCardPoolByFilter();
                    else messageText = SetCampaignCardPoolByImport(importedCardText);

                    if (!string.IsNullOrEmpty(importedRelicText)) messageText += "\n" + SetCampaignRelicPoolByImport(importedRelicText);
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError(ex);
                    messageText = "修改掉落池时出错!\n";
                    messageText += ex.ToString();
                }
            }
        }

        static string SetCampaignCardPoolByImport(string text)
        {
            if (CampaignManager.Instance?.campaignCardPools == null) return "错误: 不在游戏中";

            int previousCount = CampaignManager.Instance.campaignCardPools.Count;
            string[] lines = text.Split(new[] { '\n' });
            var newPool = new HashSet<CardSo>();
            foreach (var line in lines)
            {
                string cardName = line;
                int index = line.LastIndexOf(']');
                if (index >= 0) cardName = line.Substring(index + 1);
                cardName = cardName.Trim();
                //Plugin.Log.LogDebug($"`{cardName}`");
                var cardSo = GetCardSoByName(cardName);
                if (cardSo != null) newPool.Add(cardSo);
            }
            int currentCount = newPool.Count;
            if (currentCount == 0) return "错误: 找不到代码卡牌名称 格式可能不正确";
            CampaignManager.Instance.campaignCardPools.Clear();
            CampaignManager.Instance.campaignCardPools.AddRange(newPool);

            statusCardPool = currentCount; // use positive number to indicated import
            importedCardText = text; // save the success imported text 

            return $"导入完成。牌池卡牌数目: {previousCount} => {currentCount}";
        }

        public static CardSo GetCardSoByName(string cardName)
        {
            if (CardDataManager.Instance._allInGameCardSoDictionary.TryGetValue(cardName, out CardSo cardSo)) return cardSo;
            else return null;
        }

        static string SetCampaignCardPoolByFilter()
        {
            if (CampaignManager.Instance?.campaignCardPools == null) return "错误: 不在游戏中";
            var colorList = new List<ColorE>();
            for (int i = 0; i < ColorCount; i++)
            {
                if (colorCardEnables[i]) colorList.Add((ColorE)i);
            }
            if (colorList.Count == 0) return "错误: 必须至少选择1个颜色!";

            var newPool = new HashSet<CardSo>();
            int previousCount = CampaignManager.Instance.campaignCardPools.Count;
            if (includeStandardCardPool)
            {
                foreach (var cardDateCollection in CardDataManager.Instance.GetAllStandardDataCollections())
                {
                    foreach (var cardSo in cardDateCollection.GetAllCards())
                    {
                        if (cardSo.cardInfo.cardColorEs.All(new Func<ColorE, bool>(colorList.Contains<ColorE>)))
                            newPool.Add(cardSo);
                    }
                }
            }
            if (includeSpecialCardPool)
            {
                foreach (var cardSo in GetAllSpecialCards(true))
                {
                    if (cardSo.cardInfo.cardColorEs.All(new Func<ColorE, bool>(colorList.Contains<ColorE>)))
                        newPool.Add(cardSo);
                }
            }
            if (includeOtherCardPool)
            {
                foreach (var cardSo in GetAllSpecialCards(false))
                {
                    if (cardSo.cardInfo.cardColorEs.All(new Func<ColorE, bool>(colorList.Contains<ColorE>)))
                        newPool.Add(cardSo);
                }
            }
            int currentCount = newPool.Count;
            if (currentCount == 0) return "错误: 卡池中符合过滤条件的卡牌数为0";
            CampaignManager.Instance.campaignCardPools.Clear();
            CampaignManager.Instance.campaignCardPools.AddRange(newPool);

            statusCardPool = -currentCount; // use negative number to indicated filtered
            importedCardText = ""; // indicate user use filter instead of import

            return $"修改完成。牌池卡牌数目: {previousCount} => {currentCount}";
        }

        public static List<CardSo> GetAllSpecialCards(bool isInRandomPool)
        {
            var list = new List<CardSo>();
            foreach (var collection in CardDataManager.Instance._cardDateCollectionDictionary.Values)
            {
                if (collection.cardCollectionType != CardCollectionType.Special) continue;
                if (collection.isInRandomPool != isInRandomPool) continue;

                Plugin.Log.LogDebug(collection.cardCollectionName + ":" + collection.isInRandomPool);
                foreach (var cardSo in collection.GetAllCards())
                {
                    list.Add(cardSo);
                }
            }
            return list;
        }

        static string SetCampaignRelicPoolByImport(string text)
        {
            var relicDataManager = DataManager.Instance?.GetRelicDataManager();
            if (relicDataManager == null) return "错误: 不在游戏中";

            int previousCount = relicDataManager._campaignRelicPool.Count;
            string[] lines = text.Split(new[] { '\n' });
            var newPool = new HashSet<RelicSo>();
            foreach (var line in lines)
            {
                string relicName = line;
                int index = line.LastIndexOf(']');
                if (index >= 0) relicName = line.Substring(index + 1);
                relicName = relicName.Trim();
                //Plugin.Log.LogDebug($"`{relicName}`");
                if (relicDataManager._allInGameRelicSo.TryGetValue(relicName, out RelicSo relicSo)) newPool.Add(relicSo);
            }
            int currentCount = newPool.Count;
            if (currentCount == 0) return "错误: 找不到代码宝物名称 格式可能不正确";
            relicDataManager._campaignRelicPool.Clear();
            relicDataManager._campaignRelicPool.AddRange(newPool);

            statusRelicPool = currentCount; // use positive number to indicated import
            importedRelicText = text; // save the success imported text

            return $"修改完成。宝物池数目: {previousCount} => {currentCount}";
        }

        static List<RelicSo> GetRelicList(int collectionType)
        {
            var relicList = new List<RelicSo>();
            var relicDataManager = DataManager.Instance?.GetRelicDataManager();
            if (relicDataManager == null) return relicList;

            foreach (var collection in relicDataManager._allRelicDataCollections)
            {
                if ((int)collection.relicDataCollectionType != collectionType) continue;
                foreach (var relicSo in collection.relicSos)
                {
                    relicList.Add(relicSo);
                }
            }
            return relicList;
        }

        static string ResetCampaignCardPool()
        {
            if (CampaignManager.Instance?.campaignCardPools == null) return "错误: 不在游戏中";

            statusCardPool = 0;
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

        static string PrintCampaignCardPool()
        {
            if (CampaignManager.Instance?.campaignCardPools == null) return "错误: 不在游戏中";

            var list = CampaignManager.Instance.campaignCardPools;
            var sb = new StringBuilder();
            var cardBase = new CardBase();
            sb.AppendLine($"牌池卡牌数目: {list.Count}");

            var rarityCounts = new int[3];
            foreach (var cardso in list)
            {
                rarityCounts[(int)cardso.CardInfo.rarity]++;
            }
            for (int rarity = 0; rarity < 3; rarity++)
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

        static string PrintRelicPool(List<RelicSo> relicList = null)
        {
            var relicDataManager = DataManager.Instance?.GetRelicDataManager();
            if (relicDataManager == null) return "错误: 不在游戏中";

            if (relicList == null) relicList = relicDataManager._campaignRelicPool;
            var sb = new StringBuilder();
            var relicBase = new RelicBase();
            sb.AppendLine($"宝物池数目: {relicList.Count}");

            var rarityCounts = new int[3];
            foreach (var relicSo in relicList)
            {
                rarityCounts[(int)relicSo.relicInfo.relicRarity]++;
            }
            for (int rarity = 0; rarity <= 2; rarity++)
            {
                sb.AppendLine();
                sb.AppendLine($"==== {rarityTexts[rarity]}:{rarityCounts[rarity]} ====");
                foreach (var relicSo in relicList)
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

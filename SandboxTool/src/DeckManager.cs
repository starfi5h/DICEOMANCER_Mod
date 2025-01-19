using Card;
using Combat;
using Command;
using Cysharp.Threading.Tasks;
using Effects;
using GameData;
using Keyword;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;
using Utilities;

namespace SandboxTool
{
    public static class DeckManager
    {
        static Vector2 scrollPosition;

        static readonly bool[] rarityEnables = new bool[3] { false, false, true };
        static readonly string[] allowKeywords = new string[] { "Bonus", "Coordinated", "Distorted", "Evolve", "Exert", "Fragile",
            "Glitch", "Innate", "Item", "Keep", "Kick", "Limited", "Malady", "Perishable", "Phantom", "Rebound", "Sacrifice", "Virtue", "Wild" };

        static string copyNumInput = "1";
        static string keywordInput = "Exert";
        static string keywordArgInput = "0";
        static string messageText = "";

        public static void DrawTabContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // Gain card & relic by rariy and pool
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("过滤条件:");
            for (int i = 0; i < 3; i++) rarityEnables[i] = GUILayout.Toggle(rarityEnables[i], Strings.RarityTexts[i]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("获得卡牌");
            if (GUILayout.Button("掉落")) GainCards(0, rarityEnables);
            if (GUILayout.Button("标准")) GainCards(1, rarityEnables);
            if (GUILayout.Button("特殊")) GainCards(2, rarityEnables);
            if (GUILayout.Button("其他")) GainCards(3, rarityEnables);
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

            // Mass operations
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("批量操作");
            if (GUILayout.Button("升级")) UpgradeCards();
            if (GUILayout.Button("移除")) RemoveCards();
            if (GUILayout.Button("融合")) FuseCards();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("复制卡牌次数", GUILayout.Width(80));
            copyNumInput = GUILayout.TextField(copyNumInput);
            if (GUILayout.Button("复制"))
            {
                if (int.TryParse(copyNumInput, out int copyCount) && copyCount > 0) DuplicatedCards(copyCount);
                else messageText = $"错误: 复制次数 {copyNumInput} 非正整数!";
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();


            // Keywords
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("关键词", GUILayout.Width(40));
            keywordInput = GUILayout.TextField(keywordInput);
            GUILayout.Label("参数", GUILayout.Width(30));
            keywordArgInput = GUILayout.TextField(keywordArgInput);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("添加"))
            {
                if (!int.TryParse(keywordArgInput, out int keywordArg)) messageText = $"错误: {keywordArg} 不是整数!";
                else
                {
                    bool flag = false;
                    for (int i = 0; i < allowKeywords.Length; i++)
                    {
                        if (allowKeywords[i] == keywordInput)
                        {
                            AddKeyword(keywordInput, keywordArg);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag) messageText = $"错误: {keywordInput} 不是允许添加的关键词! " + ShowKeywords(allowKeywords);
                }
            }
            if (GUILayout.Button("移除")) RemoveKeyword(keywordInput);
            if (GUILayout.Button("列出关键词"))
            {
                bool isControlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                messageText = isControlPressed ? ShowKeywords() : ShowKeywords(allowKeywords);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            // Feedback message
            messageText = GUILayout.TextArea(messageText);

            GUILayout.EndScrollView();
        }

        static void UpgradeCards()
        {
            /*
            PickCardAction((CardBase card) => {
                CardManager.Instance.UpgradeCard(card);
                return UniTask.CompletedTask;
            }, "Key_RemoveCard");
            */

            var list = new List<CardBase>();
            foreach (var card in CardManager.Instance.GetAllPlayerCards())
            {
                if (!card.cardUpgradeInfo.GetHasCardUpgradeInfo() || card.cardUpgradeInfo.isUpgraded) continue;
                list.Add(card);
            }
            new SubCommandPickCardAndAction(new PickCardManager.PickCardConfigure
            {
                pickCardCountMode = PickCardManager.PickCardCountMode.Upto,
                pickAmount = list.Count,
                pickCardPoolType = PickCardManager.PickCardPoolType.DefiniteCardList,
                definiteCardList = list.ToArray(),
                PickCardSubmitAction = async (CardBase card) => { 
                    CardManager.Instance.UpgradeCard(card);
                    await CardManager.Instance.ChangeCardPlace(card, CardPlace.Viewed, -1);
                    await UniTask.Delay(DitwUtil.FrameInterval, false, PlayerLoopTiming.Update, default, false);
                },
                titleKeyword = "",
                actionNameKeyword = ""
            }, true);
        }

        static void RemoveCards()
        {
            new SubCommandPickCardAndAction(new PickCardManager.PickCardConfigure
            {
                pickCardCountMode = PickCardManager.PickCardCountMode.NoLimit,
                pickCardPoolType = PickCardManager.PickCardPoolType.AllCard,
                PickCardSubmitAction = CardManager.Instance.RemoveCardFromGame,
                titleKeyword = "",
                actionNameKeyword = "Key_RemoveCard"
            }, true);
        }

        static void DuplicatedCards(int dupNumber)
        {
            new SubCommandPickCardAndAction(new PickCardManager.PickCardConfigure
            {
                pickCardCountMode = PickCardManager.PickCardCountMode.NoLimit,
                pickCardPoolType = PickCardManager.PickCardPoolType.AllCard,
                PickCardSubmitAction = (CardBase card) => {
                    new SubCommandAddCardToPlayer(card.GetCardDeepCopyInfo().CreateDeepCopy(), CardPlace.Library, dupNumber, null, true, false, false);
                    return UniTask.CompletedTask;
                },
                titleKeyword = "DuplicateCard",
                actionNameKeyword = "DuplicateCard_Des"
            }, true);
        }

        static void FuseCards()
        {
            new SubCommandPickCardAndAction(new PickCardManager.PickCardConfigure
            {
                pickCardCountMode = PickCardManager.PickCardCountMode.NoLimit,
                pickCardPoolType = PickCardManager.PickCardPoolType.AllCard,
                PickCardsArraySubmitAction = async (CardBase[] cards) => {
                    if (cards.Length >= 2)
                    {
                        CardBase originCard = cards[0];
                        foreach (CardBase cardBase in cards)
                        {
                            if (cardBase != originCard)
                            {
                                foreach (EffectBase effectBase in cardBase.GetEffectsList()) originCard.AddEffectToCard(effectBase.GetEffectInfo(), true);
                                foreach (CardKeyword cardKeyword in cardBase.CardKeywords) originCard.AddKeywordToCard(cardKeyword.GetCardKeywordInfo(), true, false);
                                //foreach (ManaCost manaCost in cardBase.GetCostManaColorEArray()) originCard.AddManaCost(manaCost.GetColorEs());
                            }
                        }
                        await CardManager.Instance.ChangeCardPlace(cards[0], CardPlace.Viewed, -1);
                    }
                    await UniTask.Delay(DitwUtil.FrameInterval, false, PlayerLoopTiming.Update, default, false);
                },
                titleKeyword = "Title_FuseCommonCards",
                actionNameKeyword = "Key_FuseCommonCards"
            }, true);
        }

        static void GainCards(int poolType, bool[] filterRarities)
        {
            // EffectSpEffectAddCardToPlayerPermanent.ApplyEffectForTarget
            var list = new List<CardBase>();
            switch (poolType)
            {
                case 0:
                    foreach (var cardSo in CampaignManager.Instance.campaignCardPools)
                    {
                        if (filterRarities[(int)cardSo.cardInfo.rarity])
                            list.Add(CardManager.Instance.GenerateCardObjFromCardInfo(cardSo.CardInfo, OwnerShip.Story));
                    }
                    break;

                case 1:
                    foreach (var cardDateCollection in CardDataManager.Instance.GetAllStandardDataCollections())
                    {
                        foreach (var cardSo in cardDateCollection.GetAllCards())
                        {
                            if (filterRarities[(int)cardSo.cardInfo.rarity])
                                list.Add(CardManager.Instance.GenerateCardObjFromCardInfo(cardSo.CardInfo, OwnerShip.Story));
                        }
                    }
                    break;

                case 2:
                    foreach (var cardSo in PoolManager.GetAllSpecialCards(true))
                    {
                        if (filterRarities[(int)cardSo.cardInfo.rarity])
                            list.Add(CardManager.Instance.GenerateCardObjFromCardInfo(cardSo.CardInfo, OwnerShip.Story));
                    }
                    break;

                case 3:
                    foreach (var cardSo in PoolManager.GetAllSpecialCards(false))
                    {
                        if (filterRarities[(int)cardSo.cardInfo.rarity])
                            list.Add(CardManager.Instance.GenerateCardObjFromCardInfo(cardSo.CardInfo, OwnerShip.Story));
                    }
                    break;
            }
            new SubCommandPickCardAndAction(new PickCardManager.PickCardConfigure
            {
                pickCardCountMode = PickCardManager.PickCardCountMode.Upto,
                pickAmount = list.Count,
                pickCardPoolType = PickCardManager.PickCardPoolType.DefiniteCardList,
                definiteCardList = list.ToArray(),
                PickCardSubmitAction = (CardBase card) => { CardManager.Instance.AddCardToPlayerCards(card, CardPlace.Library); return UniTask.CompletedTask; },
                titleKeyword = "",
                actionNameKeyword = "Key_GainCardFromLootPool"
            }, true);
        }


        #region Keyword

        static string ShowKeywords(string[] keywords = null)
        {            
            var list = new List<string>();
            if (keywords == null) list.AddRange(CardKeyWordUtil.AllKeywordStrings); // KeywordBackup not in use
            else list.AddRange(keywords);
            list.Sort();

            var sb = new StringBuilder();
            foreach (var keywordName in list)
            {
                var localizedString = new LocalizedString("KeyWord", keywordName).GetLocalizedString();
                sb.Append(localizedString);
                sb.Append(" ");
                sb.AppendLine(keywordName);
            }
            if (keywords == null) sb.Insert(0, $"全部关键词:{list.Count}\n\n");
            else sb.Insert(0, $"可选关键词:{list.Count}\n\n");
            return sb.ToString();
        }

        static void AddKeyword(string keyword, int number)
        {
            // SpecialStoryAddKeywordToACard
            var list = (from card in CardManager.Instance.GetAllPlayerCards()
                        where !card.TryGetCardKeywordByName(keyword).hasKeyword
                        select card).ToList();
            if (list.Count != 0)
            {
                new SubCommandPickCardAndAction(new PickCardManager.PickCardConfigure
                {
                    pickCardPoolType = PickCardManager.PickCardPoolType.DefiniteCardList,
                    pickCardCountMode = PickCardManager.PickCardCountMode.Upto,
                    PickCardSubmitAction = async (CardBase card) => {
                        card.AddKeywordToCard(new CardKeyWordInfo(keyword, number), true, false);
                        await CardManager.Instance.ChangeCardPlace(card, CardPlace.Viewed, -1);
                        await UniTask.Delay(DitwUtil.FrameInterval, false, PlayerLoopTiming.Update, default, false);
                    },
                    pickAmount = list.Count,
                    titleKeyword = "AddKeywordToCard",
                    actionNameKeyword = "AddKeywordToCard_Des",
                    definiteCardList = list.ToArray()
                }, true);
            }
        }

        static void RemoveKeyword(string keyword)
        {
            var list = (from card in CardManager.Instance.GetAllPlayerCards()
                                   where card.TryGetCardKeywordByName(keyword).hasKeyword
                                   select card).ToList();
            if (list.Count != 0)
            {
                new SubCommandPickCardAndAction(new PickCardManager.PickCardConfigure
                {
                    pickCardPoolType = PickCardManager.PickCardPoolType.DefiniteCardList,
                    pickCardCountMode = PickCardManager.PickCardCountMode.Upto,
                    PickCardSubmitAction = async (CardBase card) => {
                        card.RemoveCardKeywordFromCard(keyword);
                        await CardManager.Instance.ChangeCardPlace(card, CardPlace.Viewed, -1);
                        await UniTask.Delay(DitwUtil.FrameInterval, false, PlayerLoopTiming.Update, default, false);
                    },
                    pickAmount = list.Count,
                    titleKeyword = "Title_RemoveKeyword",
                    actionNameKeyword = "Key_RemoveKeyword",
                    definiteCardList = list.ToArray()
                }, true);
            }
        }

        #endregion
    }
}

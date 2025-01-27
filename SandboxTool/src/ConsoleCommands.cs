using Ability;
using Card;
using Combat;
using Command;
using GameData;
using Relic;
using System.Text;
using UI;
using UnityEngine;
using UnityEngine.Localization;

namespace SandboxTool
{
    public static class ConsoleCommands
    {
        // from class DebugRedeemCode and DitwTest
        public static string ResultString { get; private set; } = "";

        public static bool Search(string translatedName)
        {
            var relicDataManager = DataManager.Instance?.GetRelicDataManager();
            if (relicDataManager == null || CardDataManager.Instance == null)
            {
                ResultString = "错误: 不在游戏中";
                return false;
            }
            if (string.IsNullOrWhiteSpace(translatedName))
            {
                ResultString = "错误: 搜寻字串为空";
                return false;
            }
            int cardCount = 0;
            var sb = new StringBuilder();
            foreach (var pair in CardDataManager.Instance._allInGameCardSoDictionary)
            {
                var localizedName = new LocalizedString("CardName", pair.Value.cardName).GetLocalizedString();
                if (localizedName.Contains(translatedName) || pair.Key.Contains(translatedName))
                {
                    cardCount++;
                    sb.Append(" [");
                    sb.Append(Strings.RarityTexts[(int)pair.Value.cardInfo.rarity]);
                    sb.Append("] ");
                    sb.Append(localizedName);
                    sb.Append(" ");
                    sb.AppendLine(pair.Key);
                }
            }
            int relicCount = 0;
            foreach (var pair in relicDataManager._allInGameRelicSo)
            {
                var localizedName = new LocalizedString("Relic", pair.Value.relicInfo.relicName).GetLocalizedString();
                if (localizedName.Contains(translatedName) || pair.Key.Contains(translatedName))
                {
                    relicCount++;
                    sb.Append(" [");
                    sb.Append(Strings.RarityTexts[(int)pair.Value.relicInfo.relicRarity]);
                    sb.Append("] ");
                    sb.Append(localizedName);
                    sb.Append(" ");
                    sb.AppendLine(pair.Key);
                }
            }
            ResultString = $"共有{cardCount}卡牌及{relicCount}宝物包含字串'{translatedName}'\n" + sb.ToString();
            return true;
        }

        public static bool GainCard(string cardName)
        {
            var cardSo = CardDataManager.Instance.GetCardSoByName(cardName);
            if (cardSo == null) return false;
            new SubCommandAddCardToPlayer(cardSo.CardInfo, CardPlace.Hand, 1, null, true, false, true);
            return true;
        }

        public static bool RemoveCard(string cardName)
        {
            int count = 0;
            CardBase[] playerCards = CardManager.Instance.GetAllPlayerCards();
            for (int i = 0; i < playerCards.Length; i++)
            {
                if (playerCards[i].name == cardName)
                {
                    new SubCommandRemoveCardFromGame(playerCards[i]);
                    count++;
                }
            }
            return count > 0;
        }

        public static bool GainAllCurrentDropCards(string _)
        {
            foreach (var cardSo in CampaignManager.Instance.campaignCardPools)
            {
                new SubCommandAddCardToPlayer(cardSo.CardInfo, CardPlace.Hand, 1, null, true, false, true);
            }
            return true;
        }

        public static bool RemoveAllCards(string _)
        {
            var playerCards = CardManager.Instance.GetAllPlayerCards();
            for (int i = 0; i < playerCards.Length; i++)
            {
                new SubCommandRemoveCardFromGame(playerCards[i]);
            }
            return CardManager.Instance.GetAllPlayerCards().Length == 0;
        }

        public static bool GainRelic(string relicName)
        {
            var relicSo = DataManager.Instance.GetRelicDataManager().GetRelicSo(relicName);
            if (relicSo == null) return false;
            // The vanilla GenerateAndGainNewRelic prevent gettind duplicated relic, so we use our own
            var relicHolder = RelicManager.Instance.GenerateRelic(relicSo.relicInfo, OwnerShip.Player);
            relicHolder.SetGeneratedPosition();
            // This will check if player has enough slots for active relic
            return RelicManager.Instance.TryAddRelicToPlayer(relicHolder);
        }

        public static bool RemoveRelic(string relicName)
        {
            int count = 0;
            foreach (RelicHolder relicHolder in RelicManager.Instance.GetAllPlayerRelics())
            {
                if (relicHolder.Relic.RelicName == relicName)
                {
                    RelicManager.Instance.RemoveRelicFromGame(relicHolder);
                    count++;
                }
            }
            return count > 0;
        }

        public static bool GainAllRelic(string _)
        {
            foreach (RelicSo relicSo in DataManager.Instance.GetRelicDataManager().GetAllInGameRelicSo())
            {
                RelicManager.Instance.GenerateAndGainNewRelic(relicSo.relicInfo); // No duplicate
            }
            return true;
        }

        public static bool RemoveAllRelics(string _)
        {
            foreach (RelicHolder relicHolder in RelicManager.Instance.GetAllPlayerRelics())
            {
                RelicManager.Instance.RemoveRelicFromGame(relicHolder);
            }
            return RelicManager.Instance.GetAllPlayerRelics().Length == 0;
        }

        public static bool SetMaxRelicSlot(string count)
        {
            if (!int.TryParse(count, out int num) && num < 2) return false;

            int existedSlotCount = RelicManager.Instance.GetMaxActivatedRelicCount();
            RelicManager.Instance.SetMaxActivatedRelicCount(num);
            if (num < existedSlotCount)
            {
                RelicManager.Instance.activatedRelicManager.activatedRelicTransforms.RemoveRange(num, existedSlotCount - num);
                RelicManager.Instance.activatedRelicManager.slotBackground.size = new Vector2(1f, 1.036f * num);
                var transform = RelicManager.Instance.activatedRelicManager.transform;
                ;
                for (int index = existedSlotCount; index > num; index--)
                {
                    var slotObj = transform.GetChild(transform.childCount - 1);
                    var divideObj = transform.GetChild(transform.childCount - 2);
                    Object.DestroyImmediate(slotObj?.gameObject);
                    Object.DestroyImmediate(divideObj?.gameObject);
                }
            }
            return RelicManager.Instance.GetMaxActivatedRelicCount() == num;
        }

        public static bool GainAllAbilityByAbilityType(string type)
        {
            bool result = false;
            foreach (AbilitySo abilitySo in DataManager.Instance.GetAbilityDataManager().GetAllAbilitySos())
            {
                if (abilitySo.abilityInfo.abilityType.ToString() == type)
                {
                    new SubCommandApplyAbility(CharacterPlayer.Instance, abilitySo.abilityInfo, 1, true, true, null);
                    result = true;
                }
            }
            return result;
        }

        public static bool PlayAllCardAnimation(string _)
        {
            var go = new DebugRedeemCode();
            go.PlayAllCardAnimation(_);
            UnityEngine.Object.Destroy(go);
            return true;
        }
    }
}
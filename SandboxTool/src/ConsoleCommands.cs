using Ability;
using Card;
using Combat;
using Command;
using GameData;
using Relic;
using UI;

namespace SandboxTool
{
    public static class ConsoleCommands
    {
        // from class DebugRedeemCode and DitwTest
        public static void GainCard(string cardName)
        {
            new SubCommandAddCardToPlayer(CardDataManager.Instance.GetCardSoByName(cardName).CardInfo, CardPlace.Hand, 1, null, true, false, true);
        }

        public static void RemoveCard(string cardName)
        {
            CardBase[] playerCards = CardManager.Instance.GetAllPlayerCards();
            for (int i = 0; i < playerCards.Length; i++)
            {
                if (playerCards[i].name == cardName) new SubCommandRemoveCardFromGame(playerCards[i]);
            }
        }

        public static void GainAllCurrentDropCards(string _)
        {
            foreach (var cardSo in CampaignManager.Instance.campaignCardPools)
            {
                new SubCommandAddCardToPlayer(cardSo.CardInfo, CardPlace.Hand, 1, null, true, false, true);
            }
        }

        public static void RemoveAllCards(string _)
        {
            CardBase[] playerCards = CardManager.Instance.GetAllPlayerCards();
            for (int i = 0; i < playerCards.Length; i++)
            {
                new SubCommandRemoveCardFromGame(playerCards[i]);
            }
        }

        public static void GainRelic(string relicName)
        {
            RelicSo relicSo = DataManager.Instance.GetRelicDataManager().GetRelicSo(relicName);
            RelicManager.Instance.GenerateAndGainNewRelic(relicSo.relicInfo);
        }

        public static void RemoveRelic(string relicName)
        {
            foreach (RelicHolder relicHolder in RelicManager.Instance.GetAllPlayerRelics())
            {
                if (relicHolder.Relic.RelicName == relicName)
                {
                    RelicManager.Instance.RemoveRelicFromGame(relicHolder);
                    return;
                }
            }
        }

        public static void GainAllRelic(string _)
        {
            foreach (RelicSo relicSo in DataManager.Instance.GetRelicDataManager().GetAllInGameRelicSo())
            {
                RelicManager.Instance.GenerateAndGainNewRelic(relicSo.relicInfo);
            }
        }

        public static void RemoveAllRelics(string _)
        {
            foreach (RelicHolder relicHolder in RelicManager.Instance.GetAllPlayerRelics())
            {
                RelicManager.Instance.RemoveRelicFromGame(relicHolder);
            }
        }

        public static void GainAllAbilityByAbilityType(string type)
        {
            foreach (AbilitySo abilitySo in DataManager.Instance.GetAbilityDataManager().GetAllAbilitySos())
            {
                if (abilitySo.abilityInfo.abilityType.ToString() == type)
                {
                    new SubCommandApplyAbility(CharacterPlayer.Instance, abilitySo.abilityInfo, 1, true, true, null);
                }
            }
        }

        public static void PlayAllCardAnimation(string empty)
        {
            var go = new DebugRedeemCode();
            go.PlayAllCardAnimation(empty);
            UnityEngine.Object.Destroy(go);
        }

    }
}
using Ability;
using Card;
using Combat;
using Command;
using GameData;
using HarmonyLib;
using Relic;
using Relic.RelicEffects;
using RollNumber;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;
using World;

namespace SandboxTool
{
    // Functions from DitwTest

    public static class CombatManager
    {
        static Vector2 scrollPosition;
        static bool overwriteOneDice;
        static string diceNumberInput = "20";
        static bool forceEnableInjure;
        static bool forceDisableInjure;
        static string damageInput = "10";
        static string abilityInput = "Power";
        static string abilityArgInput = "3";
        static string messageText = "";
        static bool accessFlag;

        public static void DrawTabContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            bool tmpBool;

            // timeScale control
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("时间倍率");
            if (GUILayout.Button("<<", GUILayout.Width(30))) Time.timeScale = (int)(Time.timeScale / 2) > 0 ? (int)(Time.timeScale / 2) : 1;
            if (GUILayout.Button("-", GUILayout.Width(30))) Time.timeScale = (int)(Time.timeScale - 1) > 0 ? (int)(Time.timeScale - 1) : 1;
            GUILayout.Label(Time.timeScale.ToString("F1"));
            if (GUILayout.Button("+", GUILayout.Width(30))) Time.timeScale = (int)(Time.timeScale + 1) <= 32 ? (int)(Time.timeScale + 1) : 32;
            if (GUILayout.Button(">>", GUILayout.Width(30))) Time.timeScale = (int)(Time.timeScale * 2) <= 32 ? (int)(Time.timeScale * 2) : 32;
            GUILayout.EndHorizontal();

            // One Dice
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            overwriteOneDice = GUILayout.Toggle(overwriteOneDice, "覆写至尊骰的骰出数值");
            diceNumberInput = GUILayout.TextField(diceNumberInput);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+10充能")) AddOneDice(10);
            if (GUILayout.Button("升級骰子")) UpdargeOneDice();
            if (GUILayout.Button("迷你骰子")) DowngradeOneDiceToD4();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            
            // Battle commands
            GUILayout.BeginVertical(GUI.skin.box);

            // Enable/disable player injure 
            GUILayout.BeginHorizontal();
            tmpBool = GUILayout.Toggle(forceEnableInjure, "强制启用负伤");
            if (tmpBool != forceEnableInjure)
            {
                forceEnableInjure = tmpBool;
                forceDisableInjure &= !tmpBool;
            }
            tmpBool = GUILayout.Toggle(forceDisableInjure, "强制停用负伤");
            if (tmpBool != forceDisableInjure)
            {
                forceDisableInjure = tmpBool;
                forceEnableInjure &= !tmpBool;
            }
            GUILayout.EndHorizontal();

            // Add mana
            GUILayout.BeginHorizontal();
            GUILayout.Label("+能量:");
            for (int i = 0; i < Strings.ColorCount; i++)
                if (GUILayout.Button(Strings.ColorTexts[i])) AddMana((ColorE)i);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("容量-1")) AdjustManaLimit(-1); 
            if (GUILayout.Button("容量+1")) AdjustManaLimit(1);
            if (GUILayout.Button("抽1张牌")) DrawOneCard();
            if (GUILayout.Button("棄1")) DiscardOneCard();
            GUILayout.EndHorizontal();

            // Deal damage
            GUILayout.BeginHorizontal();
            damageInput = GUILayout.TextField(damageInput);
            if (GUILayout.Button("治疗", GUILayout.MaxWidth(70)) && int.TryParse(damageInput, out int heal)) HealPlayer(heal);
            if (GUILayout.Button("伤害", GUILayout.MaxWidth(70)) && int.TryParse(damageInput, out int damage)) DamageEnemy(damage);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            // Ability
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("能力", GUILayout.Width(30));
            abilityInput = GUILayout.TextField(abilityInput);
            GUILayout.Label("增减层数", GUILayout.Width(60));
            abilityArgInput = GUILayout.TextField(abilityArgInput);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("己方")) messageText = SetAbiltiyAlly(abilityInput, abilityArgInput);
            if (GUILayout.Button("敌方")) messageText = SetAbiltiyEnemies(abilityInput, abilityArgInput);
            if (GUILayout.Button("列出能力"))
            {
                messageText = ShowCommonAbiltities(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.TextArea(messageText);

            GUILayout.EndScrollView();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterPlayer), nameof(CharacterPlayer.CheckHasInjure))]
        static void CheckHasInjure_Postfix(ref bool __result)
        {
            if (forceDisableInjure) __result = false;
            if (forceEnableInjure) __result = true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(RNumber), nameof(RNumber.SetRNumberByInfoWithChaos))]
        static void SetRNumberByInfoWithChaos(ref RNumberInfo rNumberInfo, bool isPermanent)
        {
            if (overwriteOneDice && isPermanent && accessFlag)
            {
                accessFlag = false; // Only the player edit action can go through
                if (int.TryParse(diceNumberInput, out int number) && number >= 0)
                {
                    Plugin.Log.LogInfo("SetRNumberByInfoWithChaos overwrite value: " + number);
                    rNumberInfo.isDiceNumber = false;
                    rNumberInfo.oriNumber = number;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RollNumberManager), nameof(RollNumberManager.SubmitChangeRNumber))]
        static void SubmitChangeRNumber_Prefix()
        {
            accessFlag = overwriteOneDice;
        }

        static void AddOneDice(int count)
        {            
            RelicHolder item = RelicManager.Instance.HasCertainRelic("TheOneDice").relicBase;
            if (item != null)
            {
                new SubCommandChangeRNumber(item.relicChargeRNumber, new RNumberInfo(item.relicChargeRNumber.GetCollapsedNumber(true) + count, false, "Standard", null), SubCommandChangeRNumber.ChangeType.Regular, true);
            }
        }

        static void UpdargeOneDice()
        {
            new CommandUpgradeTheOneDice();
        }

        static void DowngradeOneDiceToD4()
        {
            RelicHolder item = RelicManager.Instance.HasCertainRelic("TheOneDice").relicBase;
            if (item != null)
            {
                if (item.Relic is RelicTheOneDice relicTheOneDice)
                {
                    relicTheOneDice.SetTheOneDiceFaces(DiceUtilities.DiceType.D4);
                }
            }
        }

        static void AddMana(ColorE manaColor)
        {
            new SubCommandAddManaS(manaColor, 1, false, true);
        }

        static void AdjustManaLimit(int deltaAmount)
        {
            if (ManaManager.Instance == null) return;
            int newMax = ManaManager.Instance.GetMaxCount() + deltaAmount;
            ManaManager.Instance.SetMaxManaCount(newMax);
        }

        static void DrawOneCard()
        {
            new SubCommandDrawOneCard(null, false, null, true);
        }

        static void DiscardOneCard()
        {            
            List<CardBase> cardHand = CardManager.Instance.cardHand;
            if (cardHand.Count >= 1) new SubCommandDiscardCard(cardHand[0], false);
        }

        static void DamageEnemy(int damageAmount)
        {
            if (EnemyManager.Instance.GetAllEnemy().Count > 0)
            {
                new SubCommandDealDamage(new Damage(damageAmount, WorldManager.Instance, EnemyManager.Instance.GetAllEnemy()[0], DamageSourceType.IndirectDamage, DamageSpecialType.Standard), true);
            }
        }

        static void HealPlayer(int healAmount)
        {
            new SubCommandHeal(CharacterPlayer.Instance, healAmount);
        }

        static string SetAbiltiyAlly(string abiltiyName, string abilityNumberString)
        {
            if (!int.TryParse(abilityNumberString, out int level)) return $"错误: {abilityNumberString} 非整数!";

            foreach (AbilitySo abilitySo in DataManager.Instance.GetAbilityDataManager().GetAllAbilitySos())
            {
                if (abilitySo.abilityInfo.abilityName == abiltiyName)
                {
                    //new SubCommandApplyAbility(CharacterPlayer.Instance, abilitySo.abilityInfo, number, true, true, null);
                    CharacterPlayer.Instance.GainAbility(abilitySo.abilityInfo, level);
                    return $"成功: {abiltiyName} {level}";
                }
            }
            return $"失败: 找不到 {abiltiyName}";            
        }

        static string SetAbiltiyEnemies(string abiltiyName, string abilityNumberString)
        {
            if (!int.TryParse(abilityNumberString, out int level)) return $"错误: {abilityNumberString} 非整数!";

            foreach (AbilitySo abilitySo in DataManager.Instance.GetAbilityDataManager().GetAllAbilitySos())
            {
                if (abilitySo.abilityInfo.abilityName == abiltiyName)
                {
                    foreach (var character in EnemyManager.Instance.GetAllEnemy())
                    {
                        //new SubCommandApplyAbility(character, abilitySo.abilityInfo, number, true, true, null);
                        character.GainAbility(abilitySo.abilityInfo, level);
                    }
                    return $"成功: {abiltiyName} {level}";
                }
            }
            return $"失败: 找不到 {abiltiyName}";
        }

        static string ShowCommonAbiltities(bool showAll)
        {
            var abilityManager = DataManager.Instance.GetAbilityDataManager();
            if (abilityManager == null) return "错误: 尚未载入";

            var lists = new List<string>[5];
            for (int i = 0; i < 5; i++) lists[i] = new List<string>();            

            foreach (AbilitySo abilitySo in abilityManager.GetAllAbilitySos())
            {
                if (abilitySo.abilityOwnerShip != AbilityOwnerShip.All && !showAll) continue;

                lists[(int)abilitySo.abilityInfo.abilityType].Add(abilitySo.abilityInfo.abilityName);
            }

            var sb = new StringBuilder();
            if(showAll) sb.AppendLine("[Ctrl]显示所有能力");
            else sb.AppendLine("敌我通用能力");
            for (int i = 0; i < 5; i++)
            {
                if (lists[i].Count == 0) continue;
                sb.AppendLine($"==== {(AbilityType)i}:{lists[i].Count} ====");
                foreach (var abiltiyName in lists[i])
                {
                    sb.Append(new LocalizedString("Ability", abiltiyName).GetLocalizedString());
                    sb.Append(" ");
                    sb.AppendLine(abiltiyName);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}

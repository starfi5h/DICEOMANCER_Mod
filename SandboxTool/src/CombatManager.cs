using Card;
using Combat;
using Command;
using HarmonyLib;
using Relic;
using Relic.RelicEffects;
using RollNumber;
using System;
using UnityEngine;
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
        static readonly string[] colorTexts = new string[] { "红", "绿", "蓝", "紫", "虚", "黑" };
        static string damageInput = "10";

        static bool accessFlag;

        public static void DrawTabContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            bool tmpBool;

            // timeScale control
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("时间流逝速率");
            if (GUILayout.Button("<<") && Time.timeScale >= 2) Time.timeScale = (int)(Time.timeScale / 2) > 0 ? (int)(Time.timeScale / 2) : 1;
            GUILayout.Label(Time.timeScale.ToString("F1") + "x");
            if (GUILayout.Button(">>")) Time.timeScale = (int)(Time.timeScale * 2);
            GUILayout.EndHorizontal();

            // One Dice
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            overwriteOneDice = GUILayout.Toggle(overwriteOneDice, "覆写至尊骰数值");
            diceNumberInput = GUILayout.TextField(diceNumberInput);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+10充能")) AddOneDice(10);
            if (GUILayout.Button("升級至尊骰")) UpdargeOneDice();
            if (GUILayout.Button("迷你至尊骰")) DowngradeOneDiceToD4();
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
            for (int i = 0; i < 6; i++)
                if (GUILayout.Button(colorTexts[i])) AddMana((ColorE)i);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("容量-1")) AdjustManaLimit(-1); 
            if (GUILayout.Button("容量+1")) AdjustManaLimit(1);
            if (GUILayout.Button("抽1张牌")) DrawOneCard();
            GUILayout.EndHorizontal();

            // Deal damage
            GUILayout.BeginHorizontal();
            damageInput = GUILayout.TextField(damageInput);
            if (GUILayout.Button("治疗", GUILayout.MaxWidth(70)) && int.TryParse(damageInput, out int heal)) HealPlayer(heal);
            if (GUILayout.Button("伤害", GUILayout.MaxWidth(70)) && int.TryParse(damageInput, out int damage)) DamageEnemy(damage);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();


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
    }
}

using Combat;
using HarmonyLib;
using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;

namespace SeedChanger
{
    public static class SeedManager
    {
        public static int CustomSeed { get; set; }
        static TextMeshProUGUI seedText;

        public static void RefreshText(string text = "")
        {
            if (seedText == null) return;
            if (!string.IsNullOrEmpty(text))
            { 
                seedText.text = text;
                return;
            }
            int seed = CampaignManager.Instance._campaignRandomSeed;
            if (seed == 0) // Menu
            {
                if (CustomSeed <= 0) seedText.text = "Set Seed: None";
                else seedText.text = "Set Seed: " + CustomSeed.ToString("x8");
            }
            else // In game
            {
                seedText.text = "Seed: " + seed.ToString("x8");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Start))]
        public static void Start()
        {
            if (CampaignManager.Instance == null) return;
            int seed = CampaignManager.Instance._campaignRandomSeed;
            Plugin.Log.LogInfo($"GameManager.Start Seed: {seed:x8} ({seed})");

            if (seedText == null)
            {
                try
                {
                    Plugin.Log.LogDebug("Creating seed viewer text...");
                    InitUI();
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning("Error when initial seed text!");
                    Plugin.Log.LogWarning(ex);
                }
            }
            RefreshText();
        }

        public static void OnDestory()
        {
            UnityEngine.Object.Destroy(seedText?.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignManager), nameof(CampaignManager.GenerateNewSeed))]
        static void GenerateNewSeed_Postfix(CampaignManager __instance)
        {
            if (CustomSeed > 0)
            {
                __instance._campaignRandomSeed = CustomSeed;
                Plugin.Log.LogInfo($"Overwrite with custom seed: {CustomSeed:x8} ({CustomSeed})");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameOverScreen), nameof(GameOverScreen.BackToMainMenu))]
        [HarmonyPatch(typeof(GameOverScreen), nameof(GameOverScreen.BackToMiddleRealm))]
        static void SetModeToNewGame_Postfix()
        {
            // When player go back to middle realm, reset _campaignRandomSeed to 0 to indicate the mode change
            if (CampaignManager.Instance != null) CampaignManager.Instance._campaignRandomSeed = 0;
        }

        static void InitUI()
        {
            var textGo = new GameObject();
            textGo.SetActive(false);
            textGo.name = "SeedText";
            textGo.AddComponent<SeedSetter>();
            seedText = textGo.AddComponent<TextMeshProUGUI>();
            seedText.color = new Color(1, 0.6f, 0, 1);
            seedText.fontSize = 26;
            seedText.alignment = TextAlignmentOptions.Right;

            var refGo = GameObject.Find("UIManager/TopUICanvas/CloseMapButton");
            if (refGo == null)
            {
                Plugin.Log.LogWarning("Can't find UIManager/TopUICanvas/CloseMapButton!");
                return;
            }
            textGo.transform.SetParent(refGo.transform.parent);
            textGo.transform.localScale = Vector3.one;
            textGo.transform.localPosition = refGo.transform.localPosition + new Vector3(-10, 37, 0);
            textGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 20);
            textGo.SetActive(true);
        }
    }

    public class SeedSetter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            int seed = CampaignManager.Instance._campaignRandomSeed;

            if (seed == 0) // Menu
            {
                if (eventData.button == PointerEventData.InputButton.Right)
                {
                    SeedManager.CustomSeed = 0;
                    SeedManager.RefreshText();
                }
                else
                {
                    string content = GUIUtility.systemCopyBuffer;
                    Plugin.Log.LogDebug($"Paste clipboard: [{content}]");
                    if (!int.TryParse(content, System.Globalization.NumberStyles.HexNumber, null, out int value))
                    {

                        SeedManager.CustomSeed = 0;
                        SeedManager.RefreshText($"Invaild hex format! ({content})");
                    }
                    else if (value <= 0)
                    {
                        SeedManager.CustomSeed = 0;
                        SeedManager.RefreshText($"Value should be greater than zero! ({content})");
                    }
                    else
                    {
                        SeedManager.CustomSeed = value;
                        SeedManager.RefreshText();
                    }
                }
            }
            else
            {
                GUIUtility.systemCopyBuffer = seed.ToString("x8");
                SeedManager.RefreshText("Copied!");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            int seed = CampaignManager.Instance._campaignRandomSeed;
            if (seed == 0)
            {
                SeedManager.RefreshText("Left click to paste clipboard; Right click to clear");
            }
            else
            {
                SeedManager.RefreshText("Click to copy to clipboard");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SeedManager.RefreshText();
        }
    }    
}

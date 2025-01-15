using Combat;
using Command;
using HarmonyLib;
using Story;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Utilities;
using World;

namespace SandboxTool
{
    public static class MapManager
    {
        static Vector2 scrollPosition;
        static string storyInput = "";
        static string enemyInput = "";
        static string messageText = "此页功能请在地图路线选择介面使用!";

        public static void DrawTabContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // Story events
            GUILayout.BeginVertical(GUI.skin.box);

            // CommandTransitToSpecificScene.GameSceneInfo CollapseMapNodeToSceneInfo
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("火堆")) EnterRest();
            if (GUILayout.Button("商店")) EnterStore();
            if (GUILayout.Button("升级")) EnterUpgrade();
            if (GUILayout.Button("删牌")) EnterRemove();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("事件", GUILayout.Width(30));
            storyInput = GUILayout.TextField(storyInput);
            if (GUILayout.Button("输入", GUILayout.Width(50))) messageText = NodeStory(storyInput);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("怪物", GUILayout.Width(30));
            enemyInput = GUILayout.TextField(enemyInput);
            if (GUILayout.Button("输入", GUILayout.Width(50))) messageText = NodeEnemy(enemyInput);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("事件列表")) messageText = GetStoryList();
            if (GUILayout.Button("怪物列表")) messageText = GetEnemyList();
            if (GUILayout.Button("离开事件")) ExitStory();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();


            // Feedback message
            GUILayout.TextArea(messageText);

            GUILayout.EndScrollView();
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(StoryManager), "RefreshView")]
        public static Exception SupressException(Exception __exception)
        {
            if (__exception != null)
            {
                Plugin.Log.LogWarning(__exception);
                messageText = __exception.ToString();
            }
            return null;
        }

        static void ExitStory()
        {
            Plugin.Log.LogInfo("ExitStory");
            StoryManager.Instance.EndStory();
            GameManager.Instance.LeaveStoryScene();
            WorldManager.Instance.SetCampaignState(CampaignState.Traveling);
        }

        static void EnterRest()
        {
            new CommandTransitToSpecificScene(new CommandTransitToSpecificScene.GameSceneInfo
            {
                SceneType = CommandTransitToSpecificScene.GameSceneType.Rest
            });
        }

        static void EnterStore()
        {
            new CommandTransitToSpecificScene(new CommandTransitToSpecificScene.GameSceneInfo
            {
                SceneType = CommandTransitToSpecificScene.GameSceneType.Store
            });
        }

        static void EnterUpgrade()
        {
            new CommandTransitToSpecificScene(new CommandTransitToSpecificScene.GameSceneInfo
            {
                SceneType = CommandTransitToSpecificScene.GameSceneType.SmallGame,
                SmallGameBase = WorldManager.Instance.upgradeCardSmallGamePrefab
            });
        }

        static void EnterRemove()
        {
            new CommandTransitToSpecificScene(new CommandTransitToSpecificScene.GameSceneInfo
            {
                SceneType = CommandTransitToSpecificScene.GameSceneType.SmallGame,
                SmallGameBase = WorldManager.Instance.removeCardSmallGamePrefab
            });
        }

        static string NodeStory(string storyName)
        {
            storyName = storyName.Trim();
            if (string.IsNullOrWhiteSpace(storyName)) return "字串为空";
            foreach (StorySo storySo in Resources.LoadAll<StorySo>("ScriptableObjects/Stories"))
            {
                if (storySo.storyTitle == storyName)
                {
                    new CommandTransitToSpecificScene(new CommandTransitToSpecificScene.GameSceneInfo
                    {
                        SceneType = CommandTransitToSpecificScene.GameSceneType.Story,
                        StorySo = storySo
                    });
                    return "成功: " + storyName;
                }
            }
            return "失败: 找不到" + storyName;
        }

        public static string GetStoryList()
        {
            var list = new List<string>();
            foreach (StorySo storySo in Resources.LoadAll<StorySo>("ScriptableObjects/Stories"))
            {
                // Only list story that have proper images
                if (storySo.storyStates == null || storySo.storyStates.Length == 0) continue;
                bool flagNoSprite = false;
                foreach (var storyState in storySo.storyStates)
                {
                    if (storyState.storyStateSprite == null || storyState.storyStateSprite.name == "unfinished")
                    {
                        flagNoSprite = true;
                        //Plugin.Log.LogWarning(storySo.storyTitle);
                        break;
                    }
                }
                if (flagNoSprite) continue;
                list.Add(storySo.storyTitle);
            }
            list.Sort();
            var sb = new StringBuilder();
            sb.Insert(0, "可选事件数目: " + list.Count + "\n");
            foreach (var storyTitle in list)
                sb.AppendLine(storyTitle);

            /*
            if (WorldManager.Instance?.ChapterSoPool == null) return "当前不在游戏中!";
            foreach (var storySo in WorldManager.Instance.ChapterSoPool.AllPossibleStory)
            {
                sb.AppendLine(storySo.storyTitle);
            }
            */

            return sb.ToString();
        }

        static string NodeEnemy(string enemyName)
        {
            enemyName = enemyName.Trim();
            if (string.IsNullOrWhiteSpace(enemyName)) return "字串为空";
            foreach (EncounterEnemySO encounterEnemySO in Resources.LoadAll<EncounterEnemySO>("ScriptableObjects/EnemyEncounter"))
            {
                if (encounterEnemySO.encounterName == enemyName)
                {
                    new CommandTransitToSpecificScene(new CommandTransitToSpecificScene.GameSceneInfo
                    {
                        SceneType = CommandTransitToSpecificScene.GameSceneType.Battle,
                        EncounterEnemySo = encounterEnemySO
                    });
                    return "成功: " + enemyName;
                }
            }
            return "失败: 找不到" + enemyName;
        }

        static string GetEnemyList()
        {
            var list = new List<string>();            
            foreach (EncounterEnemySO encounterEnemySO in Resources.LoadAll<EncounterEnemySO>("ScriptableObjects/EnemyEncounter"))
            {
                list.Add(encounterEnemySO.encounterName);
            }
            list.Sort();

            var sb = new StringBuilder();
            sb.Insert(0, "可选敌人数目: " + list.Count + "\n");
            foreach (var name in list)
                sb.AppendLine(name);
            return sb.ToString();
        }
    }
}

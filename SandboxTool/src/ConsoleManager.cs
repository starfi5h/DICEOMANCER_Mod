using System;
using System.Collections.Generic;
using System.Reflection;
using UI;
using UnityEngine;
using World;

namespace SandboxTool
{
    public static class ConsoleManager
    {
        static DebugRedeemCode debugRedeemCode;
        static readonly Dictionary<string, MethodInfo> methodDict = new Dictionary<string, MethodInfo>();

        static Vector2 scrollPosition;
        static string commandInput;
        static string messageText;

        public static void DrawTabContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginHorizontal();
            GUILayout.Label("指令", GUILayout.Width(30));
            commandInput = GUILayout.TextField(commandInput);
            if (GUILayout.Button("输入", GUILayout.Width(50)))
            {
                messageText = ExecuteCode(commandInput);
            }
            GUILayout.EndHorizontal();

            GUILayout.TextArea(messageText);

            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"地图节点随机数:{WorldRandomSeedUtil._mapNodeUniqueSeed}");
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        public static void Init()
        {
            debugRedeemCode = new DebugRedeemCode();

            // Init method blacklist
            var skipNames = new HashSet<string>
            {
                "OnClickSubmitButton",
                "ClearInputField",
                "Update",
                "SubmitCode",
                "CheckCode",
                "EnableDebugCheat"
            };
            foreach (var methodInfo in typeof(MonoBehaviour).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                skipNames.Add(methodInfo.Name);
            }

            var methodInfos = typeof(DebugRedeemCode).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var methodInfo in methodInfos)
            {
                string name = methodInfo.Name;
                if (skipNames.Contains(name)) continue;
                if (methodInfo.GetParameters().Length < 1) continue;
                methodDict.Add(name, methodInfo);
                //Plugin.Log.LogDebug($"{name}:{methodInfo.GetParameters()[0].Name}");
            }
            messageText = "可用指令:\n" + CommandListText();
        }

        static string ExecuteCode(string code)
        {
            string parameter = "";
            string[] array = code.Split(':', (char)StringSplitOptions.None);            
            if (array.Length >= 2) parameter = array[1];
            string command = array[0];

            if (!methodDict.TryGetValue(command, out var methodInfo))
            {
                return "Error method not found: " + command + "\n可用指令:\n" + CommandListText();
            }
            try
            {
                methodInfo.Invoke(debugRedeemCode, new object[] { parameter });
            }
            catch (Exception ex)
            {
                return "Error invoking method " + command + ": " + ex.Message;
            }
            return "Success!";
        }

        static string CommandListText()
        {
            string text = "";
            foreach (var methodInfo in methodDict.Values)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters[0].Name == "empty")
                    text += $"{methodInfo.Name}\n";
                else
                    text += $"{methodInfo.Name}:[{parameters[0].Name}]\n";
            }
            return text;
        }
    }
}

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
            var methodInfos = typeof(ConsoleCommands).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var methodInfo in methodInfos)
            {
                string name = methodInfo.Name;
                if (methodInfo.GetParameters().Length < 1) continue;
                methodDict.Add(name, methodInfo);
            }
            messageText = "可用指令:\n" + CommandListText();
        }

        static string ExecuteCode(string code)
        {
            string parameter = "";
            string[] array = code.Split(':', (char)StringSplitOptions.None);            
            if (array.Length >= 2) parameter = array[1].Trim();
            string command = array[0].Trim();

            if (!methodDict.TryGetValue(command, out var methodInfo))
            {
                return "找不到指令: " + command + "\n可用指令:\n" + CommandListText();
            }
            Plugin.Log.LogInfo("Execute command " + command + ":" + parameter);
            bool result;
            try
            {
                result = (bool)methodInfo.Invoke(null, new object[] { parameter });
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(ex);
                return "指令报错 " + command + ": " + ex.Message;
            }
            Plugin.Log.LogInfo("Execute result: " + result);
            var output = (result ? "成功! " : "失敗! ") + command + ":" + parameter;
            if (command == "Search") output += "\n" + ConsoleCommands.ResultString;
            return output;
        }

        static string CommandListText()
        {
            string text = "";
            foreach (var methodInfo in methodDict.Values)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters[0].Name == "_") text += $"{methodInfo.Name}\n";
                else text += $"{methodInfo.Name}:[{parameters[0].Name}]\n";
            }
            return text;
        }
    }
}

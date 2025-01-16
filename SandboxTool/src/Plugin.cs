using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace SandboxTool
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "starfi5h.plugin.SandboxTool";
        public const string NAME = "SandboxTool";
        public const string VERSION = "1.0.1";

        public static ManualLogSource Log;
        private static Harmony harmony;

        private ConfigEntry<KeyboardShortcut> mainWindowShortcut;        
        private bool showWindow = true;
        private string windowName = "沙盒工具";
        private const int windowId = 12800;
        private Rect windowRect = new Rect(100, 150, 300, 330);
        private int selectedTab = 0;
        private readonly string[] tabNames = { "戰鬥", "地图", "卡池", "控制台" };
        private bool isResizing;

        public void Awake()
        {
            Log = Logger;
            harmony = new Harmony(GUID);
            mainWindowShortcut = Config.Bind("KeyBind", "Main Window Shortcut", new KeyboardShortcut(KeyCode.F1),
                "Hotkey to open the mod main window\n开启视窗的热键");
            windowName += " (" + mainWindowShortcut.Value.ToString() + ")";

            ConsoleManager.Init();
            harmony.PatchAll(typeof(CombatManager));
            harmony.PatchAll(typeof(MapManager));
            harmony.PatchAll(typeof(PoolManager));

#if DEBUG
            //Debug.PrintCollections();
            harmony.PatchAll(typeof(Debug));
#endif
            //DontDestroyOnLoad(this); // Fix plugin gets destory in BepInEx 5.4.23
            Log.LogInfo("SandboxTool Load. Version:" + VERSION);
        }


        public void OnDestroy()
        {
            Log.LogInfo("OnDestroy");
#if DEBUG
            harmony.UnpatchSelf();
            harmony = null;
#endif
        }

        public void Update()
        {
            // Toggle window visibility with hotkey
            if (mainWindowShortcut.Value.IsDown())
            {
                showWindow = !showWindow;
            }
        }

        public void OnGUI()
        {
            if (!showWindow) return;

            // Make the window draggable and get the returned position
            Color originalColor = GUI.backgroundColor; // Save the original color
            GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);
            windowRect = GUILayout.Window(windowId, windowRect, DrawWindow, windowName);
            HandleResize(ref windowRect);
            GUI.backgroundColor = originalColor;
        }

        private void DrawWindow(int windowID)
        {
            // Draw close button
            if (GUI.Button(new Rect(windowRect.width - 23, 3, 20, 20), "X"))
            {
                showWindow = false;
                return;
            }

            GUILayout.Space(2);

            // Draw tab buttons
            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabNames.Length; i++)
            {
                GUI.backgroundColor = selectedTab == i ? Color.gray : Color.white;
                if (GUILayout.Button(tabNames[i], GUILayout.Height(25)))
                {
                    selectedTab = i;
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            // Draw tab content
            switch (selectedTab)
            {
                case 0:
                    CombatManager.DrawTabContent();
                    break;
                case 1:
                    MapManager.DrawTabContent();
                    break;
                case 2:
                    PoolManager.DrawTabContent();
                    break;
                case 3:
                    ConsoleManager.DrawTabContent();
                    break;
            }

            // Make the window draggable            
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));            
        }

        private void HandleResize(ref Rect windowRect)
        {
            Rect resizeHandleRect = new Rect(windowRect.xMax - 10, windowRect.yMax - 10, 25, 25);
            
            if (resizeHandleRect.Contains(Event.current.mousePosition) && !windowRect.Contains(Event.current.mousePosition))
            {
                GUI.Box(resizeHandleRect, "↘"); // Draw a resize handle in the bottom-right corner for 20x20 pixel
                if (Event.current.type == EventType.MouseDown)
                {
                    isResizing = true;
                }
            }
            if (Event.current.type == EventType.MouseUp)
            {
                isResizing = false;
            }

            if (isResizing)
            {
                // Calculate new window size based on mouse position, keeping the minimum window size as 30x30
                windowRect.xMax = Math.Max(Event.current.mousePosition.x, windowRect.xMin + 30);
                windowRect.yMax = Math.Max(Event.current.mousePosition.y, windowRect.yMin + 30);
            }

            // EatInputInRect
            if (!(Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))) //Eat only when left-click
                return;
            if (windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                Input.ResetInputAxes();
        }
    }
}

using System.Collections.Generic;

namespace SandboxTool
{
    public static class Strings
    {
        public static readonly string[] TabNames = new string[] { "戰鬥", "地图", "牌组", "卡池", "指令" };
        public const int ColorCount = 6;
        public const int RarityCount = 3;
        public static readonly string[] ColorTexts = new string[] { "红", "绿", "蓝", "紫", "虚", "黑" };
        public static readonly string[] RarityTexts = new string[] { "普通", "稀有", "传说" };

        /* For future translation json file
        static readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();
        
        public static string Get(string keyString)
        {
            if (dictionary.TryGetValue(keyString, out string translatedString)) return translatedString;
            return keyString;
        }
        */
    }
}

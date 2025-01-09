# Seed Changer

You can set the starting seed to play the same run.  
可以自己设置游戏的开局随机种子  

----

## Usage 用法

![menu](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SeedChanger/img/menu.png)  
![ingame](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SeedChanger/img/ingame.png)  
The seed code is hex (0-9,a-f) of 32-bit positive integer.  
During gaming, you can click on the top-right orange seed text to copy the seed.  
In menu, left click on the seed text to paste and set the seed code from the clipborad.   
If the format is incorrect, it will reject and set the seed back to random.  

种子码是以十六进制数(0-9,a-f)表示的32位正整数。  
游戏过程中，您可以点击右上角的橙色种子文字来复制种子。  
在菜单中，左键单击橙色文字以从剪贴板粘贴种子代码设置。  
如果格式不正确，它将拒绝并将种子重新设置为随机。  

----

## Installation 安装

1. Install [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.17) framework.  
2. Put SeedChanger.dll in BepInEx\plugins folder.  

....  

1. 安装 [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.17) 框架。  
2. 将 SeedChanger.dll 放入 BepInEx\plugins 文件夹中。  

----

## Others

In this mod, the starting seed only decides
- Map generation  
- Event types  
- Loot (cards, relics)  
- Shop item  
  
The rest may still random and share the same random number.

在此mod中，起始种子只决定
- 地图生成  
- 事件类型  
- 战利品（卡牌、宝物）  
- 商店物品  
  
其余部分仍可能为随机，共用相同的随机数。
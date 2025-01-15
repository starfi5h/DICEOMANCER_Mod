# 沙盒工具 SandboxTool

这个Mod引入了多种新功能，让你能更好地控制游戏中的各种机制。


## 功能

默认热键F1开启mod视窗，在视窗右下角可以拖曳调整大小

----

### 战斗
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_combat.png)  
#### 动画速度调整
- 时间流逝速率：调整游戏中时间流逝的速度。  
使用<<按钮减慢时间速度（将当前速度除以2，最低为1倍）>>按钮加速时间（将当前速度乘以2）。

#### 至尊骰修改
- 开启“覆写至尊骰数值”选项来手动设定骰子数值。在旁边输入框中输入你希望的数值(非负整数)。
- +10充能：为你的骰子增加10点充能。
- 升級至尊骰：升级你的骰子。
- 迷你至尊骰：将你的骰子降级为D4。

#### 战斗机制
- 强制启用/停用负伤：非红色职业也可以开启负伤机制。
- +能量：点击不同颜色的按钮来增加对应颜色的能量。
- 容量-1 和 容量+1：调整你的能量容量上限。
- 抽1张牌：抽取一张卡片。  
- 治疗：根据输入的数字治疗玩家。  
- 伤害：根据输入的数字对敌人造成伤害。  

----

### 地图
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_map.png)  
- 在当前节点进入篝火，商店，升级，删除场景  
- 进入输入字串的事件场景  
- 进入输入字串的怪物场景  
- 列出可用的事件和怪物场景名称
  
此页功能请在地图路线选择介面使用，否则可能会报错

----

### 卡池
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_drop.png)  
- 载入时自动依照以下设定修改掉落池  
注意：奖励池的修改在没有勾选选项时，会在SL重新载入时重置!
- 牌池构成修改：将职业池打混，以选定的颜色重新分配卡池  
用这个功能就能将紫卡从掉落卡池中排除  
- 宝物池修改
- 列出当前的卡牌池和宝物池

----

### 命令台
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_console.png)  
格式为[命令]:[参数]。举例：
- `GainCard:BalanceTweak` 在卡组中加入一张`平衡调整`
- `GainRelic:ForgetMeNot` 获得宝物`勿忘我`
- `GainAllRelic` 得到所有遗物
- `RemoveAllCards` 移除玩家所有的牌
  
地图节点随机数：随机数_mapNodeUniqueSeed的值，可以观察世界线变更的情况  

----

## 安装 Installation

1. 安装 [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.17) 框架。  
2. 将 SandboxTool.dll 放入 BepInEx\plugins 文件夹中。  

----

## 设置 Config

在配置文件`BepInEx\config\starfi5h.plugin.SandboxTool.cfg`(游戏带mod启动一次后产生)  
可以设置mod开启视窗的热键, 默认为F1  

## 更新日志 Changelog

- v1.0.0 - Initial Release. (Game version v1.1.15) 2025/1/16
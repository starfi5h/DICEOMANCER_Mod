# 沙盒工具 SandboxTool

此控制台Mod引入了调试和扩展功能，让你能更好地控制游戏中的各种机制。  
默认热键F1开启mod视窗，在视窗右下角可以拖曳调整大小。  
感谢杀戮尖塔Loadout Mod的启发。  

----

## 战斗
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_combat.png)  

### 动画速度调整
- 时间倍率：调整游戏中时间流逝的速度。允许范围为1倍至32倍。<<减半，-减一，+加一，>>翻倍。

### 至尊骰修改
- 开启“覆写至尊骰数值”选项来手动设定骰子数值。在旁边输入框中输入你希望的数值(非负整数)。
- +10充能：为至尊骰宝物增加10点充能。
- 升級骰子：升级至尊骰。
- 迷你骰子：将至尊骰降级为四面骰D4。

### 战斗机制
- 强制启用/停用负伤：非红色职业也可以开启负伤机制。
- +能量：点击不同颜色的按钮来增加对应颜色的能量。
- 容量-1 和 容量+1：调整你的能量容量上限。
- 抽1张牌：抽取一张卡片。
- 弃1：弃掉最左手边的卡片。
- 治疗：根据输入的数字治疗玩家。  
- 伤害：根据输入的数字对敌人造成伤害。  

### 能力修改
- 输入框：能力(英文)，层数为正是添加，为负是减少
- 己方：为玩家角色增减能力
- 敌方：为所有敌方角色增减能力
- 列出能力：显示通用能力。按住Ctrl显示所有能力。
----

## 地图
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_map.png)  
- 在当前节点进入篝火，商店，升级，删除场景  
- 进入输入字串的事件场景  
- 进入输入字串的怪物场景  
- 列出可用的事件和怪物场景名称
  
此页功能请在地图路线选择介面使用，否则可能会报错

----

## 牌组
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_deck.png) 

### 过滤与获取卡牌
- 按稀有度筛选卡牌，并通过以下按钮获取：
  - 掉落：从目前的可掉落卡池获取卡牌
  - 标准：包含所有职业的可掉落卡池
  - 特殊：只能透过商店或特殊事件获取的卡池
  - 其他

### 批量操作修改牌库
- 升级：升级选定卡牌。
- 移除：移除选定卡牌。
- 融合：选择两张或多张卡牌，将它们的效果和关键词叠加到第一张选定的卡牌中。
- 复制：多次复制指定卡牌。输入复制次数后，点击“复制”按钮。

### 关键词修改
- 添加关键词：为卡牌添加指定关键词及其参数。
- 移除关键词：移除卡牌的某个关键词。
- 列出关键词：显示可用关键词。按住Ctrl显示所有关键词。

### 消息反馈
操作结果会显示在下方的消息框中，包括成功或错误信息。

----

## 卡池
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_drop.png)  
允许你自定义游戏中的卡牌和遗物掉落池,让你能更好地控制游戏过程中可以获得的卡牌和遗物。

### 卡牌池设置
- 可以通过颜色筛选卡牌(红、绿、蓝、紫、虚、黑)
- 可选择包含的卡池类型:
  - 标准卡池: 原版游戏的掉落卡池，包含所有职业
  - 特殊卡池: 不在一般的掉落卡池中, 只能透过商店或特殊事件获取
  - 其他卡牌: 衍生卡牌、教学或废案等
- 支持导出当前卡池配置
- 支持导入自定义卡池配置
- 可以随时重置回默认卡池

### 遗物池设置
- 支持导出当前遗物池配置
- 支持导入自定义遗物池配置
- 可以查看不同类型的遗物池:
  - 标准池
  - 颜色池
  - 其他遗物(不会自然掉落的)
- 可以随时重置回默认遗物池

### 使用说明

1. 如果希望自定义设置在每次读档时自动生效,请勾选"载入时自动依照以下设定修改掉落池"选项。

2. 修改卡池:
   - 使用筛选功能: 选择想要的卡牌颜色和卡池类型,然后点击"套用修改"
   - 使用导入功能: 将之前导出的卡池配置粘贴到代码文本区,然后点击"导入"

3. 修改遗物池:
   - 可以先查看各类遗物池的内容
   - 将想要的遗物池配置粘贴到代码文本区,然后点击"导入"

4. 如果想要保存当前配置:
   - 点击"导出"按钮
   - 点击"复制"将配置保存到剪贴板
   - 建议将配置保存到文本文件中以便后续使用

5. 如果对修改结果不满意,随时可以点击"重置"恢复到默认设置。

代码格式: 每一行为一张卡牌/遗物,以最后面的英文辨识名称。  
注意: 如果没有勾选"载入时自动依照以下设定修改掉落池"选项,你的自定义设置会在重新读档时重置。

----

## 指令
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_console.png)  
格式为[命令]:[参数]。举例：
- `Search:骰` 搜索名称包含'骰'的卡牌及宝物。列出稀有度,名称,英文辨识名称
- `GainCard:BalanceTweak` 在牌组中加入一张`平衡调整`
- `RemoveCard:Dice` 从牌组中移除卡牌`至尊骰`
- `RemoveAllCards` 移除玩家所有的牌
- `GainRelic:TalismanOfPGB` 获得宝物`工匠護符`
- `RemoveRelic:TheOneDice` 移除宝物`至尊骰`
- `GainAllRelic` 得到所有宝物
- `RemoveAllRelics` 移除所有宝物
- `SetMaxRelicSlot:5` 将主动宝物槽位扩增至5
- `GainAllAbilityByAbilityType:Util` 战斗中获得所有Util能力
- `PlayAllCardAnimation` 展示所有动画
  
地图节点随机数：随机数_mapNodeUniqueSeed的值，可以观察世界线变更的情况  

----

## 安装 Installation

1. 安装 [BepInEx 5.4.17](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.17) 框架。  
2. 将 SandboxTool.dll 放入 BepInEx\plugins 文件夹中。  

启用成功时, 会在游戏起始页面看到mod视窗。  
如果视窗没有出现, 请将`BepInEx\config\BepInEx.cfg`配置文件中将HideManagerGameObject选项设为true  

## 设置 Config

在配置文件`BepInEx\config\starfi5h.plugin.SandboxTool.cfg`(游戏带mod启动一次后产生)  
可以设置mod开启视窗的热键, 默认为F1  

----

## 更新日志 Changelog

- v1.0.3 - 允许加入重复宝物。新增命令:搜索Search,增加主动槽位SetMaxRelicSlot 2025/1/27
- v1.0.2 - 加入牌组及卡片修改功能。加入增益减益修改功能。更新指令集 2025/1/20
- v1.0.1 - 加入卡池导入功能 2025/1/16
- v1.0.0 - Initial Release. (Game version v1.1.15) 2025/1/16
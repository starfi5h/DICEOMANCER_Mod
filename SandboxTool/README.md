# 沙盒工具 SandboxTool

这个Mod引入了多种新功能，让你能更好地控制游戏中的各种机制。  
默认热键F1开启mod视窗，在视窗右下角可以拖曳调整大小。  

----

## 战斗
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

## 地图
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_map.png)  
- 在当前节点进入篝火，商店，升级，删除场景  
- 进入输入字串的事件场景  
- 进入输入字串的怪物场景  
- 列出可用的事件和怪物场景名称
  
此页功能请在地图路线选择介面使用，否则可能会报错

----

## 卡池
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_drop.png)  
允许你自定义游戏中的卡牌和遗物掉落池,让你能更好地控制游戏过程中可以获得的卡牌和遗物。

### 卡牌池设置
- 可以通过颜色筛选卡牌(红、绿、蓝、紫、虚、黑)
- 可选择包含的卡池类型:
  - 标准卡池: 原版游戏的掉落卡池，包含所有职业
  - 特殊卡池: 不在一般的掉落卡池中, 只能透过商店或特殊事件获取
  - 其他卡牌: 衍生卡牌等
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

## 命令台
![img](https://raw.githubusercontent.com/starfi5h/DICEOMANCER_Mod/dev/SandboxTool/img/sandboxTool_console.png)  
格式为[命令]:[参数]。举例：
- `GainCard:BalanceTweak` 在卡组中加入一张`平衡调整`
- `GainRelic:ForgetMeNot` 获得宝物`勿忘我`
- `GainAllRelic` 得到所有遗物
- `RemoveAllCards` 移除玩家所有的牌
  
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

- v1.0.1 - 加入卡池导入功能 2025/1/16
- v1.0.0 - Initial Release. (Game version v1.1.15) 2025/1/16
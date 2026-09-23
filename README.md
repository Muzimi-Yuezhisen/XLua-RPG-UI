# XLua RPG UI Demo

基于 Unity 6、UGUI、XLua 与 AssetBundle 实现的 RPG UI 与背包系统原型。C# 负责 Lua 环境和资源加载等底层服务，Lua 负责界面生命周期、交互逻辑与数据展示。

## 功能

- Lua 驱动主界面与背包界面的显示、隐藏和事件响应
- 装备、道具、宝石三类物品页签切换
- 根据玩家数据动态生成物品格并刷新数量
- 通过 JSON 配置物品名称、类型、图标和说明
- 从 AssetBundle 加载 UI Prefab、JSON 与 SpriteAtlas
- 封装同步、异步资源加载、依赖缓存和卸载接口
- 配置 `UnityAction` 与 `UnityAction<bool>`，支持 Lua 绑定 UGUI 事件

## 核心流程

1. `Main.cs` 初始化 `LuaEnv` 并执行 `Main.lua`。
2. Lua 初始化类型别名、物品配置和玩家背包数据。
3. `MainPanel` 响应入口按钮并打开 `BagPanel`。
4. `BagPanel` 根据页签选择数据，创建 `ItemGrid`。
5. `ItemGrid` 从 SpriteAtlas 获取图标并刷新数量文本。

## 主要目录

- `Assets/Scripts/ProjectBase`：Lua 环境与 AssetBundle 管理
- `Assets/Lua`：UI、数据与 Lua 面向对象实现
- `Assets/ABRes/Prefabs/UI`：主界面、背包和物品格 Prefab
- `Assets/ABRes/Json`：物品配置
- `Assets/ABRes/SpriteAtlas`：物品图集

## 运行环境

- Unity `6000.0.77f1`
- XLua
- UGUI / TextMeshPro

使用 Unity 打开项目后运行 `Assets/Scenes/SampleScene.unity`。

## 说明

当前项目重点展示 Lua UI 架构、C# 与 Lua 交互以及 AssetBundle 资源加载。版本比对、远端下载和本地资源替换等完整热更新流程尚未包含在本项目中。

print("ping")
-- 缩写类别名
require("InitClass")

-- 初始化道具表信息
require("ItemData")

-- 读取玩家信息1本地2服务器
require("PlayerData")
PlayerData:Init()

require("BasePanel")
require("MainPanel")
require("BagPanel")
require("ItemGird")
MainPanel:ShowMe("MainPanel")

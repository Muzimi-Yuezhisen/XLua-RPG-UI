-- 读出json表
local txt = ABMgr:LoadRes("json", "ItemData", typeof(TextAsset))
local itemList = json.decode(txt.text)
-- 转存成字典
ItemData = {}
for _, v in pairs(itemList) do
    ItemData[v.id] = v
end

-- 将格子封装成类
Object:subClass("ItemGrid")
-- 成员变量
ItemGrid.obj = nil
ItemGrid.imgIcon = nil
ItemGrid.Text = nil

-- 成员函数
-- 实例化格子对象
function ItemGrid:Init(father, posX, posY)
    self.obj = ABMgr:LoadRes("ui", "ItemGrid")
    self.obj.transform:SetParent(father, false)
    self.obj.transform.localPosition = Vector3(posX, posY, 0)
    self.imgIcon = self.obj.transform:Find("bk/imgIcon"):GetComponent(typeof(Image))
    self.Text = self.obj.transform:Find("bk/Text (TMP)"):GetComponent(typeof(TMP_Text))
end

-- 初始化格子信息
function ItemGrid:InitData(data)
    local itemData = ItemData[data.id]
    local strs = string.split(itemData.icon, "_")
    local spriteAtlas = ABMgr:LoadRes("ui", strs[1], typeof(SpriteAtlas))
    self.imgIcon.sprite = spriteAtlas:GetSprite(strs[2])
    self.Text.text = data.num
end

function ItemGrid:Destroy()
    GameObject.Destroy(self.obj)
    self.obj = nil
end

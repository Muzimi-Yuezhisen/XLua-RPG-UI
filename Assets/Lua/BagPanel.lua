BasePanel:subClass("BagPanel")

BagPanel.Content = nil
BagPanel.Content = nil
BagPanel.items = {}
BagPanel.nowType = -1

function BagPanel:Init(name)
    self.base.Init(self, name)

    if self.isInitEvent == false then
        self.Content = self:GetControl("svBag", "ScrollRect").transform:Find("Viewport/Content")

        self:GetControl("BtnClose", "Button").onClick:AddListener(function ()
            self:HideMe()
        end)

        self:GetControl("togEquip", "Toggle").onValueChanged:AddListener(function (value)
            if value == true then
                self:ChangeType(1)
            end
        end)
        self:GetControl("togItem", "Toggle").onValueChanged:AddListener(function (value)
            if value == true then
                self:ChangeType(2)
            end
        end)
        self:GetControl("togGem", "Toggle").onValueChanged:AddListener(function (value)
            if value == true then
                self:ChangeType(3)
            end
        end)
        self.isInitEvent = true
    end
end

function BagPanel:ShowMe(name)
    self.base.ShowMe(self, name)
    if self.nowType == -1 then
        self:ChangeType(1)
    end
end

function BagPanel:HideMe()
    self.panelObj:SetActive(false)
end

-- 切换页签 1.装备 2.道具 3.宝石
function BagPanel:ChangeType(type)
    if self.nowType == type then
        return
    end
    -- 更新前删除旧格子
    for i = 1, #self.items do
        self.items[i]:Destroy()
    end
    self.items = {}
    self.nowType = type

    local nowItems = nil
    if type == 1 then
        nowItems = PlayerData.equips
    elseif type == 2 then
        nowItems = PlayerData.items
    elseif type == 3 then
        nowItems = PlayerData.gems
    end

    for i = 1, #nowItems do
        local grid = ItemGrid:new()
        grid:Init(self.Content, (i - 1) % 4 * 175, math.floor((i - 1) / 4) * 175)
        grid:InitData(nowItems[i])
        table.insert(self.items, grid)
    end
end

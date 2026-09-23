-- 面向对象的面板基类
Object:subClass("BasePanel")

BasePanel.panelObj = nil
-- 模拟一个字典，键为控件名，值为控件
BasePanel.controls = {}
-- 避免重复监听
BasePanel.isInitEvent = false

function BasePanel:Init(name)
    if self.panelObj == nil then
        self.panelObj = ABMgr:LoadRes("ui", name, typeof(GameObject))
        self.panelObj.transform:SetParent(Canvas, false)
        -- 找到所有的UI控件
        local allControls = self.panelObj:GetComponentsInChildren(typeof(UIBehaviour))
        -- 为了只保留有用的控件，控件命名按照制定规则
        for i = 0, allControls.Length - 1 do
            local controlName = allControls[i].name
            if string.find(controlName, "btn") ~= nil or string.find(controlName, "Btn") ~= nil
                or string.find(controlName, "tog") ~= nil or string.find(controlName, "img") ~= nil
                or string.find(controlName, "sv") ~= nil or string.find(controlName, "txt") ~= nil then
                local typeName = allControls[i]:GetType().Name
                -- 避免出现一个对象上挂载多个UI控件
                if self.controls[controlName] ~= nil then
                    self.controls[controlName][typeName] = allControls[i]
                else
                    -- 存储形式：{btnRole = {Image = 控件, Button = 控件}，
                    --          togItem = {Toggle = 控件}}
                    self.controls[controlName] = { [typeName] = allControls[i] }
                end
            end
        end
    end
end

function BasePanel:GetControl(name, typename)
    if self.controls[name] ~= nil then
        local sameNameControls = self.controls[name]
        if sameNameControls[typename] ~= nil then
            return sameNameControls[typename]
        end
    end
    return nil
end

function BasePanel:ShowMe(name)
    self:Init(name)
    self.panelObj:SetActive(true)
end

function BasePanel:HideMe()
    self.panelObj:SetActive(false)
end

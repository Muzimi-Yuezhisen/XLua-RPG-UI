BasePanel:subClass("MainPanel")

function MainPanel:Init(name)
    self.base.Init(self, name)
    if self.isInitEvent == false then
        self:GetControl("btnRole", "Button").onClick:AddListener(function ()
            self:BtnRoleClick()
        end)
        self.isInitEvent = true
    end
end

function MainPanel:BtnRoleClick()
    BagPanel:ShowMe("BagPanel")
end

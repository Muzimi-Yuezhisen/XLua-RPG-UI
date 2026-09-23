PlayerData = {}

--道具信息
PlayerData.equips = {}
PlayerData.items = {}
PlayerData.gems = {}

--初始化道具
function PlayerData:Init()

    table.insert(self.equips,{id = 1,num = 1})
    table.insert(self.equips,{id = 2,num = 1})

    table.insert(self.items,{id = 3,num = 50})
    table.insert(self.items,{id = 4,num = 10})

    table.insert(self.gems,{id = 5,num = 99})
    table.insert(self.gems,{id = 6,num = 188})
end

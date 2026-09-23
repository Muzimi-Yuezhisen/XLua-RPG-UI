-- 导入脚本
require("Object")
require("SplitTools")
require("JsonUtility")
Json = json

-- 简化书写
GameObject = CS.UnityEngine.GameObject
Resources = CS.UnityEngine.Resources
Transform = CS.UnityEngine.Transform
RectTransform = CS.UnityEngine.RectTransform
TextAsset = CS.UnityEngine.TextAsset

SpriteAtlas = CS.UnityEngine.U2D.SpriteAtlas
Vector3 = CS.UnityEngine.Vector3
Vector2 = CS.UnityEngine.Vector2

UI = CS.UnityEngine.UI
Image = UI.Image
Text = UI.Text
Button = UI.Button
Toggle = UI.Toggle
ScrollRect = UI.ScrollRect
TMP_Text = CS.TMPro.TextMeshProUGUI
UIBehaviour = CS.UnityEngine.EventSystems.UIBehaviour

-- 找Canvas，只要一次就可以
Canvas = GameObject.Find("Canvas").transform

-- 自己写的C#脚本
ABMgr = CS.ABMgr.GetInstance()

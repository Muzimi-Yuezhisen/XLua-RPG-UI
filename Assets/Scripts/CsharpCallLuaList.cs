using System;
using System.Collections.Generic;
using UnityEngine.Events;
using XLua;

public static class CsharpCallLuaList
{
    // Lua 函数作为 C# 回调时，对应委托需要加入生成列表。
    [CSharpCallLua]
    public static List<Type> csharpCallLuaList = new List<Type>()
    {
        typeof(UnityAction),
        typeof(UnityAction<bool>),
    };
}

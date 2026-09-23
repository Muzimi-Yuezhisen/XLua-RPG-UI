using System.IO;
using UnityEngine;
using XLua;

public class LuaMgr : BaseManager<LuaMgr>
{

    private LuaEnv luaEnv;

    //得到Lua中的_G
    public LuaTable Global
    {
        get
        {
            return luaEnv.Global;
        }
    }

    //初始化
    public void Init()
    {
        if (luaEnv != null) return;
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(MyCustomLoader);
        luaEnv.AddLoader(MyCustomABLoader);
#if UNITY_EDITOR
        StartEmmyLuaDebugger();
#endif
    }

    //重定向
    private byte[] MyCustomLoader(ref string filePath)
    {
        string path = Application.dataPath + "/Lua/" + filePath + ".lua";

        if (File.Exists(path))
        {
            // xLua 用这个字符串作为 chunk 名；必须是真实文件路径，EmmyLua 才能对上断点
            filePath = Path.GetFullPath(path).Replace('\\', '/');
            return File.ReadAllBytes(path);
        }

        if (filePath != "emmy_core")
        {
            Debug.Log("MyCustomLoader重定向失败，文件路径为" + path);
        }
        return null;
    }

    //重定向AB包
    private byte[] MyCustomABLoader(ref string filePath)
    {
        string path = Application.streamingAssetsPath + "/lua";
        if (!File.Exists(path))
        {
            return null;
        }

        AssetBundle ab = AssetBundle.LoadFromFile(path);
        if (ab == null)
        {
            return null;
        }

        TextAsset tx = ab.LoadAsset<TextAsset>(filePath + ".lua");
        return tx != null ? tx.bytes : null;
    }

#if UNITY_EDITOR
    private void StartEmmyLuaDebugger()
    {
        string dllDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Tools/EmmyLua")).Replace('\\', '/');
        string script =
            "package.cpath = package.cpath .. ';" + dllDir + "/?.dll'\n" +
            "local ok, dbg = pcall(require, 'emmy_core')\n" +
            "if not ok then\n" +
            "    print('EmmyLua debugger load failed: ' .. tostring(dbg))\n" +
            "    return\n" +
            "end\n" +
            "local cok, err = pcall(function()\n" +
            "    dbg.tcpConnect('127.0.0.1', 9966)\n" +
            "    dbg.waitIDE()\n" +
            "end)\n" +
            "if cok then\n" +
            "    print('EmmyLua debugger connected')\n" +
            "else\n" +
            "    print('EmmyLua debugger connect failed: ' .. tostring(err))\n" +
            "end\n";
        luaEnv.DoString(script);
        // 等 VS Code 把断点发过来，再去 require Main.lua
        System.Threading.Thread.Sleep(500);
    }
#endif

    //执行
    public void DoString(string str)
    {
        if(luaEnv == null)
        {
            Debug.Log("解析器未初始化");
            return;
        }
        luaEnv.DoString(str);
    }

    public void DoLuaFile(string fileName)
    {
        string str = string.Format("require('{0}')", fileName);
        DoString(str);
    }

    //释放垃圾
    public void Tick()
    {
        if (luaEnv == null)
        {
            Debug.Log("解析器未初始化");
            return;
        }
        luaEnv.Tick();
    }

    //销毁
    public void Dispose()
    {
        if (luaEnv == null)
        {
            Debug.Log("解析器未初始化");
            return;
        }
        luaEnv.Dispose();
        luaEnv = null;
    }
}

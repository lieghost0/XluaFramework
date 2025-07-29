using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode GameMode;
    void Start()
    {
        Manager.Event.Subscribe(10000, OnLuaInit);

        AppConst.GameMode = this.GameMode;
        DontDestroyOnLoad(this);

        Manager.Resource.ParseVersionFile();
        Manager.Lua.Init();
    }

    private static void OnLuaInit(object args)
    {
        Manager.Lua.StartLua("main");

        //效率低下 不使用
        XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
        func.Call();
    }

    private void OnApplicationQuit()
    {
        Manager.Event.Unsubscribe(10000, OnLuaInit);
    }
}

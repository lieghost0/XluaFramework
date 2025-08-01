using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode GameMode;
    public bool OpenLog;
    void Start()
    {
        Manager.Event.Subscribe((int)GameEvent.StartLua, StartLua);
        Manager.Event.Subscribe((int)GameEvent.GameInit, GameInit);

        AppConst.GameMode = this.GameMode;
        AppConst.OpenLog = this.OpenLog;
        DontDestroyOnLoad(this);

        if (AppConst.GameMode == GameMode.UpdateMode)
            this.gameObject.AddComponent<HotUpdate>();
        else
            Manager.Event.Fire((int)GameEvent.GameInit);

    }

    private void GameInit(object args)
    {
        if(AppConst.GameMode != GameMode.EditorMode)
            Manager.Resource.ParseVersionFile();
        Manager.Lua.Init();
    }

    private static void StartLua(object args)
    {
        Manager.Lua.StartLua("main");

        //效率低下 不使用
        XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
        func.Call();

        Manager.Pool.CreateGameObjectPool("UI", 10);
        Manager.Pool.CreateGameObjectPool("Monster", 120);
        Manager.Pool.CreateGameObjectPool("Effect", 120);
        Manager.Pool.CreateAssetPool("AssetBundle", 10);
    }

    private void OnApplicationQuit()
    {
        Manager.Event.Unsubscribe((int)GameEvent.StartLua, StartLua);
        Manager.Event.Unsubscribe((int)GameEvent.GameInit, GameInit);
    }
}

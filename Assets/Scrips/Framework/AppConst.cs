using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    EditorMode,
    PackageBundle,
    UpdateMode,
}

public enum GameEvent
{
    GameInit = 10000,
    StartLua,
}
public class AppConst
{
    public const string BundleExtension = ".ab";
    public const string FileListName = "filelist.txt";
    public const string MD5FileListName = "MD5filelist.txt";
    public static GameMode GameMode = GameMode.EditorMode;
    public static bool OpenLog = true;
    //热更资源的地址
    public const string ResourcesUrl = "http://192.168.1.6/AssetBundles";

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class HotUpdate : MonoBehaviour
{
    byte[] m_ReadPathFileListData;
    byte[] m_ServerFileListData;
    internal class DownFileInfo
    {
        public string url;
        public string fileName;
        public DownloadHandler fileData;
    }
    /// <summary>
    /// 下载单个文件
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(DownFileInfo info, Action<DownFileInfo> complete)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        yield return webRequest.SendWebRequest();
        if(webRequest.result == UnityWebRequest.Result.ProtocolError 
            || webRequest.result == UnityWebRequest.Result.ConnectionError
            || webRequest.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogErrorFormat("下载文件出错: result:{0} url:{1} error:{2}", webRequest.result, info.url, webRequest.error);
            yield break;
            //重试
        }
        info.fileData = webRequest.downloadHandler;
        complete?.Invoke(info);
        webRequest.Dispose();
    }

    /// <summary>
    /// 下载多个文件
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="complete"></param>
    /// <param name="DownLoadAllComplete"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(List<DownFileInfo> infos, Action<DownFileInfo> complete, Action DownLoadAllComplete)
    {
        foreach (DownFileInfo info in infos)
        {
            yield return DownLoadFile(info, complete);
        }
        DownLoadAllComplete?.Invoke();
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileData, string path)
    {
        string content = fileData.Trim().Replace("\r", "");
        string[] files = content.Split("\n");
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        for(int i = 0; i < files.Length; i++)
        {
            string[] info = files[i].Split("|");
            DownFileInfo fileInfo = new DownFileInfo();
            fileInfo.fileName = info[1];
            fileInfo.url = Path.Combine(path, info[1]);
            downFileInfos.Add(fileInfo);
        }
        return downFileInfos;
    }

    private void Start()
    {
        if (IsFirstInstall())
        {
            ReleaseResources();
        }
        else
        {
            CheckUpdate();
        }
    }

    private bool IsFirstInstall()
    {
        //判断只读目录是否存在版本文件
        bool isExistsReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadPath, AppConst.FileListName));

        //判断可读写目录是否存在版本文件
        bool isExistsReadWritePath = FileUtil.IsExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));

        return  isExistsReadPath && !isExistsReadWritePath;
    }

    private void ReleaseResources()
    {
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownLoadFile(info, OnDownLoadReadPathFileListComplete));

    }

    private void OnDownLoadReadPathFileListComplete(DownFileInfo file)
    {
        m_ReadPathFileListData = file.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        StartCoroutine(DownLoadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));
    }

    private void OnReleaseFileComplete(DownFileInfo fileInfo)
    {
        Debug.Log("OnReleaseFileComplete:" + fileInfo.url);
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    private void OnReleaseAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdate();
    }

    private void CheckUpdate()
    {
        string url = Path.Combine(AppConst.ResourcesUrl, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownLoadFile(info, OnDownLoadServerFileListComplete));
    }

    private void OnDownLoadServerFileListComplete(DownFileInfo file)
    {
        m_ServerFileListData = file.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResourcesUrl);
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        for (int i = 0; i < fileInfos.Count; i++)
        {
            string localPath = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            if (!FileUtil.IsExists(localPath))
            {
                fileInfos[i].url = Path.Combine(AppConst.ResourcesUrl, fileInfos[i].fileName);
                downListFiles.Add(fileInfos[i]);
            }
        }
        if (downListFiles.Count > 0)
            StartCoroutine(DownLoadFile(downListFiles, OnUpdateFileComplete, OnUpdateAllFileComplete));
        else
            EnterGame();
    }

    private void OnUpdateFileComplete(DownFileInfo file)
    {
        Debug.Log("OnUpdateFileComplete:" + file.url);
        string writeFile = Path.Combine(PathUtil.ReadWritePath, file.fileName);
        FileUtil.WriteFile(writeFile, file.fileData.data);
    }

    private void OnUpdateAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ServerFileListData);
        EnterGame();
    }

    private void EnterGame()
    {
        Manager.Resource.ParseVersionFile();
        Manager.Resource.LoadUI("TestUI", OnComplete);
    }

    private void OnComplete(UnityEngine.Object obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}

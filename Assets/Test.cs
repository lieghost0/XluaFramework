using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(PathUtil.BundleOutPath + "/ui/prefabs/testui.prefab.ab");
        yield return request;
        AssetBundleCreateRequest request1 = AssetBundle.LoadFromFileAsync(PathUtil.BundleOutPath + "/ui/res/bg01.png.ab");
        yield return request1;
        AssetBundleCreateRequest request2 = AssetBundle.LoadFromFileAsync(PathUtil.BundleOutPath + "/ui/res/ui_btton_2.png.ab");
        yield return request2;
        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync("Assets/BuildResources/UI/Prefabs/TestUI.prefab");
        yield return bundleRequest;

        GameObject go = Instantiate(bundleRequest.asset) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

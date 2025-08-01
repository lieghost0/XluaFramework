using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    //缓存UI
    Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();
    //ui分组
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();

    private Transform m_UIParent;

    private void Awake()
    {
        m_UIParent = this.transform.parent.Find("Entity");
    }

    public void SetEntityGroup(List<string> groups)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            GameObject go = new GameObject("Group-" + groups[i]);
            go.transform.SetParent(m_UIParent, false);
            m_UIGroups.Add(groups[i], go.transform);
        }
    }

    Transform GetEntityGroup(string group)
    {
        if (!m_UIGroups.ContainsKey(group))
            Debug.LogError("group is not exist");
        return m_UIGroups[group];
    }

    public void ShowEntity(string name, string group, string luaName)
    {
        GameObject ui = null;
        if (m_UI.TryGetValue(name, out ui))
        {
            EntityLogic entityLogic = ui.GetComponent<EntityLogic>();
            entityLogic.OnShow();
            return;
        }

        Manager.Resource.LoadPrefab(name, (UnityEngine.Object obj) =>
        {
            GameObject ui = Instantiate(obj) as GameObject;
            m_UI.Add(name, ui);

            Transform parent = GetEntityGroup(group);
            ui.transform.SetParent(parent, false);

            EntityLogic entityLogic = ui.AddComponent<EntityLogic>();
            entityLogic.Init(luaName);
            entityLogic.OnShow();
        });
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TalentLoader : MonoBehaviour
{
    public static TalentLoader Instance;
    public TextAsset jsonFile;
    
    // 最终的数据仓库，方便用 ID 查天赋
    public Dictionary<string, Talent> talentDict = new Dictionary<string, Talent>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Load();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            DeBug();
        }
    }

    public void Load()
    {
        if (jsonFile == null) return;

        // 原始加载：此时 preTanlents 里只有字符串 ID
        TalentRoot root = JsonUtility.FromJson<TalentRoot>(jsonFile.text);
        
        // 构建字典：为了方便查找，把 List 转为 Dictionary
        foreach (var t in root.talents)
        {
            if (!talentDict.ContainsKey(t.id))
            {
                talentDict.Add(t.id, t);
            }
        }

        // 遍历所有天赋，根据 preTanlents 里的 ID，去字典里找到对应的对象
        foreach (var t in root.talents)
        {
            // 初始化运行时列表
            t.preTalentObjects = new List<Talent>();

            // 遍历 ID 列表
            foreach (string reqId in t.preTanlents)
            {
                if (talentDict.ContainsKey(reqId))
                {
                    // 把找到的真对象加进去
                    t.preTalentObjects.Add(talentDict[reqId]);
                }
                else
                {
                    Debug.LogWarning($"天赋 {t.name} 的前置天赋ID {reqId} 不存在！");
                }
            }
        }

        Debug.Log("天赋加载并连接完成！" + talentDict.Count + " 个天赋已加载。");
    }

    public Talent GetTalentById(string id)
    {
        if (talentDict.ContainsKey(id))
        {
            return talentDict[id];
        }
        else
        {
            Debug.LogWarning($"请求的天赋ID {id} 不存在！");
            return null;
        }
    }

    public void DeBug()
    {
        string firstName = "无";
        if (talentDict.Count > 0)
        {
            firstName = talentDict.Values.First().name;
        }
        Debug.Log("当前已加载的天赋数：" + talentDict.Count + " 第一个天赋名称：" + firstName);
    }
}
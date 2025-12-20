using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class MissionCondition
{
    public string type;    
    public int[] paramList;   // 参数列表
}

[Serializable]
public class MissionData
{
    public string id;
    public string name;
    public string description;
    public int rewardTalent;
    public int rewardPolicyId;
    public int[] preMissionIds;
    public List<MissionCondition> conditions; // 检测条件
}

public class MissionManager : MonoBehaviour
{

    public static MissionManager instance;

    public List<MissionData> missionList = new List<MissionData>();
    public TextAsset missionJson;

    private void Awake()
    {
        instance = this; 
        LoadMissions();
    }

    void LoadMissions()
    {
        MissionData[] allMissions = JsonHelper.FromJson<MissionData>(missionJson.text);
        missionList.AddRange(allMissions);

        Debug.Log($"[MissionManager] 加载了 {missionList.Count} 个Mission");

        foreach (var m in missionList)
        {
            Debug.Log($"[MissionManager] 加载Mission: ID={m.id}, Name={m.name} Condition: count:{m.conditions.Count} ");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

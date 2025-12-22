using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[Serializable]
public class MissionData
{
    public string id;
    public string name;
    public string description;
    public int rewardTalent;
    public int rewardPolicyId;
    public List<int> preMissionIds;
    public List<MissionCondition> conditions; // 检测条件
}

public class MissionManager : MonoBehaviour
{

    public static MissionManager instance;
    
    private GameStatistics statistics;

    public List<MissionData> missionList = new List<MissionData>();
    public TextAsset missionJson;

    //任务是否完成
    private Dictionary<string, bool> isComplete = new Dictionary<string, bool>();

    private void Awake()
    {
        instance = this; 
        LoadMissions();
    }

    private void Start()
    {
        statistics = GameControl.Instance.gameStatistics;
    }

    void LoadMissions()
    {
        MissionData[] allMissions = JsonHelper.FromJson<MissionData>(missionJson.text);
        missionList.AddRange(allMissions);

        Debug.Log($"[MissionManager] 加载了 {missionList.Count} 个Mission");
        ConvertConditionsToDerived();
        foreach (var m in missionList)
        {
            Debug.Log($"[MissionManager] 加载Mission: ID={m.id}, Name={m.name} Condition: count:{m.conditions.Count} ");
        }
    }

    void ConvertConditionsToDerived()
    {

        foreach (var mission in missionList)
        {
            if (mission.conditions == null) continue;


            for (int i = 0; i < mission.conditions.Count; i++)
            {

                MissionCondition raw = mission.conditions[i];


                MissionCondition derived = CreateDerivedCondition(raw.type);

                if (derived != null)
                {
                    derived.type = raw.type;
                    derived.paramList = raw.paramList;
                    mission.conditions[i] = derived;
                }
            }
        }
        Debug.Log("所有 MissionCondition 已成功替换为对应的派生类实例。");
    }

    private MissionCondition CreateDerivedCondition(string type)
    {
        return type switch
        {
            "currentReign" => new CurrentReignCondition(),
            "totalReign" => new TotalReignCondition(),
            "runsWithLongReign" => new LongReignRunsCondition(),
            "policyUsageCount" => new PolicyUsageCondition(),
            "survivalAfterPolicy" => new SurvivalPolicyCondition(),
            "survivalAfterOption" => new SurvivalOptionCondition(),
            "requiredEventFlags" => new EventFlagsCondition(),
            _ => null
        };
    }

    bool CheckPreMissions(List<int> preMissions)
    {
        foreach(var premission in preMissions)
        {
            string preId = premission.ToString("000");
            if(isComplete.ContainsKey(preId) == false || isComplete[preId] == false)
            {
                return false;
            }
        }
        return true;
    }

}

[Serializable]
public class MissionCondition
{
    public string type;
    public int[] paramList;   // 参数列表

    public virtual bool CheckComplete(GameStatistics gameStatistics)
    {
        return false;
    }
}


public class CurrentReignCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        return gameStatistics.currentReignYears >= paramList[0];
    }
}


public class TotalReignCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        return gameStatistics.totalReginYears >= paramList[0];
    }
}


public class LongReignRunsCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        return paramList[0] <= 0;
    }
}


public class PolicyUsageCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        if (gameStatistics.policyUsageCount.ContainsKey(paramList[1].ToString("000")) == false) return false;
        return gameStatistics.policyUsageCount[paramList[1].ToString("000")] >= paramList[0];
    }
}


public class SurvivalPolicyCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        if (gameStatistics.policyFirstYear.ContainsKey(paramList[1].ToString("000")) == false) return false;
        return gameStatistics.currentReignYears - gameStatistics.policyFirstYear[paramList[1].ToString("000")] >= paramList[0];
    }
}


public class SurvivalOptionCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        if (gameStatistics.judgeFirstYear[paramList[1]] == -1) return false;
        return gameStatistics.currentReignYears - gameStatistics.judgeFirstYear[paramList[1]] >= paramList[0];
    }
}


public class EventFlagsCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        foreach(var id in paramList)
        {
            if (gameStatistics.judgeValue[id] == false) return false;
        }
        return true;
    }
}
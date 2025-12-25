using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;




[Serializable]
public class MissionData
{
    public string id;
    public string name;
    public string description;
    public int rewardTalent;
    public int rewardPolicyId;
    public int[] randomEventSet;
    public List<int> preMissionIds;
    public List<MissionCondition> conditions; // 检测条件

    public bool CheckComplete(GameStatistics gs)
    {
        foreach(var con in conditions)
        {
            if(con.CheckComplete(gs) == false)
            {
                return false;
            }
        }
        return true;
    }

    public bool CheckPreMissions()
    {
        foreach (var premission in preMissionIds)
        {
            if (!MissionManager.Instance.isComplete(premission))
            {
                return false;
            }
        }
        return true;
    }

    public void MissionComplete()
    {
        /*
         * Undo: 任务做完的逻辑
         */
    }
}

public class MissionManager : MonoBehaviour
{

    public static MissionManager Instance;
    
    private GameStatistics statistics;

    public List<MissionData> missionList = new List<MissionData>();

    /*
      当前已激活的任务
      目前采用遍历missionList的方式发放后续任务
      可以用建图拓扑的方式优化
      
      现在存储已激活任务是missionList中的下标
    */
    public List<int> activeMissions;
    public TextAsset missionJson;

    //任务是否完成
    private Dictionary<int, bool> _isComplete;

    private void Awake()
    {
        Instance = this; 
        LoadMissions();
    }

    private void Start()
    {
        statistics = GameControl.Instance.gameStatistics;
        activeMissions = statistics.activeMissions;
        _isComplete = statistics.isComplete;
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

    public bool isComplete(int id)
    {
        return _isComplete.TryGetValue(id, out bool res) ? res : false;
    }

    public bool isComplete(string id)
    {
        int newId = int.Parse(id);
        return isComplete(newId);
    }

    public MissionData GetTaskDataById(int id)
    {
        if (id <= 0) return null;
        return missionList[id - 1];
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
            "policyUseOutCount" => new PolicyUseOutCondition(),
            _ => null
        };
    }

    public void CheckComplete()
    {
        foreach(var m in activeMissions)
        {
            if (missionList[m].CheckComplete(statistics))
            {
                missionList[m].MissionComplete();
                _isComplete[int.Parse(missionList[m].id)] = true;
            }
        }
    }

    public void Dispatch()
    {
        foreach(var m in missionList)
        {
            if(!isComplete(int.Parse(m.id)) && m.CheckPreMissions())
            {
                activeMissions.Add(int.Parse(m.id) - 1);
            }
        }
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
        return gameStatistics.GetPolicyUsage(paramList[1]) >= paramList[0];
    }
}


public class SurvivalPolicyCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        int firstYear = gameStatistics.GetPolicyFirstYear(paramList[1]);
        return firstYear != -1 && (gameStatistics.currentReignYears - firstYear) >= paramList[0];
    }
}


public class SurvivalOptionCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        int firstYear = gameStatistics.judgeFirstYear[paramList[1]];
        return firstYear != -1 && (gameStatistics.currentReignYears - firstYear) >= paramList[0];
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

public class PolicyUseOutCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics)
    {
        return gameStatistics.policyUseOutCount > paramList[0];
    }
}
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
    public string[] rewardPolicyId;
    public int randomEventSet;
    public List<string> preMissionIds;
    public List<MissionCondition> conditions; // 检测条件

    public bool CheckComplete(GameStatistics gs)
    {
        foreach(var con in conditions)
        {
            if(con.CheckComplete(gs, id) == false)
            {
                return false;
            }
        }
        Debug.Log("ID: " + id + " is Completed");
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

    public bool MissionComplete()
    {
        if(rewardPolicyId != null)
        {
            int count = rewardPolicyId.Length;

            if(GameControl.Instance.stats.isBagFull(count))
            {
                return false;
            }

            //发放道具
            foreach(var id in rewardPolicyId)
            {
                PolicyItem newItem = PolicyManager.Instance.GetPolicy(id);
                GameControl.Instance.AddPolicy(newItem);
            }
        }
        
        //获得天赋点
        TalantManager.Instance.AddTalentPoints(rewardTalent);

        //事件集
        if (randomEventSet != 0)
        {
            EventDatabase.Instance?.ActivateEventSetById(randomEventSet);
            Debug.Log($"[OptionEffectHandler] 更新激活事件集: {randomEventSet}");
        }

        GameControl.Instance.gameStatistics.UnRegister(id);

        return true;
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
    //这个List为了防止Bug：这一轮新dispatch的任务会用上一局的数据进行check
    public List<int> newActiveMissions = new List<int>();
    public TextAsset missionJson;

    //任务是否完成
    private Dictionary<string, bool> _isComplete;

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

        //游戏开始的时候发任务
        Dispatch();
    }

    public void StartNewGame()
    {
        newActiveMissions.Clear();

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

    public bool isComplete(string id)
    {
        return _isComplete.TryGetValue(id, out var result) ? result : false;
    }

    public MissionData GetTaskDataById(int id)
    {
        if (id < 0) return null;
        return missionList[id];
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
            if (!newActiveMissions.Contains(m) && missionList[m].CheckComplete(statistics))
            {
                _isComplete[missionList[m].id] = true;
            }
        }
    }

    public void Dispatch()
    {
        foreach(var m in missionList)
        {
            if(!isComplete(m.id) && m.CheckPreMissions() && !activeMissions.Contains(int.Parse(m.id) - 1))
            {
                Debug.Log("[MissionManager] 任务" + m.id + " 已发放");
                activeMissions.Add(int.Parse(m.id) - 1);
                newActiveMissions.Add(int.Parse(m.id) - 1);
                statistics.Register(m);
            }
        }
    }
    
    //通过id进行任务奖励的领取
    public void Reward(int id)
    {
        bool flag = missionList[id - 1].MissionComplete();

        if(!flag)
        {
            return;
        }
        Debug.Log("[MissionManager] 任务：" + id + "已完成  奖励已领取");

        activeMissions.Remove(id - 1);

        //发新的任务
        Dispatch();
            
    }
}

[Serializable]
public class MissionCondition
{
    public string type;
    public int[] paramList;   // 参数列表

    public virtual bool CheckComplete(GameStatistics gameStatistics, string id=null)
    {
        return false;
    }
}


public class CurrentReignCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics, string id = null)
    {
        Debug.Log(gameStatistics.currentReignYears + "    " + paramList[0]);
        return gameStatistics.currentReignYears >= paramList[0];
    }
}


public class TotalReignCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics, string id = null)
    {
        return gameStatistics.totalReginYears >= paramList[0];
    }
}


public class LongReignRunsCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics, string id = null)
    {
        return gameStatistics.runsWithLongReign[id].y >= paramList[0];
    }
}


public class PolicyUsageCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics, string id = null)
    {
        return gameStatistics.GetPolicyUsage(paramList[1]) >= paramList[0];
    }
}


public class SurvivalPolicyCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics, string id = null)
    {
        int firstYear = gameStatistics.GetPolicyFirstYear(paramList[1]);
        return firstYear != -1 && (gameStatistics.currentReignYears - firstYear) >= paramList[0];
    }
}


public class SurvivalOptionCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics, string id = null)
    {
        int firstYear = gameStatistics.judgeFirstYear[paramList[1]];
        return firstYear != -1 && (gameStatistics.currentReignYears - firstYear) >= paramList[0];
    }
}


public class EventFlagsCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics, string id = null)
    {
        foreach(var pid in paramList)
        {
            if (gameStatistics.judgeValue[pid] == false) return false;
        }
        return true;
    }
}

public class PolicyUseOutCondition : MissionCondition
{
    public override bool CheckComplete(GameStatistics gameStatistics, string id = null)
    {
        return gameStatistics.policyUseOutCount >= paramList[0];
    }
}